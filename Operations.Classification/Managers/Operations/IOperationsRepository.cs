using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.Managers.Imports;

namespace Operations.Classification.Managers.Operations
{
    public interface IOperationsRepository
    {
        Task<List<UnifiedAccountOperation>> GetAll(Guid accountId);

        Task Export(string filePath, IList<AccountOperationBase> operations);

        void Clear(Guid accountId);

        Task<ImportExecutionImpact> ExecuteImport(ImportCommand importCommand, Stream sourceData);
    }
}