using Operations.Classification.GererMesComptes;
using QifApi;

namespace Operations.Classification.Gmc.IntegrationTests.Steps
{
    public class GererMesCompteTestContext
    {
        public GererMesCompteTestContext(PostScenarioCleaner cleaner)
        {
            Cleaner = cleaner;
            GmcClient = new GererMesComptesClient();
            GmcAccounts = new AccountInfoRepository(GmcClient);
            GmcOperations = new OperationsRepository(GmcClient);
        }

        public PostScenarioCleaner Cleaner { set; get; }

        public GererMesComptesClient GmcClient { get; }

        public AccountInfoRepository GmcAccounts { set; get; }


        public OperationsRepository GmcOperations { set; get; }

        public string LastExportedQifData { set; get; }

        public QifDom LastExportedQifDom { set; get; }

        public RunImportResult LastQifImportResult { set; get; }

        public string ToImportQifData { set; get; }
    }
}