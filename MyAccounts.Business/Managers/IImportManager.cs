using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MyAccounts.Business.Managers.Imports;

namespace MyAccounts.Business.Managers
{
    public interface IImportManager
    {
        Task<bool> RequestImportExecution(ImportCommand importCommand, Stream sourceData);

        Task<List<ImportCommand>> GetAll(Guid accountId);

        Task DeleteImports(Guid accountId, IEnumerable<Guid> importCommands);

        Task<bool> ReplayCommands(Guid accountId);

        Task<bool> ReplayCommands(Guid accountId, List<ImportCommand> importCommands);

        Task<List<ImportExecutionImpact>> GetLastExecutionImpact(Guid accountId, IEnumerable<Guid> importCommands);

        Task<List<ImportExecutionImpact>> GetExecutionImpacts(Guid accountId, Guid commandId);
    }
}