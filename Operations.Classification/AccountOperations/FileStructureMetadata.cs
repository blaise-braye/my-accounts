using System.Text;
using Operations.Classification.AccountOperations.Contracts;

namespace Operations.Classification.AccountOperations
{
    public class FileStructureMetadata
    {
        public SourceKind SourceKind { get; set; }

        public string Encoding { get; set; }

        public string Culture { get; set; }
    }
}