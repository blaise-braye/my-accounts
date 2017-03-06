using System.Collections.Generic;

using Operations.Classification.GeoLoc;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Operations.Classification.Tests.Steps
{
    [Binding, Scope(Feature = "NormalizePlaceInformation")]
    public class NormalizePlaceInformationSteps
    {
        private readonly GeoCodingSpike _geoCodingRepository = new GeoCodingSpike();

        private readonly List<GeoCoding> _geoCodings = new List<GeoCoding>();
        
        [When(@"I geocode those entries, knowing the city information is at the end of the text")]
        public void WhenIGeocodeThoseEntriesKnowingTheCityInformationIsAtTheEndOfTheText(Table table)
        {
            foreach (var freetext in table.Rows)
            {
                var query = freetext[0];
                var geoCoding = _geoCodingRepository.FindNearestZipCodeByCityKnowingCityIsAtEndOfText(query);
                _geoCodings.Add(geoCoding);
            }
        }

        [Then(@"the geocoding results are")]
        public void ThenTheGeocodingResultsAre(Table expectedResults)
        {
            expectedResults.CompareToSet(_geoCodings);
        }
    }
}
