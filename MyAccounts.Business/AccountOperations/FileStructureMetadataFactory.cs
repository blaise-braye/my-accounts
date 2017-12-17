using MyAccounts.Business.AccountOperations.Contracts;

namespace MyAccounts.Business.AccountOperations
{
    public class FileStructureMetadataFactory
    {
        public static FileStructureMetadata CreateDefault(SourceKind sourceKind)
        {
            var metadata = new FileStructureMetadata { SourceKind = sourceKind };

            if (CsvAccountOperationManager.DefaultCsvConfigurations.ContainsKey(sourceKind))
            {
                var defaultConfig = CsvAccountOperationManager.DefaultCsvConfigurations[sourceKind];
                metadata.Encoding = defaultConfig.Encoding.WebName;
                metadata.Culture = defaultConfig.CultureInfo.Name;
                metadata.DecimalSeparator = defaultConfig.CultureInfo.NumberFormat.CurrencyDecimalSeparator;
            }

            return metadata;
        }
    }
}