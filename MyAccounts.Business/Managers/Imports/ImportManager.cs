using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using MyAccounts.Business.IO.Caching;
using MyAccounts.Business.Managers.Operations;

namespace MyAccounts.Business.Managers.Imports
{
    public class ImportManager : IImportManager
    {
        private const string ImportsByAccountIdRoute = "/Imports/{0}";
        private const string ImportExecutionImpactByAccountIdAndCommandIdRoute = "/Imports/{0}/ExecutionImpacts/{1}";

        private static readonly ILog _logger = LogManager.GetLogger(typeof(ImportManager));

        private readonly ICacheProvider _cacheProvider;
        private readonly IAccountCommandRepository _accountCommandRepository;
        private readonly IOperationsManager _operationsManager;

        public ImportManager(ICacheProvider cacheProvider, IAccountCommandRepository accountCommandRepository, IOperationsManager operationsManager)
        {
            _cacheProvider = cacheProvider;
            _accountCommandRepository = accountCommandRepository;
            _operationsManager = operationsManager;
        }

        public async Task<bool> ReplayCommands(Guid accountId)
        {
            var commands = await GetAll(accountId);
            return await ReplayCommands(accountId, commands);
        }

        public async Task<bool> ReplayCommands(Guid accountId, List<ImportCommand> importCommands)
        {
            if (importCommands.Any(a => a.AccountId != accountId))
            {
                throw new InvalidOperationException("the import must be strictly related to one account name");
            }

            await _operationsManager.Clear(accountId);

            var result = true;

            var orderedCommands = importCommands.OrderByDescending(c => c.CreationDate);

            foreach (var importCommand in orderedCommands)
            {
                using (var sourceData = await _accountCommandRepository.OpenAttachment(importCommand))
                {
                    var importExecutionImpact = await _operationsManager.ExecuteImport(importCommand, sourceData);
                    await _accountCommandRepository.AddExecutionImpact(importCommand.AccountId, importExecutionImpact);
                    result &= importExecutionImpact.Success;
                }
            }

            return result;
        }

        public async Task<bool> RequestImportExecution(ImportCommand importCommand, Stream sourceData)
        {
            bool result = false;

            try
            {
                if (!sourceData.CanSeek)
                {
                    var seekableStream = new MemoryStream();
                    await sourceData.CopyToAsync(seekableStream);
                    sourceData = seekableStream;
                    sourceData.Seek(0, SeekOrigin.Begin);
                }

                await _accountCommandRepository.Add(importCommand, sourceData);

                sourceData.Seek(0, SeekOrigin.Begin);
                var executionImportResult = await _operationsManager.ExecuteImport(importCommand, sourceData);
                result = await _accountCommandRepository.AddExecutionImpact(importCommand.AccountId, executionImportResult);
            }
            catch (Exception exn)
            {
                _logger.Error("import failed", exn);
            }

            if (result)
            {
                await GetCacheEntry(importCommand.AccountId).DeleteAsync();
            }

            return result;
        }

        public async Task<List<ImportCommand>> GetAll(Guid accountId)
        {
            var result = await GetCacheEntry(accountId).GetOrAddAsync(
                () => _accountCommandRepository.GetAll(accountId));
            return result;
        }

        public async Task DeleteImports(Guid accountId, IEnumerable<Guid> importCommands)
        {
            foreach (var commandId in importCommands)
            {
                await _accountCommandRepository.Delete(accountId, commandId);
                await GetCacheEntry(accountId, commandId).DeleteAsync();
            }
        }

        public async Task<List<ImportExecutionImpact>> GetLastExecutionImpact(Guid accountId, IEnumerable<Guid> importCommands)
        {
            var result = new List<ImportExecutionImpact>();

            foreach (var importCommandId in importCommands)
            {
                var impacts = await GetExecutionImpacts(accountId, importCommandId);
                var lastImpact = impacts.OrderByDescending(i => i.CreationDate).FirstOrDefault();

                if (lastImpact != null)
                {
                    result.Add(lastImpact);
                }
            }

            return result;
        }

        public async Task<List<ImportExecutionImpact>> GetExecutionImpacts(Guid accountId, Guid commandId)
        {
            var result = await GetCacheEntry(accountId, commandId).GetOrAddAsync(
                () => _accountCommandRepository.GetExecutionImpacts(accountId, commandId));
            return result;
        }

        private ICacheEntry<List<ImportCommand>> GetCacheEntry(Guid accountId)
        {
            return _cacheProvider.GetJSonCacheEntry<List<ImportCommand>>(
                string.Format(
                    ImportsByAccountIdRoute,
                    accountId));
        }

        private ICacheEntry<List<ImportExecutionImpact>> GetCacheEntry(Guid accountId, Guid commandId)
        {
            return _cacheProvider.GetJSonCacheEntry<List<ImportExecutionImpact>>(
                string.Format(
                    ImportExecutionImpactByAccountIdAndCommandIdRoute,
                    accountId,
                    commandId));
        }
    }
}
