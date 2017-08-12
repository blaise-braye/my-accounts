using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.WorkingCopyStorage
{
    public class OperationsRepository : IOperationsRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OperationsRepository));
        private readonly ICsvAccountOperationManager _csvAccountOperationManager;
        private readonly UnifiedAccountOperationPatternTransformer _transactionPatternMapper;
        private readonly IWorkingCopy _workingCopy;

        public OperationsRepository(
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

        public Task Export(string filePath, IList<AccountOperationBase> operations)
        {
            return _csvAccountOperationManager.WriteAsync(filePath, operations);
        }

        public async Task<List<UnifiedAccountOperation>> GetAll(Guid accountId)
        {
            var result = new List<UnifiedAccountOperation>();

            var operationsDirectory = GetAccountOperationsDirectory(accountId);
            if (Fs.Directory.Exists(operationsDirectory))
            {
                var files = Fs.Directory.GetFiles(operationsDirectory, "*.json").OrderBy(f => Fs.File.GetCreationTime(f));
                foreach (var file in files)
                {
                    Stream stream = null;
                    try
                    {
                        stream = Fs.File.OpenRead(file);
                        using (var sr = new StreamReader(stream))
                        {
                            stream = null;
                            var jsonOperations = await sr.ReadToEndAsync();
                            var operations =
                                JsonConvert.DeserializeObject<List<UnifiedAccountOperation>>(jsonOperations);
                            operations.ForEach(o => result.Insert(0, o));
                        }
                    }
                    catch
                    {
                        stream?.Dispose();
                    }
                }
            }

            return result;
        }

        public void Clear(Guid accountId)
        {
            var operationsDirectory = GetAccountOperationsDirectory(accountId);
            if (Fs.Directory.Exists(operationsDirectory))
            {
                Fs.Directory.Delete(operationsDirectory, true);
            }
        }

        public async Task<bool> ExecuteImport(ImportCommand importCommand, Stream sourceData)
        {
            try
            {
                var operationsDirectory = GetAccountOperationsDirectory(importCommand.AccountId);
                await _workingCopy.CreateFolderIfDoesNotExistsYet(operationsDirectory);

                var filteredData = await ReadAndFilterNewImportDataOnly(importCommand, sourceData);
                if (filteredData.Operations.Length > 0)
                {
                    var unifiedAccountOperations = filteredData.Operations
                        .Select(_transactionPatternMapper.Apply)
                        .Where(t => !string.IsNullOrEmpty(t.OperationId))
                        .OrderByDescending(t => t.OperationId, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var jsonOperations = JsonConvert.SerializeObject(unifiedAccountOperations, Formatting.Indented);

                    var filePrefixPath = Path.Combine(operationsDirectory, $"{filteredData.MaxDate:yyyy-MM-dd}");
                    var filePath = $"{filePrefixPath}.json";

                    var counter = 1;
                    while (Fb.Exists(filePath))
                    {
                        filePath = $"{filePrefixPath}.{counter}.json";
                    }

                    Stream stream = null;
                    try
                    {
                        stream = Fs.File.OpenWrite(filePath);
                        using (var sw = new StreamWriter(stream))
                        {
                            stream = null;
                            await sw.WriteAsync(jsonOperations);
                        }
                    }
                    catch
                    {
                        stream?.Dispose();
                        throw;
                    }
                }
            }
            catch (Exception exn)
            {
                _logger.Error("import failed", exn);
            }

            return true;
        }

        private string GetAccountOperationsDirectory(Guid accountId)
        {
            return _workingCopy.GetAbsolutePath(accountId, "operations");
        }

        private async Task<ReadAndFilterNewImportDataOnlyResult> ReadAndFilterNewImportDataOnly(
            ImportCommand importCommand, Stream sourceData)
        {
            var knownOperations = await GetAll(importCommand.AccountId);
            var knownOperationsIds = new HashSet<string>(knownOperations.Select(o => o.OperationId));

            var tmpLastDate = DateTime.MinValue;
            FileStructureMetadata fileStructureMetadata = GetFileMetadata(importCommand);

            var operationsToImport = (await _csvAccountOperationManager.ReadAsync(sourceData, fileStructureMetadata))
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

        private FileStructureMetadata GetFileMetadata(ImportCommand importCommand)
        {
            var md = _csvAccountOperationManager.GetDefaultFileMetadata(importCommand.SourceKind);

            if (!string.IsNullOrEmpty(importCommand.Culture))
            {
                md.Culture = importCommand.Culture;
            }

            if (!string.IsNullOrEmpty(importCommand.Encoding))
            {
                md.Encoding = importCommand.Encoding;
            }

            return md;
        }

        private class ReadAndFilterNewImportDataOnlyResult
        {
            public DateTime MaxDate { get; set; }

            public AccountOperationBase[] Operations { get; set; }
        }
    }
}