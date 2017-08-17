using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Operations.Classification.Managers.Imports
{
    public interface IAccountCommandRepository
    {
        Task<List<ImportCommand>> GetAll(Guid accountId);

        Task<bool> Add(ImportCommand importCommand, Stream attachment);

        Task<bool> Replace(ImportCommand importCommand);

        Task<bool> Delete(Guid accountId, Guid commandId);

        Task<Stream> OpenAttachment(ImportCommand importCommand);

        Task<bool> AddExecutionImpact(Guid accountId, ImportExecutionImpact importCommand);

        Task<List<ImportExecutionImpact>> GetExecutionImpacts(Guid accountId, Guid commandId);
    }
}