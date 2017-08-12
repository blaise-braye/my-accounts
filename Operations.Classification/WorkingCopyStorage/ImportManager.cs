using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace Operations.Classification.WorkingCopyStorage
{
    public interface IImportManager
    {
        Task<bool> RequestImportExecution(ImportCommand importCommand, Stream sourceData);

        Task<List<ImportCommand>> GetAll(Guid accountId);

        Task<bool> ReplayCommand(Guid accountId, List<ImportCommand> importCommands);
    }

    public class ImportManager : IImportManager
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ImportManager));

        private readonly IAccountCommandRepository _accountCommandRepository;

        private readonly IOperationsRepository _operationsRepository;

        public ImportManager(IAccountCommandRepository accountCommandRepository, IOperationsRepository operationsRepository)
        {
            _accountCommandRepository = accountCommandRepository;
            _operationsRepository = operationsRepository;
        }

        public async Task<bool> ReplayCommand(Guid accountId, List<ImportCommand> importCommands)
        {
            if (importCommands.Any(a => a.AccountId != accountId))
            {
                throw new InvalidOperationException("the import must be strictly related to one account name");
            }

            _operationsRepository.Clear(accountId);

            var result = true;

            foreach (var importCommand in importCommands)
            {
                using (var sourceData = await _accountCommandRepository.OpenAttachment(importCommand))
                {
                    result &= await _operationsRepository.ExecuteImport(importCommand, sourceData);
                }
            }

            return result;
        }

        public async Task<bool> RequestImportExecution(ImportCommand importCommand, Stream sourceData)
        {
            try
            {
                if (!sourceData.CanSeek)
                {
                    var seekableStream = new MemoryStream();
                    await sourceData.CopyToAsync(seekableStream);
                    sourceData = seekableStream;
                    sourceData.Seek(0, SeekOrigin.Begin);
                }

                if (await _operationsRepository.ExecuteImport(importCommand, sourceData))
                {
                    sourceData.Seek(0, SeekOrigin.Begin);
                    await _accountCommandRepository.Add(importCommand, sourceData);
                }
            }
            catch (Exception exn)
            {
                _logger.Error("import failed", exn);
            }

            return true;
        }

        public Task<List<ImportCommand>> GetAll(Guid accountId)
        {
            return _accountCommandRepository.GetAll(accountId);
        }
    }
}
