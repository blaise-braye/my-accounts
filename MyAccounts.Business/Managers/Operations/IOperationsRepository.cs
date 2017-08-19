using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Unified;
using MyAccounts.Business.Managers.Imports;

namespace MyAccounts.Business.Managers.Operations
{
    public interface IOperationsRepository
    {
        Task<List<UnifiedAccountOperation>> GetAll(Guid accountId);

        Task Export(string filePath, IList<AccountOperationBase> operations);

        void Clear(Guid accountId);

        Task<ImportExecutionImpact> ExecuteImport(ImportCommand importCommand, Stream sourceData);
    }
}