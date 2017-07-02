using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.WpfUi.Data
{
    public interface ITransactionsRepository
    {
        Task<List<UnifiedAccountOperation>> GetTransformedUnifiedOperations(string accountName);

        Task<bool> Import(string accountName, Stream importData, SourceKind sourceKind);

        Task Export(string filePath, IList<AccountOperationBase> operations);
    }
}