using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Operations.Classification.WorkingCopyStorage
{
    public interface IAccountCommandRepository
    {
        Task<List<ImportCommand>> GetAll(Guid accountId);

        Task<bool> Add(ImportCommand importCommand, Stream attachment);

        Task<bool> Replace(ImportCommand importCommand);

        Task<Stream> OpenAttachment(ImportCommand importCommand);
    }
}