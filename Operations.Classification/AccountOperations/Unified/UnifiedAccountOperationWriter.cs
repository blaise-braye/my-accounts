using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

using QifApi.Config;
using QifApi.Transactions;

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

        public void WriteQif(string targetFilePath, IEnumerable<UnifiedAccountOperation> operations)
        {
            var qifDom = new QifApi.QifDom();

            foreach (var operation in operations)
            {
                var label = operation.Note;

                if (!string.IsNullOrEmpty(operation.PatternName))
                {
                    var humanFriendlyParts =
                        new List<string>()
                                {
                                    operation.ThirdParty,
                                    operation.Address,
                                    operation.City,
                                    operation.IBAN,
                                    operation.BIC?.Insert(0, "BIC "),
                                    operation.Communication?.Insert(0, "COMMUNICATION : "),
                                    operation.PatternName
                                }.Where(s => !string.IsNullOrWhiteSpace(s))
                            .Select(s => s.Trim());
                    label = string.Join(" - ", humanFriendlyParts);
                }

                var basicTransaction = new BasicTransaction()
                {
                    Number = operation.OperationId,
                    Amount = operation.Income - operation.Outcome,
                    Date = operation.ValueDate,
                    Memo = label,
                };
                qifDom.BankTransactions.Add(basicTransaction);
            }

            qifDom.Configuration.WriteDateFormatMode = WriteDateFormatMode.Custom;
            qifDom.Configuration.CustomWriteDateFormat = "dd'/'MM'/'yyyy";

            qifDom.Export(targetFilePath);
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
