using System;
using System.Linq;

namespace Operations.Classification.GeoLoc
{
    public class Place
    {
        private string[] _abbrev;

        private string[] _streets;

        public string Name { get; set; }

        public string[] Abbrevs
        {
            get
            {
                return _abbrev ?? (_abbrev = Array.Empty<string>());
            }

            set
            {
                _abbrev = value;
            }
        }

        public string[] Streets
        {
            get
            {
                return _streets ?? (_streets = Array.Empty<string>());
            }

            set
            {
                _streets = value;
            }
        }
    }
}