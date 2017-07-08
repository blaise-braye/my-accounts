using System.Collections.Generic;
using System.Threading.Tasks;
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.WpfUi.Managers.Transactions
{
    public interface ITransactionsManager
    {
        Task<List<UnifiedAccountOperation>> GetTransformedUnifiedOperations(string accountName);
    }
}