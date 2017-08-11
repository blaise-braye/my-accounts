using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.WpfUi.Managers.Transactions
{
    public interface IOperationsManager
    {
        Task<List<UnifiedAccountOperation>> GetTransformedUnifiedOperations(Guid accountId);
    }
}