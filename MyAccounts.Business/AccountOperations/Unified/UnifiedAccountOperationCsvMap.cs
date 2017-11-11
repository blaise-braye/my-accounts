using CsvHelper.Configuration;

namespace MyAccounts.Business.AccountOperations.Unified
{
    public sealed class UnifiedAccountOperationCsvMap : ClassMap<UnifiedAccountOperation>
    {
        public UnifiedAccountOperationCsvMap()
        {
            AutoMap();
            Map(m => m.ValueDate).TypeConverterOption.Format("dd/MM/yyyy");
        }
    }
}