using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Operations.Classification.Collections;
using Operations.Classification.Properties;

namespace Operations.Classification.GeoLoc
{
    public class PlaceInfoResolver
    {
        private const int ExpectedMaximumSpacesInOneLocalisationWord = 2;

        private readonly Trie _placesWordsTrie;

        private readonly ILookup<string, Place> _placesByWordLookup;

        public PlaceInfoResolver()
        {
            _placesWordsTrie = new Trie();

            var places = JsonConvert.DeserializeObject<List<Place>>(Resources.Places);

            IEnumerable<Tuple<Place, string>> wordFlatifiedPlaces =
                places.Select(l => Tuple.Create(l, l.Name)).Union(places.SelectMany(l => l.Abbrevs.Select(abbrev => Tuple.Create(l, abbrev))));

            _placesByWordLookup = wordFlatifiedPlaces.ToLookup(s => s.Item2, s => s.Item1);
            _placesWordsTrie.InsertRange(_placesByWordLookup.Select(grp => grp.Key));
        }

        public PlaceInfo ResolveKnowingPlaceInfoIsAtEndOfText(string freetext, bool containsAdressInfo)
        {
            var result = new PlaceInfo(freetext);

            if (containsAdressInfo)
            {
                int addressStartIndex;
                var address = GetAdressFromEndOfInput(freetext, out addressStartIndex);
                string city = string.Empty;

                if (!string.IsNullOrEmpty(address))
                {
                    result.SetNotRelatedToPlaceInfo(addressStartIndex, freetext.Length - addressStartIndex);

                    int cityStartIndex;
                    city = GetCityFromEndOfInput(address, out cityStartIndex);
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
                int cityStartIndex;
                var city = GetCityFromEndOfInput(freetext, out cityStartIndex);
                if (!string.IsNullOrEmpty(city))
                {
                    result.City = city;
                    result.SetNotRelatedToPlaceInfo(cityStartIndex, freetext.Length - cityStartIndex);
                }
            }

            return result;
        }
        
        private string GetCityFromEndOfInput(string input, out int localisationIndex)
        {
            var words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var maxToTake = ExpectedMaximumSpacesInOneLocalisationWord + 1;
            if (words.Length > maxToTake)
            {
                words = words.Skip(words.Length - maxToTake).ToArray();
            }

            for (int i = words.Length; i > 0; i--)
            {
                var word = string.Join(" ", words.Skip(words.Length - i));
                if (_placesWordsTrie.Search(word))
                {
                    localisationIndex = input.Length - word.Length;
                    return word;
                }
            }

            // if not found special cases' knowledge, take the latest word
            var fallbackWord = words[words.Length - 1];
            localisationIndex = input.Length - fallbackWord.Length;

            // trim the fallback word (cleanup of extensions)
            fallbackWord = fallbackWord.Trim('/');

            return fallbackWord;
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
    }
}
