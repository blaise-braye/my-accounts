using CsvHelper.Configuration;
using MyAccounts.Business.AccountOperations.Contracts;

namespace MyAccounts.Business.AccountOperations.Sodexo
{
    public sealed class SodexoOperationCsvMap : ClassMap<SodexoOperation>
    {
        public SodexoOperationCsvMap()
        {
            Map(m => m.Date).Name("Date").TypeConverterOption.Format("dd'-'MM'-'yyyy");
            Map(m => m.Amount).Name("Amount");
            Map(m => m.Detail).Name("Affilié");
            Map(m => m.SourceKind).Default(SourceKind.Unknwon);
        }
    }
}