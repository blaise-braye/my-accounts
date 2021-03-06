using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using log4net;
using MyAccounts.Business.Properties;
using Newtonsoft.Json;

namespace MyAccounts.Business.GeoLoc
{
    public class PlacesRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PlacesRepository));

        public ILookup<string, Place> GetPlacesByWordLookup()
        {
            var places = GetCustomPlaces();
            var wordFlatifiedPlaces = places.Select(l => Tuple.Create(l, l.Name))
                .Union(places.SelectMany(l => l.Abbrevs.Select(abbrev => Tuple.Create(l, abbrev))));

            var placesByWordLookup = wordFlatifiedPlaces.ToLookup(s => s.Item2, s => s.Item1);
            return placesByWordLookup;
        }

        public List<Place> GetCustomPlaces()
        {
            var places = JsonConvert.DeserializeObject<List<Place>>(Resources.Places);
            return places;
        }

        public List<BelgianPlace> GetBelgianPlaces()
        {
            List<BelgianPlace> belgianPlaces;
            var config = new CsvHelper.Configuration.Configuration();
            config.RegisterClassMap<BelgianPlaceCsvMap>();

            StringReader sr = null;
            try
            {
                sr = new StringReader(Resources.zipcodes_alpha_beligum);
                using (var reader = new CsvReader(sr, config))
                {
                    sr = null;
                    belgianPlaces = reader.GetRecords<BelgianPlace>().ToList();
                }
            }
            catch (Exception exn)
            {
                _logger.Error("could not parse belgian places, empty list will be used instead", exn);
                belgianPlaces = new List<BelgianPlace>();
            }
            finally
            {
                sr?.Dispose();
            }

            return belgianPlaces;
        }
    }
}