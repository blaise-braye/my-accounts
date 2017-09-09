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

        public async Task<List<UnifiedAccountOperation>> GetAll(Guid accountId)
        {
            var result = new List<UnifiedAccountOperation>();

            var operationsDirectory = GetAccountOperationsDirectory(accountId);
            if (Fs.DirectoryExists(operationsDirectory))
            {
                var files = Fs.DirectoryGetFiles(operationsDirectory, "*.json").OrderBy(f => Fs.FileGetCreationTime(f));
                foreach (var file in files)
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
                var operationsDirectory = GetAccountOperationsDirectory(importCommand.AccountId);
                await _workingCopy.CreateFolderIfDoesNotExistsYet(operationsDirectory);

                var filteredData = await ReadAndFilterNewImportDataOnly(importCommand, sourceData);
                result.NotCompliant = filteredData.NotCompliant;
                result.AlreadyKnown = filteredData.AlreadyKnown;
                result.NewOperations = filteredData.NewOperations.Length;

                if (filteredData.NewOperations.Length > 0)
                {
                    var newOperations = filteredData.NewOperations
                        .OrderByDescending(t => t.OperationId, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var jsonOperations = JsonConvert.SerializeObject(newOperations, Formatting.Indented);

                    var filePrefixPath = Path.Combine(operationsDirectory, $"{filteredData.MaxDate:yyyy-MM-dd}");
                    var filePath = $"{filePrefixPath}.json";

                    var counter = 1;
                    while (Fs.FileExists(filePath))
                    {
                        filePath = $"{filePrefixPath}.{counter}.json";
                    }

                    Stream stream = null;
                    try
                    {
                        stream = Fs.FileCreate(filePath);
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
                result.Error = exn.Message;
                _logger.Error("import failed", exn);
            }

            return result;
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

            var operations = await _csvAccountOperationManager.ReadAsync(sourceData, fileStructureMetadata);

            var result = new ReadAndFilterNewImportDataOnlyResult();
            var operationsToImport = operations
                .Select(
                    operation =>
                    {
                        var unifiedOperation = _transactionPatternMapper.Apply(operation);

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
                        else if (unifiedOperation.ValueDate > tmpLastDate)
                        {
                            tmpLastDate = unifiedOperation.ValueDate;
                        }

                        return unifiedOperation;
                    })
                .Where(o => o != null)
                .ToArray();

            if (tmpLastDate == DateTime.MinValue)
            {
                tmpLastDate = DateTime.Today;
            }

            result.MaxDate = tmpLastDate;
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

            return md;
        }

        private class ReadAndFilterNewImportDataOnlyResult
        {
            public DateTime MaxDate { get; set; }

            public UnifiedAccountOperation[] NewOperations { get; set; }

            public int NotCompliant { get; set; }

            public int AlreadyKnown { get; set; }
        }
    }
}