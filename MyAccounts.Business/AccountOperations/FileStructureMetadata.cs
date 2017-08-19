using MyAccounts.Business.AccountOperations.Contracts;

namespace MyAccounts.Business.AccountOperations
{
    public class FileStructureMetadata
    {
        public SourceKind SourceKind { get; set; }

        public string Encoding { get; set; }

        public string Culture { get; set; }
    }
}