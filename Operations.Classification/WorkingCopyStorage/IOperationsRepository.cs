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

        Task<bool> RequestImportExecution(ImportCommand importCommand, Stream sourceData);

        Task Export(string filePath, IList<AccountOperationBase> operations);

        Task<bool> ReplayCommand(Guid accountId, List<ImportCommand> importCommands);

        void Clear(Guid accountId);
    }
}