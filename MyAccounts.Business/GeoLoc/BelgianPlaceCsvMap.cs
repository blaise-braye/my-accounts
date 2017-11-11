using CsvHelper.Configuration;

namespace MyAccounts.Business.GeoLoc
{
    public sealed class BelgianPlaceCsvMap : ClassMap<BelgianPlace>
    {
        public BelgianPlaceCsvMap()
        {
            Map(m => m.PostalCode).Name("Code postal");
            Map(m => m.Locality).Name("Localité");
            Map(m => m.SubCommune).Name("Sous-commune");
            Map(m => m.Province).Name("Province");
            Map(m => m.Language).Name("Langue");
        }
    }
}