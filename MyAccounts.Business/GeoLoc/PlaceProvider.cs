using System.Globalization;
using System.Linq;
using System.Text;
using Operations.Classification.Collections;

namespace Operations.Classification.GeoLoc
{
    public class PlaceProvider
    {
        private readonly Trie _placesWordsTrie;
        private readonly ILookup<string, Place> _placesByWordLookup;

        public PlaceProvider(ILookup<string, Place> placesByWordLookup, Trie placesWordsTrie)
        {
            _placesByWordLookup = placesByWordLookup;
            _placesWordsTrie = placesWordsTrie;
        }

        public static PlaceProvider Load(PlacesRepository placesRepository)
        {
            var belgianPlaces = placesRepository.GetBelgianPlaces();
            var placesByWordLookup = placesRepository.GetPlacesByWordLookup();

            var placesWordsTrie = new Trie();
            placesWordsTrie.InsertRange(placesByWordLookup.Select(grp => grp.Key));
            placesWordsTrie.InsertRange(belgianPlaces.Select(p => RemoveDiacritics(p.Locality).ToUpperInvariant()));
            placesWordsTrie.InsertRange(belgianPlaces.Select(p => RemoveDiacritics(p.Province).ToUpperInvariant()));

            return new PlaceProvider(placesByWordLookup, placesWordsTrie);
        }

        public bool IsAnyPlaceEndingWith(string placeSuffix)
        {
            return _placesWordsTrie.Search(placeSuffix);
        }

        public Place GetCustomPlaceFromWord(string word)
        {
            return _placesByWordLookup[word].SingleOrDefault();
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
    }
}