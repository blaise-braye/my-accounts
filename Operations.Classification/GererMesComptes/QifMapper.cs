using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Operations.Classification.AccountOperations.Unified;
using QifApi;
using QifApi.Config;
using QifApi.Transactions;

namespace Operations.Classification.GererMesComptes
{
    public static class QifMapper
    {
        private static readonly CultureInfo _englishCulture = new CultureInfo("en-US");

        private static readonly Regex _whiteSpaceRegex = new Regex(@"[ /_:\.]+", RegexOptions.Compiled);

        public static Configuration QifConfiguration => new Configuration
        {
            ReadDateFormatMode = ReadDateFormatMode.Custom,
            CustomReadDateFormat = "MM/dd/yyyy",
            WriteDateFormatMode = WriteDateFormatMode.Custom,
            CustomWriteDateFormat = "MM'/'dd'/'yyyy"
        };

        public static string BuildLabel(UnifiedAccountOperation operation)
        {
            var label = operation.Note;

            if (!string.IsNullOrEmpty(operation.PatternName))
            {
                var prioritizedParts =
                    new List<string>
                        {
                            operation.ThirdParty,
                            operation.Address,
                            operation.City,
                            operation.Communication,
                            operation.IBAN,
                            operation.BIC?.Insert(0, "BIC ")
                        }.Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => _whiteSpaceRegex.Replace(s, " ").Trim())
                        .Where(s => !string.IsNullOrEmpty(s)).ToArray();

                var patternNameLength = operation.PatternName.Length;
                var maxLength = 128 - patternNameLength;

                var sb = new StringBuilder();
                for (var i = 0; i < prioritizedParts.Length; i++)
                {
                    var toAdd = $"{prioritizedParts[i]} - ";
                    if (sb.Length + toAdd.Length <= maxLength)
                    {
                        sb.Append(toAdd);
                    }
                }

                sb.Append(operation.PatternName);

                label = sb.ToString();
                label = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(label.ToLowerInvariant());
            }

            return label;
        }

        public static QifDom ParseQifDom(string qifData)
        {
            using (new TemporaryCulture(_englishCulture))
            {
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(qifData));
                var qifDom = QifDom.ImportFile(new StreamReader(ms), QifConfiguration);
                return qifDom;
            }
        }

        public static IEnumerable<BasicTransaction> ToBasicTransactions(this IEnumerable<UnifiedAccountOperation> operations)
        {
            foreach (var operation in operations)
            {
                var basicTransaction = new BasicTransaction
                {
                    Number = operation.OperationId,
                    Amount = operation.Income - operation.Outcome,
                    Date = operation.ValueDate,
                    Memo = BuildLabel(operation)
                };

                yield return basicTransaction;
            }
        }

        public static string ToQifData(this IEnumerable<BasicTransaction> transactions)
        {
            var qifDom = new QifDom(QifConfiguration) { BankTransactions = transactions.ToList() };
            var tempFileName = Path.GetTempFileName();
            try
            {
                using (new TemporaryCulture(_englishCulture))
                {
                    qifDom.Export(tempFileName);
                }

                var qifData = File.ReadAllText(tempFileName);
                return qifData;
            }
            finally
            {
                if (File.Exists(tempFileName))
                {
                    try
                    {
                        File.Delete(tempFileName);
                    }
                    catch
                    {
                        /* ignored */
                    }
                }
            }
        }

        public static string ToQifData(this IEnumerable<UnifiedAccountOperation> operations)
        {
            var transactions = operations.ToBasicTransactions();
            var qifData = transactions.ToQifData();
            return qifData;
        }

        public class TemporaryCulture : IDisposable
        {
            private readonly CultureInfo _previousCulture;

            public TemporaryCulture(CultureInfo cultureInfo)
            {
                _previousCulture = CultureInfo.CurrentCulture;
                CultureInfo.CurrentCulture = cultureInfo;
            }

            public void Dispose()
            {
                CultureInfo.CurrentCulture = _previousCulture;
            }
        }
    }
}