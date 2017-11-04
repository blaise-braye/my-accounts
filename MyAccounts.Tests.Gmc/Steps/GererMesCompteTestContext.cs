using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MyAccounts.Business.GererMesComptes;
using QifApi;

namespace MyAccounts.Tests.Gmc.Steps
{
    public class GererMesCompteTestContext
    {
        public GererMesCompteTestContext(PostScenarioCleaner cleaner)
        {
            Cleaner = cleaner;
            GmcClient = new GererMesComptesClient(h => new LoggingHandler(h));
            GmcAccounts = new AccountInfoRepository(GmcClient);
            GmcOperations = new OperationsRepository(GmcClient);
        }

        public PostScenarioCleaner Cleaner { get; set; }

        public GererMesComptesClient GmcClient { get; }

        public AccountInfoRepository GmcAccounts { get; set; }
        
        public OperationsRepository GmcOperations { get; set; }

        public string LastExportedQifData { get; set; }

        public QifDom LastExportedQifDom { get; set; }

        public RunImportResult LastQifImportResult { get; set; }
        
        public TransactionDeltaSet LastOperationsDelta { get; set; }

        public class LoggingHandler : DelegatingHandler
        {
            private static readonly ILog _logger = LogManager.GetLogger(typeof(HttpMessageHandler));

            public LoggingHandler(HttpMessageHandler innerHandler)
                : base(innerHandler)
            {
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Stopwatch processingTime = null;
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Processing Request: {request.Method} - {request.RequestUri}");

                    if (_logger.IsVerboseEnabled() && request.Content != null)
                    {
                        var rawContent = await request.Content.ReadAsStringAsync();
                        _logger.Verbose($"Request Content{Environment.NewLine} {rawContent}");
                    }
                    
                    processingTime = Stopwatch.StartNew();
                }
                
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
                
                if (processingTime != null)
                {
                    var elapsed = processingTime.Elapsed;
                    processingTime.Stop();
                    _logger.Debug($"Processed Request in {elapsed.TotalMilliseconds:N0} msec - with response status {response.StatusCode}, {response.ReasonPhrase} : {request.Method} - {request.RequestUri}");
                    
                    if (_logger.IsVerboseEnabled() && response.Content != null)
                    {
                        var rawContent = await response.Content.ReadAsStringAsync();
                        _logger.Verbose($"**** Response Content ***{Environment.NewLine} {rawContent}");
                    }
                }
                
                return response;
            }
        }
    }
}