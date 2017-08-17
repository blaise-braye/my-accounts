using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.Caching;
using Operations.Classification.Managers.Imports;

namespace Operations.Classification.Managers.Operations
{
    public interface IOperationsManager
    {
        Task<List<UnifiedAccountOperation>> GetTransformedUnifiedOperations(Guid accountId);

        Task<List<UnifiedAccountOperation>> DetectPotentialDuplicates(Guid accountId);

        Task Export(string exportFilePath, List<AccountOperationBase> operations);
    }

    public class OperationsManager : IOperationsManager
    {
        private const string UnifiedAccountOperationsByAccountRoute = "/UnifiedAccountOperations/{0}";
        private static readonly ILog _log = LogManager.GetLogger(typeof(ImportManager));
        private readonly IOperationsRepository _operationsRepository;

        public OperationsManager(IOperationsRepository operationsRepository)
        {
            _operationsRepository = operationsRepository;
        }

        public async Task<List<UnifiedAccountOperation>> GetTransformedUnifiedOperations(Guid accountId)
        {
            var result = await GetCacheEntry(accountId).GetOrAddAsync(
                () => _operationsRepository.GetAll(accountId));
            return result;
        }

        public async Task<List<UnifiedAccountOperation>> DetectPotentialDuplicates(Guid accountId)
        {
            var operations = await GetTransformedUnifiedOperations(accountId);

            var doublonsByOperationId = operations.GroupBy(d => d.OperationId).Where(g => g.Count() > 1).SelectMany(g => g);

            var doublonsByDataAndValue = operations.GroupBy(d => $"{d.ValueDate}-{d.Income}-{d.Outcome}").Where(g => g.Count() > 1).SelectMany(g => g);

            var result = doublonsByOperationId.Union(doublonsByDataAndValue)
                .OrderByDescending(d => d.OperationId)
                .ThenByDescending(d => d.ValueDate)
                .ThenByDescending(d => d.Income)
                .ThenByDescending(d => d.Outcome)
                .ToList();

            // validate fortis operations sequence number (detect missing operations)
            var fortisOperations = operations.Where(
                    op => op.SourceKind == AccountOperations.Contracts.SourceKind.FortisCsvArchive
                          || op.SourceKind == AccountOperations.Contracts.SourceKind.FortisCsvExport)
                .OrderBy(op => op.OperationId);
            int[] previousOperationId = null;
            foreach (var fortisOperation in fortisOperations)
            {
                var operationIdParts = fortisOperation.OperationId.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (operationIdParts.Length == 2)
                {
                    var operationYear = int.Parse(operationIdParts[0]);
                    var operationYearNumber = int.Parse(operationIdParts[1]);

                    if (previousOperationId != null)
                    {
                        var prevOpYear = previousOperationId[0];
                        var prevOpYearNumber = previousOperationId[1];
                        var sequenceAsExpectated = true;
                        if (operationYear == prevOpYear + 1)
                        {
                            if (operationYearNumber != 1)
                            {
                                sequenceAsExpectated = false;
                            }
                        }
                        else if (operationYear == prevOpYear)
                        {
                            if (operationYearNumber != prevOpYearNumber + 1)
                            {
                                sequenceAsExpectated = false;
                            }
                        }
                        else
                        {
                            sequenceAsExpectated = false;
                        }

                        if (!sequenceAsExpectated)
                        {
                            _log.Error(
                                $"operation id sequence mismatch (previous {string.Join("-", previousOperationId)}, current {string.Join("-", operationIdParts)}");
                        }
                    }

                    previousOperationId = new[] { operationYear, operationYearNumber };
                }
                else
                {
                    _log.Error(
                        $"operation id format suspicious : {fortisOperation.OperationId}");
                }
            }

            return result;
        }

        public Task Export(string exportFilePath, List<AccountOperationBase> operations)
        {
            return _operationsRepository.Export(exportFilePath, operations);
        }

        private static ICacheEntry<List<UnifiedAccountOperation>> GetCacheEntry(Guid accountId)
        {
            return CacheProvider.GetJSonCacheEntry<List<UnifiedAccountOperation>>(string.Format(UnifiedAccountOperationsByAccountRoute, accountId));
        }
    }
}