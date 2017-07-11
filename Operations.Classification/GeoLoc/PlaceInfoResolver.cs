using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Operations.Classification.Collections;
using Operations.Classification.Properties;

namespace Operations.Classification.GeoLoc
{
    public class PlaceInfoResolver
    {
        private const int ExpectedMaximumSpacesInOneLocalisationWord = 2;

        private readonly List<BelgianPlace> _belgianPlaces;

        private readonly ILookup<string, Place> _placesByWordLookup;

        private readonly Trie _placesWordsTrie;

        public PlaceInfoResolver()
        {
            _placesWordsTrie = new Trie();

            var places = JsonConvert.DeserializeObject<List<Place>>(Resources.Places);

            var config = new CsvConfiguration();
            config.RegisterClassMap<BelgianPlaceCsvMap>();

            StringReader sr = null;
            try
            {
                sr = new StringReader(Resources.zipcodes_alpha_beligum);
                using (var reader = new CsvReader(sr, config))
                {
                    sr = null;
                    _belgianPlaces = reader.GetRecords<BelgianPlace>().ToList();
                }
            }
            finally
            {
                sr?.Dispose();
            }

            var wordFlatifiedPlaces = places.Select(l => Tuple.Create(l, l.Name))
                .Union(places.SelectMany(l => l.Abbrevs.Select(abbrev => Tuple.Create(l, abbrev))));

            _placesByWordLookup = wordFlatifiedPlaces.ToLookup(s => s.Item2, s => s.Item1);
            _placesWordsTrie.InsertRange(_placesByWordLookup.Select(grp => grp.Key));
            _placesWordsTrie.InsertRange(_belgianPlaces.Select(p => RemoveDiacritics(p.Locality).ToUpper()));
            _placesWordsTrie.InsertRange(_belgianPlaces.Select(p => RemoveDiacritics(p.Province).ToUpperInvariant()));
        }

        public PlaceInfo ResolveKnowingPlaceInfoIsAtEndOfText(string freetext, bool containsAdressInfo)
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
                    var cityPlace = _placesByWordLookup[cityWord].SingleOrDefault();
                    result.City = cityPlace?.Name ?? cityWord;
                    result.SetNotRelatedToPlaceInfo(cityStartIndex, freetext.Length - cityStartIndex);
                }
            }

            return result;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private string GetAdressFromEndOfInput(string input, out int localisationIndex)
        {
            var cityWord = GetCityFromEndOfInput(input, out localisationIndex);

            var streetPrefixes = new List<string> { " AVENUE", " RUE", " VOIE", " CHEMIN", " BLD", " BD", " B.D.", " AV", "CHAUSSEE", " AV.", " RTE" };
            if (!string.IsNullOrEmpty(cityWord))
            {
                var city = _placesByWordLookup[cityWord].SingleOrDefault();
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
                var word = string.Join(" ", words.Skip(words.Length - i));
                var trimmedWord = word.Trim('/', '-', '.', ' ');
                if (_placesWordsTrie.Search(trimmedWord))
                {
                    localisationIndex = input.Length - word.Length;
                    return trimmedWord;
                }
            }

            localisationIndex = -1;
            return string.Empty;
        }
    }
}