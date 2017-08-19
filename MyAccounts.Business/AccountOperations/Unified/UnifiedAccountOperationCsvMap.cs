using CsvHelper.Configuration;

namespace Operations.Classification.AccountOperations.Unified
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