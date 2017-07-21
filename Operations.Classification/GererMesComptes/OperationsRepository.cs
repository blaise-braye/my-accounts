using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using QifApi;
using QifApi.Transactions;

namespace Operations.Classification.GererMesComptes
{
    public class OperationsRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OperationsRepository));

        private readonly GererMesComptesClient _client;
        private readonly QifDataImportWorker _importWorker;

        public OperationsRepository(GererMesComptesClient client)
        {
            _client = client;
            _importWorker = new QifDataImportWorker(client);
        }

        private HttpClient Transport => _client.Transport;

        public async Task<TransactionDeltaSet> DryRunImport(string accountId, string availableQifData)
        {
            var availableQifDom = QifMapper.ParseQifDom(availableQifData);
            var minDate = availableQifDom.BankTransactions.Min(t => t.Date.AddDays(-1));
            var maxDate = availableQifDom.BankTransactions.Max(t => t.Date);

            var exportedQifData = await ExportQif(accountId, minDate, maxDate);
            var exportedQifDom = QifMapper.ParseQifDom(exportedQifData);
            var exportedByKey = exportedQifDom.BankTransactions.ToLookup(s => s.GetBankTransactionLookupKey());

            var delta = new TransactionDeltaSet();

            foreach (var availableBt in availableQifDom.BankTransactions)
            {
                var key = availableBt.GetBankTransactionLookupKey();
                if (!exportedByKey.Contains(key))
                {
                    delta.SetAddAction(availableBt);
                }
                else
                {
                    var exportedSimilars = exportedByKey[key].ToList();
                    if (exportedSimilars.Count > 1)
                    {
                        var exportedSimilar = exportedSimilars.FirstOrDefault(s => !delta.IsTargetProcessed(s) && s.Memo == availableBt.Memo);
                        if (exportedSimilar != null)
                        {
                            delta.SetNothingAction(availableBt, exportedSimilar);
                        }
                        else
                        {
                            delta.SetMultipleTargetsPossibleAction(availableBt);
                        }
                    }
                    else
                    {
                        var exportedItem = exportedSimilars.Single();
                        if (exportedItem.Memo != availableBt.Memo)
                        {
                            delta.SetUpdateMemoAction(availableBt, exportedItem);
                        }
                        else
                        {
                            delta.SetNothingAction(availableBt, exportedItem);
                        }
                    }
                }
            }

            var potentialExports = exportedQifDom.BankTransactions.Where(t => !delta.IsTargetProcessed(t)).ToList();
            TryToResolveAmbiguousItems(delta, potentialExports);

            var toRemoveItems = exportedQifDom.BankTransactions.Where(t => !delta.IsTargetProcessed(t));
            foreach (var exportedItem in toRemoveItems)
            {
                delta.SetRemoveAction(exportedItem);
            }

            return delta;
        }

        public Task<string> ExportQif(string accountId, int days)
        {
            return ExportQif(accountId, DateTime.Today.AddDays(-days - 1), DateTime.Today);
        }

        public async Task<string> ExportQif(string accountId, DateTime startDate, DateTime endData)
        {
            var dico = new Dictionary<string, string>
                           {
                               ["id_account"] = accountId,
                               ["export"] = "dates",
                               ["interval_start"] = ToUnixTime(startDate.Date).ToString(),
                               ["interval_end"] = ToUnixTime(endData.Date).ToString(),
                               ["format"] = "qif",
                               ["date_format"] = "mm-dd-aaaa",
                               ["amount_format"] = "auto",
                               ["csv_separator"] = "semi-colon",
                               ["csv_column"] = "yes",
                               ["category_separator"] = "%3A%3A",
                               ["encoding"] = "utf-8"
                           };

            var response = await Transport.PostAsync("/fr/u/finances/comptes/" + accountId + "/export.html", new FormUrlEncodedContent(dico));
            response.EnsureSuccessStatusCode();

            var qifData = await response.Content.ReadAsStringAsync();
            qifData = qifData.Replace("\n", Environment.NewLine);

            return qifData;
        }

        public Task<RunImportResult> Import(string accountId, string qifData)
        {
            return _importWorker.Import(accountId, qifData);
        }

        public async Task<RunImportResult> RunImportFromDeltaActions(string accountId, List<TransactionDelta> operationsDelta)
        {
            var deltasOfAddKind = operationsDelta.Where(s => s.Action == DeltaAction.Add);
            var newTransactions = deltasOfAddKind.Select(i => i.Source).ToList();

            RunImportResult result;
            var qifData = newTransactions.ToQifData();

            if (newTransactions.Count == 0)
            {
                result = new RunImportResult(true, qifData);
            }
            else
            {
                result = await Import(accountId, qifData);
            }

            return result;
        }

        public async Task<string> WaitExportAvailability(string accountId, string lastImportedQifData, int secondsToWait = 20)
        {
            var sw = Stopwatch.StartNew();
            bool available;

            var importedQifDom = QifMapper.ParseQifDom(lastImportedQifData);

            var importedByKey = importedQifDom.BankTransactions.ToLookup(s => s.GetBankTransactionLookupKey());
            var importedKeys = new HashSet<string>(importedByKey.Select(i => i.Key));
            string exportedQifData;

            QifDom lastExportedQifDom;
            do
            {
                exportedQifData = await ExportQif(
                    accountId,
                    importedQifDom.BankTransactions.Min(t => t.Date).AddDays(-1),
                    importedQifDom.BankTransactions.Max(t => t.Date));

                lastExportedQifDom = QifMapper.ParseQifDom(exportedQifData);
                var exportedbyKey = lastExportedQifDom.BankTransactions.ToLookup(s => s.GetBankTransactionLookupKey());
                available = exportedbyKey.Select(s => s.Key).Where(importedKeys.Contains).Intersect(importedKeys).Count() == importedKeys.Count;
                if (!available)
                {
                    _logger.Warn("number of imported items do not match number exported items yet");
                }
                else
                {
                    foreach (var importedKeyItems in importedByKey)
                    {
                        var exported = exportedbyKey[importedKeyItems.Key].ToList();
                        foreach (var importedKeyItem in importedKeyItems)
                        {
                            available = exported.Any(exportedKeyItem => importedKeyItem.Memo == exportedKeyItem.Memo);

                            if (!available)
                            {
                                _logger.Warn($"item not available in export yet :{importedKeyItem.Number} - {importedKeyItem.Memo}");
                                break;
                            }
                        }

                        if (!available)
                        {
                            break;
                        }
                    }
                }

                if (!available)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                }
            }
            while (sw.Elapsed.TotalSeconds < secondsToWait && !available);

            if (!available)
            {
                var rawExport = string.Join(Environment.NewLine, lastExportedQifDom.BankTransactions.Select(
                    s => $"{s.GetBankTransactionLookupKey()} - {s.Amount} - {s.Memo}"));
                _logger.Debug($"Last received export{Environment.NewLine}{rawExport}");

                throw new Exception($"Timeout, could not detect export availability during {secondsToWait} seconds");
            }

            return exportedQifData;
        }

        private static long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        private void TryToResolveAmbiguousItems(TransactionDeltaSet delta, List<BasicTransaction> potentialExports)
        {
            var ambiguousItemsGroups = delta.GetDeltaByAction(DeltaAction.MultipleTargetsPossible);

            var toImportSplittedMemos = ambiguousItemsGroups
                .ToDictionary(t => t, t => new HashSet<string>(t.Source.Memo.Split(' ')));

            var exportSplittedMemos = potentialExports.ToDictionary(t => t, t => new HashSet<string>(t.Memo.Split(' ')));

            var toImportItemsByKey = ambiguousItemsGroups.ToLookup(t => t.Source.GetBankTransactionLookupKey());
            var exportedItemsByKey = potentialExports.ToLookup(t => t.GetBankTransactionLookupKey());

            foreach (var toImportKeyItems in toImportItemsByKey)
            {
                var key = toImportKeyItems.Key;
                var exportedKeyItems = exportedItemsByKey[key].ToList();

                var suggestedResolution = new List<Tuple<TransactionDelta, BasicTransaction>>();

                foreach (var importedKeyItem in toImportKeyItems)
                {
                    var importedKeyItemWords = toImportSplittedMemos[importedKeyItem];
                    var nearest = exportedKeyItems
                        .OrderByDescending(exportedKeyITem => exportSplittedMemos[exportedKeyITem].Intersect(importedKeyItemWords).Count())
                        .FirstOrDefault();

                    if (nearest != null)
                    {
                        exportedKeyItems.Remove(nearest);
                        suggestedResolution.Add(Tuple.Create(importedKeyItem, nearest));
                    }
                }

                if (suggestedResolution.Count == toImportKeyItems.Count())
                {
                    foreach (var tuple in suggestedResolution)
                    {
                        delta.SetUpdateMemoAction(tuple.Item1, tuple.Item2);
                    }
                }
            }
        }
    }
}