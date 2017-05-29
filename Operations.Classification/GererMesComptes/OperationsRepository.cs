using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Operations.Classification.Extensions;
using QifApi.Transactions;

namespace Operations.Classification.GererMesComptes
{
    public class OperationsRepository
    {
        private readonly GererMesComptesClient _client;

        public OperationsRepository(GererMesComptesClient client)
        {
            _client = client;
        }

        private HttpClient Transport => _client.Transport;

        public async Task<RunImportResult> Import(string accountId, string qifData)
        {
            var result = new RunImportResult(false, qifData);
            //1 upload file
            var content = new MultipartFormDataContent();
            content.Headers.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
            var qifDataStream = new MemoryStream(Encoding.UTF8.GetBytes(qifData));
            var streamContent = new StreamContent(qifDataStream, (int)qifDataStream.Length);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            content.Add(streamContent, "file", $"{Guid.NewGuid()}.qif");

            var uploadResponse = await Transport.PostAsync("/system/requests/user/parameters/accounts.html?action_form=importManual_upload_tmp", content);
            uploadResponse.EnsureSuccessStatusCode();

            var uploadResponseModel = JObject.Parse(await uploadResponse.Content.ReadAsStringAsync());
            var uploadSucceed = (bool)uploadResponseModel["response"];

            var processRequestSucceed = false;
            JObject processResponseModel = null;

            if (uploadSucceed)
            {
                var fileId = (string)uploadResponseModel["tmp_name"];

                //2 request processing of file
                var fields = new Dictionary<string, string>();
                fields["id_account"] = accountId;
                fields["format"] = "QIF";
                fields["file"] = fileId;
                fields["i_account_import"] = "0";
                fields["account_iban"] = "no";
                fields["date_format"] = "m-d-Y";
                fields["number_format"] = "fr";
                fields["qif_import_third_party"] = "yes";
                fields["check_duplicates"] = "yes";
                fields["auto_categorization"] = "yes";

                var processResponse = await Transport.PostAsync(
                    "/system/requests/user/finances/account/account.import.html?action_form=askImportManual",
                    new FormUrlEncodedContent(fields));
                processResponse.EnsureSuccessStatusCode();
                processResponseModel = JObject.Parse(await processResponse.Content.ReadAsStringAsync());
                processRequestSucceed = (bool)processResponseModel["response"];
            }

            if (processRequestSucceed)
            {
                //3 wait file is processed
                var importId = processResponseModel["id_import"];
                var fields = new { id_import = importId }.ToRawMembersDictionary();

                var attempts = 0;
                var finished = false;
                bool failed;
                var sw = Stopwatch.StartNew();
                do
                {
                    var postContent = new FormUrlEncodedContent(fields);
                    var checkProgressionResponse =
                        await Transport.PostAsync("/system/requests/user/finances/account/account.import.html?action_form=checkProgression", postContent);

                    failed = !checkProgressionResponse.IsSuccessStatusCode;
                    JObject checkProgressionModel = null;

                    if (!failed)
                    {
                        checkProgressionModel = JObject.Parse(await checkProgressionResponse.Content.ReadAsStringAsync());
                        failed = !(bool)checkProgressionModel["response"] || (string)checkProgressionModel["errno"] != null;
                    }

                    if (!failed)
                        finished = (int)checkProgressionModel["progression"] == 100;

                    if (!finished && sw.Elapsed.TotalSeconds < 30)
                        await Task.Delay(300);
                } while (sw.Elapsed.TotalSeconds < 30 && attempts++ < 20 && !failed && !finished);
                sw.Stop();

                result.Success = !failed && finished;
            }

            return result;
        }

        public async Task<RunImportResult> RunImport(string accountId, List<TransactionDelta> operationsDelta)
        {
            var deltasOfAddKind = operationsDelta.Where(s => s.Action == DeltaAction.Add);
            var newTransactions = deltasOfAddKind.Select(i => i.Source).ToList();
            
            RunImportResult result;
            var qifData = newTransactions.ToQifData();
                
            if (newTransactions.Count == 0)
                result = new RunImportResult(true, qifData);
            else
                result = await Import(accountId, qifData);

            return result;
            
        }

