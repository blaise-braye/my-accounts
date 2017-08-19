namespace MyAccounts.Business.GeoLoc
{
    public class BelgianPlace
    {
        public string PostalCode { get; set; }

        public string Locality { get; set; }

        public string SubCommune { get; set; }

        public string Province { get; set; }

        public string Language { get; set; }

        public override string ToString()
        {
            return $"{PostalCode} {Locality} {Province}";
        }
    }
}