using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.WpfUi.Data
{
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly ICsvAccountOperationManager _csvAccountOperationManager;
        private readonly UnifiedAccountOperationPatternTransformer _transactionPatternMapper;
        private readonly IWorkingCopy _workingCopy;

        public TransactionsRepository(
            IWorkingCopy workingCopy,
            ICsvAccountOperationManager csvAccountOperationManager,
            UnifiedAccountOperationPatternTransformer transactionPatternMapper)
        {
            _workingCopy = workingCopy;
            _csvAccountOperationManager = csvAccountOperationManager;
            _transactionPatternMapper = transactionPatternMapper;
        }

        private IFileSystem Fs => _workingCopy.Fs;

        private FileBase Fb => Fs.File;

        public async Task<bool> Import(string accountName, Stream importData, SourceKind sourceKind)
        {
            var operationsDirectory = _workingCopy.GetAccountOperationsDirectory(accountName);
            await _workingCopy.CreateFolderIfDoesNotExistsYet(operationsDirectory);

            var filteredData = await ReadAndFilterNewImportDataOnly(accountName, importData, sourceKind);
            if (filteredData.Operations.Length > 0)
            {
                var filePrefixPath = Path.Combine(operationsDirectory, $"./{filteredData.MaxDate:yyyy-MM-dd}-{sourceKind}");
                var filePath = $"{filePrefixPath}.csv";

                var counter = 1;
                while (Fb.Exists(filePath))
                {
                    filePath = $"{filePrefixPath}.{counter}.csv";
                }

                await _csvAccountOperationManager.WriteAsync(filePath, filteredData.Operations);
            }

            return true;
        }

        public Task Export(string filePath, IList<AccountOperationBase> operations)
        {
            return _csvAccountOperationManager.WriteAsync(filePath, operations);
        }

        public async Task<List<UnifiedAccountOperation>> GetTransformedUnifiedOperations(string accountName)
        {
            var operationsBases = await GetAllOperations(accountName);
            var result = operationsBases
                .Select(_transactionPatternMapper.Apply)
                .OrderByDescending(t => t.OperationId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return result;
        }

        private async Task<List<AccountOperationBase>> GetAllOperations(string accountName)
        {
            var result = new List<AccountOperationBase>();

            var operationsDirectory = _workingCopy.GetAccountOperationsDirectory(accountName);
            if (Directory.Exists(operationsDirectory))
            {
                var files = Directory.GetFiles(operationsDirectory, "*.csv");
                foreach (var file in files)
                {
                    var sourceKind = CsvAccountOperationManager.DetectSourceKindFromFileContent(file);
                    var fileOperations = await _csvAccountOperationManager.ReadAsync(file, sourceKind);
                    result.AddRange(fileOperations);
                }
            }

            return result;
        }

        private async Task<ReadAndFilterNewImportDataOnlyResult> ReadAndFilterNewImportDataOnly(
            string accountName,
            Stream importData,
            SourceKind sourceKind)
        {
            var knownOperations = await GetAllOperations(accountName);
            var knownOperationsIds = new HashSet<string>(
                knownOperations
                    .Select(_transactionPatternMapper.Apply)
                    .Select(o => o.OperationId));

            var tmpLastDate = DateTime.MinValue;
            var operationsToImport = _csvAccountOperationManager.Read(importData, sourceKind)
                .Select(
                    operation =>
                    {
                        var unifiedOperation = _transactionPatternMapper.Apply(operation);

                        if (unifiedOperation.ValueDate > tmpLastDate)
                        {
                            tmpLastDate = unifiedOperation.ValueDate;
                        }

                        return new { operation, unifiedOperation };
                    })
                .Where(o => knownOperationsIds.Add(o.unifiedOperation.OperationId))
                .Select(o => o.operation)
                .ToArray();

            if (tmpLastDate == DateTime.MinValue)
            {
                tmpLastDate = DateTime.Today;
            }

            return new ReadAndFilterNewImportDataOnlyResult
            {
                MaxDate = tmpLastDate,
                Operations = operationsToImport
            };
        }

        private class ReadAndFilterNewImportDataOnlyResult
        {
            public DateTime MaxDate { get; set; }

            public AccountOperationBase[] Operations { get; set; }
        }
    }
}