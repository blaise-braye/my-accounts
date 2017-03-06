namespace Operations.Classification.GeoLoc
{
    public class GeoCoding
    {
        public string FreeText { get; set; }
        
        public string FormattedAddress { get; set; }

        public string[] Tags { get; set; }

        public string Route { get; set; }

        public string StreetNumber { get; set; }

        public string PostalCode { get; set; }

        public string SubLocality { get; set; }

        public string Locality { get; set; }

        public string Country { get; set; }

        public string AdministrativeAreaLevel1 { get; set; }

        public string AdministrativeAreaLevel2 { get; set; }
    }
}