namespace MyAccounts.Business.GererMesComptes
{
    public class RunImportResult
    {
        public RunImportResult(bool success, string importedQifData = null)
        {
            Success = success;
            ImportedQifData = importedQifData;
        }

        public bool Success { get; set; }

        public string ImportedQifData { get; set; }
    }
}