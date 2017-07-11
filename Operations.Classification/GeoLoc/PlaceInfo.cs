using System;
using System.Collections.Generic;
using System.Linq;

namespace Operations.Classification.GeoLoc
{
    public class PlaceInfo
    {
        private readonly List<Tuple<int, int>> _notPlaceInfoRelatedInFreeText = new List<Tuple<int, int>>();

        public PlaceInfo(string freeTextInput)
        {
            FreeTextInput = freeTextInput;
        }

        /// <summary>
        ///     the free text that has been used to resolve the place info
        /// </summary>
        public string FreeTextInput { get; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        public string Adress { get; set; }

        public string GetFreeTextWithoutPlaceInfo()
        {
            var flagTable = new bool[FreeTextInput.Length];
            foreach (var nottogoInOutput in _notPlaceInfoRelatedInFreeText.SelectMany(t => Enumerable.Range(t.Item1, t.Item2)))
            {
                flagTable[nottogoInOutput] = true;
            }

            var result = string.Concat(FreeTextInput.Where((c, idx) => !flagTable[idx]));

            return result.Trim();
        }

        public void SetNotRelatedToPlaceInfo(int position, int length)
        {
            _notPlaceInfoRelatedInFreeText.Add(Tuple.Create(position, length));
        }
    }
}