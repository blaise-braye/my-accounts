using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using MyAccounts.Business.AccountOperations;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Unified;
using MyAccounts.Business.IO;
using MyAccounts.Business.Managers.Imports;
using Newtonsoft.Json;

namespace MyAccounts.Business.Managers.Operations
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

        public Task Export(Stream target, IList<AccountOperationBase> operations)
        {
            return _csvAccountOperationManager.WriteAsync(target, operations);
        }

        public async Task<bool> Update(Guid accountId, IList<UnifiedAccountOperation> operations)
        {
            var result = true;
            var files = GetFilePaths(accountId, operations);
            var existingOperations = await ReadOperations(files);
            var operationsById = existingOperations.ToDictionary(op => op.UId);
            foreach (var operation in operations)
            {
                if (!operationsById.ContainsKey(operation.UId))
                {
                    result = false;
                    _logger.Error($"Operation with Uid {operation.UId} does not exist");
                }
                else
                {
                    operationsById[operation.UId] = operation;
                }
            }

            if (result)
            {
                var operationsToWrite = operationsById.Values;

                result = await WriteOperations(accountId, operationsToWrite);
            }
            
            return result;
        }
        
        public async Task<List<UnifiedAccountOperation>> GetAll(Guid accountId)
        {
            var result = new List<UnifiedAccountOperation>();

            var operationsDirectory = GetAccountOperationsDirectory(accountId);
            if (Fs.DirectoryExists(operationsDirectory))
            {
                var files = Fs.DirectoryGetFiles(operationsDirectory, "*.json").ToArray();
                var operations = await ReadOperations(files);
                result.AddRange(operations.OrderByDescending(op => op.ValueDate));
            }

            return result;
        }

        public void Clear(Guid accountId)
        {
            var operationsDirectory = GetAccountOperationsDirectory(accountId);
            if (Fs.DirectoryExists(operationsDirectory))
            {
                Fs.DirectoryDelete(operationsDirectory, true);
            }
        }

        public async Task<ImportExecutionImpact> ExecuteImport(ImportCommand importCommand, Stream sourceData)
        {
            var result = new ImportExecutionImpact { CommandId = importCommand.Id };

            try
            {
                var filteredData = await ReadAndFilterNewImportDataOnly(importCommand, sourceData);
                result.NotCompliant = filteredData.NotCompliant;
                result.AlreadyKnown = filteredData.AlreadyKnown;
                result.NewOperations = filteredData.NewOperations.Length;

                if (filteredData.NewOperations.Length > 0)
                {
                    // Merge new operations with existing operations
                    var newOperations = filteredData.NewOperations;
                    var operationsFiles = GetFilePaths(importCommand.AccountId, newOperations);

                    var existingOperations = await ReadOperations(operationsFiles);
                    
                    var operationsToWrite = existingOperations.Concat(newOperations).ToList();

                    await WriteOperations(importCommand.AccountId, operationsToWrite);
                }
            }
            catch (Exception exn)
            {
                result.Error = exn.Message;
                _logger.Error("import failed", exn);
            }

            return result;
        }

        private string[] GetFilePaths(Guid accountId, IEnumerable<UnifiedAccountOperation> newOperations)
        {
            var operationsFiles = newOperations
                .Select(t => GetFilePathByValueDate(accountId, t.ValueDate))
                .Distinct()
                .ToArray();
            return operationsFiles;
        }

        private string GetFilePathByValueDate(Guid accountId, DateTime valueDate)
        {
            var operationsDirectory = GetAccountOperationsDirectory(accountId);

            string operationFile = string.Empty;
            
            if (string.IsNullOrEmpty(operationFile))
            {
                operationFile = Path.Combine(operationsDirectory, $"{valueDate:yyyy-MM}.json");
            }

            return operationFile;
        }
        
        private Task<List<UnifiedAccountOperation>> ReadOperations(params string[] files)
        {
            return Task.WhenAll(files.Select(async file =>
            {
                var result = new List<UnifiedAccountOperation>();
                if (Fs.FileExists(file))
                {
                    Stream stream = null;
                    try
                    {
                        stream = Fs.FileOpenRead(file);
                        using (var sr = new StreamReader(stream))
                        {
                            stream = null;
                            var jsonOperations = await sr.ReadToEndAsync();
                            var operations =
                                JsonConvert.DeserializeObject<List<UnifiedAccountOperation>>(jsonOperations);
                            result.AddRange(operations);
                        }
                    }
                    catch
                    {
                        stream?.Dispose();
                    }
                }

                return result;
            })).ContinueWith(task => task.Result.SelectMany(list => list).ToList());
        }

        /// <summary>
        /// the written operations will override the operations of the related months
        /// </summary>
        /// <param name="accountId">account id</param>
        /// <param name="operationsToWrite">operations to write</param>
        /// <returns>A task</returns>
        private async Task<bool> WriteOperations(Guid accountId, IEnumerable<UnifiedAccountOperation> operationsToWrite)
        {
            var operationsDirectory = GetAccountOperationsDirectory(accountId);
            await _workingCopy.CreateFolderIfDoesNotExistsYet(operationsDirectory);

            var operationsToTargetFile = operationsToWrite
                .OrderByDescending(t => t.OperationId, StringComparer.OrdinalIgnoreCase)
                .ToLookup(t => GetFilePathByValueDate(accountId, t.ValueDate));

            var fileTasks = await Task.WhenAll(operationsToTargetFile.Select(async (fileAndOperations) =>
            {
                var file = fileAndOperations.Key;
                var operations = fileAndOperations.ToList();

                var jsonOperations = JsonConvert.SerializeObject(operations, Formatting.Indented);

                Stream stream = null;
                try
                {
                    stream = Fs.FileCreate(file);
                    using (var sw = new StreamWriter(stream))
                    {
                        stream = null;
                        await sw.WriteAsync(jsonOperations);
                    }

                    return true;
                }
                catch (Exception exn)
                {
                    stream?.Dispose();
                    _logger.Error($"Failed to write operations to file {file}", exn);
                    return false;
                }
            }));

            return fileTasks.All(b => b);
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
            
            FileStructureMetadata fileStructureMetadata = GetFileMetadata(importCommand);

            var operations = await _csvAccountOperationManager.ReadAsync(sourceData, fileStructureMetadata);

            var result = new ReadAndFilterNewImportDataOnlyResult();
            var operationsToImport = operations
                .Select(
                    operation =>
                    {
                        var unifiedOperation = _transactionPatternMapper.Apply(operation, fileStructureMetadata.GetCultureInfo());

                        if (string.IsNullOrEmpty(unifiedOperation.OperationId))
                        {
                            result.NotCompliant++;
                            unifiedOperation = null;
                        }
                        else if (!knownOperationsIds.Add(unifiedOperation.OperationId))
                        {
                            result.AlreadyKnown++;
                            unifiedOperation = null;
                        }

                        return unifiedOperation;
                    })
                .Where(o => o != null)
                .ToArray();
            
            result.NewOperations = operationsToImport;
            return result;
        }

        private FileStructureMetadata GetFileMetadata(ImportCommand importCommand)
        {
            var md = FileStructureMetadataFactory.CreateDefault(importCommand.SourceKind);

            if (!string.IsNullOrEmpty(importCommand.Culture))
            {
                md.Culture = importCommand.Culture;
            }

            if (!string.IsNullOrEmpty(importCommand.Encoding))
            {
                md.Encoding = importCommand.Encoding;
            }

            if (!string.IsNullOrEmpty(importCommand.DecimalSeparator))
            {
                md.DecimalSeparator = importCommand.DecimalSeparator;
            }

            return md;
        }

        private class ReadAndFilterNewImportDataOnlyResult
        {
            public UnifiedAccountOperation[] NewOperations { get; set; }

            public int NotCompliant { get; set; }

            public int AlreadyKnown { get; set; }
        }
    }
}