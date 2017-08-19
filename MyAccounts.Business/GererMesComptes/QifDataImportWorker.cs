using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Util;
using MyAccounts.Business.Extensions;
using Newtonsoft.Json.Linq;

namespace MyAccounts.Business.GererMesComptes
{
    public class QifDataImportWorker
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QifDataImportWorker));

        public QifDataImportWorker(GererMesComptesClient client)
        {
            Transport = client.Transport;
        }

        public HttpClient Transport { get; }

        public async Task<RunImportResult> Import(string accountId, string qifData)
        {
            var result = new RunImportResult(false, qifData);

            for (int attempt = 0; !result.Success && attempt < 3; attempt++)
            {
                _logger.Debug($"Starting automated import procedure - attempt {attempt}");
                JObject uploadResponseModel = await UploadData(qifData);
                var uploadSuccess = (bool)uploadResponseModel["response"];

                JObject processResponseModel = null;

                if (uploadSuccess)
                {
                    var fileId = (string)uploadResponseModel["tmp_name"];
                    processResponseModel = await StartProcessingFile(accountId, fileId);
                }

                var processUploadRequestSuccess = uploadSuccess && (bool)processResponseModel["response"];
                if (processUploadRequestSuccess)
                {
                    var importId = processResponseModel["id_import"].Value<string>();
                    //3 wait file is processed
                    result.Success = await PollAfterProcessingCompletion(importId);
                }
            }

            return result;
        }

        private async Task<bool> PollAfterProcessingCompletion(string importId)
        {
            var fields = new { id_import = importId }.ToRawMembersDictionary();

            var checkProgressionAttempts = 0;
            var finished = false;
            bool failed;
            var sw = Stopwatch.StartNew();
            int progression = 0;

            HttpResponseMessage lastCheckProgressionResponse;
            JObject lastCheckProgressionModel = null;
            do
            {
                var postContent = new FormUrlEncodedContent(fields);

                lastCheckProgressionResponse = await Transport.PostAsync("/system/requests/user/finances/account/account.import.html?action_form=checkProgression", postContent);
                failed = !lastCheckProgressionResponse.IsSuccessStatusCode;

                if (!failed)
                {
                    lastCheckProgressionModel = JObject.Parse(await lastCheckProgressionResponse.Content.ReadAsStringAsync());
                    failed = !(bool)lastCheckProgressionModel["response"] || (string)lastCheckProgressionModel["errno"] != null;
                }

                if (!failed)
                {
                    var previousProgression = progression;
                    progression = (int)lastCheckProgressionModel["progression"];
                    if (previousProgression != progression)
                    {
                        _logger.Debug($"import process progressed: {progression} %, check number : {checkProgressionAttempts}, eslapsed time : {sw.Elapsed.TotalSeconds:N2} seconds");
                    }

                    finished = progression == 100;

                    if (!finished && sw.Elapsed.TotalSeconds < 30)
                    {
                        await Task.Delay(500);
                    }
                }
            }
            while (sw.Elapsed.TotalSeconds < 30 && checkProgressionAttempts++ < 20 && !failed && !finished);
            sw.Stop();

            var success = !failed && finished;

            if (!success)
            {
                _logger.DebugExt(() => $"last received progression response{Environment.NewLine}" +
                                       $"Request : {lastCheckProgressionResponse.RequestMessage?.RequestUri}{Environment.NewLine}" +
                                       $"Response Model : {lastCheckProgressionModel}");
            }

            _logger.DebugExt(() =>
                $"import process completed. success ? {success}. eslapsed time : {sw.Elapsed.TotalSeconds:N2} seconds");

            return success;
        }

        private async Task<JObject> StartProcessingFile(string accountId, string fileId)
        {
            var fields =
                                new Dictionary<string, string>
                                {
                                    ["id_account"] = accountId,
                                    ["format"] = "QIF",
                                    ["file"] = fileId,
                                    ["i_account_import"] = "0",
                                    ["account_iban"] = "no",
                                    ["date_format"] = "m-d-Y",
                                    ["number_format"] = "fr",
                                    ["qif_import_third_party"] = "yes",
                                    ["check_duplicates"] = "yes",
                                    ["auto_categorization"] = "yes"
                                };

            var processResponse = await Transport.PostAsync(
                "/system/requests/user/finances/account/account.import.html?action_form=askImportManual",
                new FormUrlEncodedContent(fields));
            processResponse.EnsureSuccessStatusCode();
            var processResponseModel = JObject.Parse(await processResponse.Content.ReadAsStringAsync());
            return processResponseModel;
        }

        private async Task<JObject> UploadData(string qifData)
        {
            var content = new MultipartFormDataContent();
            content.Headers.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
            var qifDataStream = new MemoryStream(Encoding.UTF8.GetBytes(qifData));
            var streamContent = new StreamContent(qifDataStream, (int)qifDataStream.Length);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            content.Add(streamContent, "file", $"{Guid.NewGuid()}.qif");

            var uploadResponse = await Transport.PostAsync("/system/requests/user/parameters/accounts.html?action_form=importManual_upload_tmp", content);
            uploadResponse.EnsureSuccessStatusCode();

            var uploadResponseModel = JObject.Parse(await uploadResponse.Content.ReadAsStringAsync());
            return uploadResponseModel;
        }
    }
}