        public Task<string> ExportQif(string accountId, int days)
        {
            return ExportQif(accountId, DateTime.Today.AddDays(-days - 1), DateTime.Today);
        }

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
                            delta.SetNothingAction(availableBt, exportedSimilar);
                        else
                            delta.SetMultipleTargetsPossibleAction(availableBt);
                    }
                    else
                    {
                        var exportedItem = exportedSimilars.Single();
                        if (exportedItem.Memo != availableBt.Memo)
                            delta.SetUpdateMemoAction(availableBt, exportedItem);
                        else
                            delta.SetNothingAction(availableBt, exportedItem);
                    }
                }
            }

            var potentialExports = exportedQifDom.BankTransactions.Where(t => !delta.IsTargetProcessed(t)).ToList();
            TryToResolveAmbiguousItems(delta, potentialExports);

            var toRemoveItems = exportedQifDom.BankTransactions.Where(t => !delta.IsTargetProcessed(t));
            foreach (var exportedItem in toRemoveItems)
                delta.SetRemoveAction(exportedItem);

            return delta;
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

                    suggestedResolution.Add(Tuple.Create(importedKeyItem, nearest));
                }

                if (suggestedResolution.Count == toImportKeyItems.Count())
                    foreach (var tuple in suggestedResolution)
                        delta.SetUpdateMemoAction(tuple.Item1, tuple.Item2);
            }
        }

        public async Task<string> WaitExportAvailability(string accountId, string lastImportedQifData)
        {
            var sw = Stopwatch.StartNew();
            bool available;

            var importedQifDom = QifMapper.ParseQifDom(lastImportedQifData);

            var importedByKey = importedQifDom.BankTransactions.ToLookup(s => s.GetBankTransactionLookupKey());
            var importedKeys = new HashSet<string>(importedByKey.Select(i => i.Key));
            string exportedQifData;
            do
            {
                exportedQifData = await ExportQif(
                    accountId,
                    importedQifDom.BankTransactions.Min(t => t.Date).AddDays(-1),
                    importedQifDom.BankTransactions.Max(t => t.Date));
                var exportedQifDom = QifMapper.ParseQifDom(exportedQifData);
                var exportedbyKey = exportedQifDom.BankTransactions.ToLookup(s => s.GetBankTransactionLookupKey());
                available = exportedbyKey.Select(s => s.Key).Where(importedKeys.Contains).Union(importedKeys).Count() == importedKeys.Count;
                if (!available)
                {
                    Trace.WriteLine("number of imported items do not match number exported items yet");
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
                                Trace.WriteLine($"item not available in export yet :{importedKeyItem.Number} - {importedKeyItem.Memo}");
                                break;
                            }
                        }
                        if (!available)
                            break;
                    }
                    if (!available)
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                }
            } while (sw.Elapsed.TotalSeconds < 60 && !available);

            if (!available)
                throw new Exception("Timeout, could not detect export availability during 60 seconds");

            return exportedQifData;
        }

        public async Task<string> ExportQif(string accountId, DateTime startDate, DateTime endData)
        {
            var dico = new Dictionary<string, string>();

            dico["id_account"] = accountId;
            dico["export"] = "dates";
            dico["interval_start"] = ToUnixTime(startDate.Date).ToString();
            dico["interval_end"] = ToUnixTime(endData.Date).ToString();
            dico["format"] = "qif";
            dico["date_format"] = "mm-dd-aaaa";
            dico["amount_format"] = "auto";
            dico["csv_separator"] = "semi-colon";
            dico["csv_column"] = "yes";
            dico["category_separator"] = "%3A%3A";
            dico["encoding"] = "utf-8";

            string qifData;

            var response = await Transport.PostAsync("/fr/u/finances/comptes/" + accountId + "/export.html", new FormUrlEncodedContent(dico));
            response.EnsureSuccessStatusCode();

            qifData = await response.Content.ReadAsStringAsync();
            qifData = qifData.Replace("\n", Environment.NewLine);

            return qifData;
        }

        private static long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }
    }
}