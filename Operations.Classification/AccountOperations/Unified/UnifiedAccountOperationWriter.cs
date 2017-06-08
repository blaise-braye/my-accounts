using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Operations.Classification.GererMesComptes;

namespace Operations.Classification.AccountOperations.Unified
{
    public class UnifiedAccountOperationWriter
    {
        public void WriteCsv(string targetFile, IEnumerable<UnifiedAccountOperation> operations)
        {
            var csvConfiguration = GetCsvConfiguration();
            using (var fs = File.Open(targetFile, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    using (var cw = new CsvWriter(sw, csvConfiguration))
                    {
                        cw.WriteHeader<UnifiedAccountOperation>();
                        cw.WriteRecords(operations);
                    }
                }
            }
        }

        public async Task WriteQif(string targetFilePath, IEnumerable<UnifiedAccountOperation> operations)
        {
            var qifData = operations.ToQifData();
            using (var fs = File.Open(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var sw = new StreamWriter(fs))
            {
                await sw.WriteAsync(qifData);
            }
        }

        private static CsvConfiguration GetCsvConfiguration()
        {
            var config = new CsvConfiguration();
            config.RegisterClassMap<UnifiedAccountOperationCsvMap>();
            config.Encoding = Encoding.UTF8;
            config.Delimiter = ";";
            config.TrimFields = true;
            config.TrimHeaders = true;
            config.QuoteAllFields = true;
            return config;
        }
    }
}