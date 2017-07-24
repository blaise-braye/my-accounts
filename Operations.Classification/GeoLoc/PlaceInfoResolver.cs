using System;
using System.Collections.Generic;
using System.Linq;

namespace Operations.Classification.GeoLoc
{
    public class PlaceInfoResolver
    {
        private const int ExpectedMaximumSpacesInOneLocalisationWord = 2;

        private readonly PlaceProvider _structures;

        public PlaceInfoResolver(PlaceProvider structures)
        {
            _structures = structures;
        }

        public PlaceInfo ResolveFromEndOfText(string freetext, bool containsAdressInfo)
        {
            var result = new PlaceInfo(freetext);

            if (containsAdressInfo)
            {
                var address = GetAdressFromEndOfInput(freetext, out int addressStartIndex);
                var city = string.Empty;

                if (!string.IsNullOrEmpty(address))
                {
                    result.SetNotRelatedToPlaceInfo(addressStartIndex, freetext.Length - addressStartIndex);

                    city = GetCityFromEndOfInput(address, out int cityStartIndex);
                    if (!string.IsNullOrEmpty(city))
                    {
                        if (city.Equals(address))
                        {
                            address = string.Empty;
                        }
                        else
                        {
                            address = address.Remove(cityStartIndex - 1).TrimEnd();
                        }
                    }
                }

                result.City = city;
                result.Adress = address;
            }
            else
            {
                var cityWord = GetCityFromEndOfInput(freetext, out int cityStartIndex);
                if (!string.IsNullOrEmpty(cityWord))
                {
                    var cityPlace = _structures.GetCustomPlaceFromWord(cityWord);
                    result.City = cityPlace?.Name ?? cityWord;
                    result.SetNotRelatedToPlaceInfo(cityStartIndex, freetext.Length - cityStartIndex);
                }
            }

            return result;
        }

        private string GetAdressFromEndOfInput(string input, out int localisationIndex)
        {
            var cityWord = GetCityFromEndOfInput(input, out localisationIndex);

            var streetPrefixes = new List<string> { " AVENUE", " RUE", " VOIE", " CHEMIN", " BLD", " BD", " B.D.", " AV", "CHAUSSEE", " AV.", " RTE" };
            if (!string.IsNullOrEmpty(cityWord))
            {
                var city = _structures.GetCustomPlaceFromWord(cityWord);
                if (city != null)
                {
                    streetPrefixes.AddRange(city.Streets);
                }
            }

            var streetPrefixIndx = streetPrefixes.Select(sp => input.IndexOf(sp, StringComparison.Ordinal)).FirstOrDefault(i => i > 0);

            if (streetPrefixIndx > 0)
            {
                localisationIndex = streetPrefixIndx;
                var adress = input.Substring(localisationIndex, input.Length - localisationIndex);
                return adress.Trim();
            }

            // if address prefix not detected, let's return the city
            return cityWord;
        }

        private string GetCityFromEndOfInput(string input, out int localisationIndex)
        {
            var words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var maxToTake = ExpectedMaximumSpacesInOneLocalisationWord + 1;
            if (words.Length > maxToTake)
            {
                words = words.Skip(words.Length - maxToTake).ToArray();
            }

            for (var i = words.Length; i > 0; i--)
            {
                var placeSuffix = string.Join(" ", words.Skip(words.Length - i));
                var trimmedPlaceSuffix = placeSuffix.Trim('/', '-', '.', ' ');
                if (_structures.IsAnyPlaceEndingWith(trimmedPlaceSuffix))
                {
                    localisationIndex = input.Length - placeSuffix.Length;
                    return trimmedPlaceSuffix;
                }
            }

            localisationIndex = -1;
            return string.Empty;
        }
    }
}