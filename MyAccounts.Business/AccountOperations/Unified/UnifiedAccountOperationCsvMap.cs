using CsvHelper.Configuration;

namespace MyAccounts.Business.AccountOperations.Unified
{
    public sealed class UnifiedAccountOperationCsvMap : CsvClassMap<UnifiedAccountOperation>
    {
        public UnifiedAccountOperationCsvMap()
        {
            AutoMap();
            Map(m => m.ValueDate).TypeConverterOption("dd/MM/yyyy");
        }
    }
}