using System;
using System.Collections.Generic;
using System.Linq;

using Geocoding.Google;

namespace Operations.Classification.GeoLoc
{
    public class GeoCodingSpike
    {
        public GeoCoding FindNearestZipCodeByCityKnowingCityIsAtEndOfText(string freetext)
        {
            var text = new GeoCoding { FreeText = freetext };

            GoogleGeocoder geocoder = new GoogleGeocoder();

            List<GoogleAddress> addresses = new List<GoogleAddress>();

            var freeTextParts = freetext.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < freeTextParts.Length - 1 && !addresses.Any(); i++)
            {
                var query = string.Join(" ", freeTextParts.Skip(i));
                addresses = geocoder.Geocode(query).ToList();
            }

            var address = addresses.OrderBy(a => a.IsPartialMatch ? 0 : 1).FirstOrDefault();

            if (address != null)
            {
                var formattedAddress = address.FormattedAddress;

                var componentsByType = address.Components.SelectMany(c => c.Types.Select(t => new { c, t })).ToLookup(c => c.t, c => c.c);

                var country = componentsByType[GoogleAddressType.Country].FirstOrDefault()?.LongName;
                var route = componentsByType[GoogleAddressType.Route].FirstOrDefault()?.LongName;
                var streetNumber = componentsByType[GoogleAddressType.StreetNumber].FirstOrDefault()?.LongName;
                var postalCode = componentsByType[GoogleAddressType.PostalCode].FirstOrDefault()?.LongName;
                var administrativeAreaLevel1 = componentsByType[GoogleAddressType.AdministrativeAreaLevel1].FirstOrDefault()?.LongName;
                var administrativeAreaLevel2 = componentsByType[GoogleAddressType.AdministrativeAreaLevel1].FirstOrDefault()?.LongName;
                var sublocality = componentsByType[GoogleAddressType.SubLocality].FirstOrDefault()?.LongName;
                var locality = componentsByType[GoogleAddressType.Locality].FirstOrDefault()?.LongName;

                text.FormattedAddress = formattedAddress;
                text.Locality = locality;
                text.SubLocality = sublocality;
                text.Route = route;
                text.StreetNumber = streetNumber;
                text.PostalCode = postalCode;
                text.Country = country;
                text.AdministrativeAreaLevel1 = administrativeAreaLevel1;
                text.AdministrativeAreaLevel2 = administrativeAreaLevel2;
            }

            return text;
        }
    }
}