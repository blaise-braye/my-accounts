using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.WorkingCopyStorage
{
    public interface IOperationsRepository
    {
        Task<List<UnifiedAccountOperation>> GetAll(Guid accountId);

        Task Export(string filePath, IList<AccountOperationBase> operations);

        void Clear(Guid accountId);

        Task<bool> ExecuteImport(ImportCommand importCommand, Stream sourceData);
    }
}