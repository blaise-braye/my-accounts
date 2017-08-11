using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using Microsoft.Win32;
using Newtonsoft.Json;
using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.WorkingCopyStorage;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Technical.Caching;
using Operations.Classification.WpfUi.Technical.Collections.Filters;
using Operations.Classification.WpfUi.Technical.Input;
using Operations.Classification.WpfUi.Technical.Projections;

namespace Operations.Classification.WpfUi.Managers.Transactions
{
    public class OperationsManager : ViewModelBase, IOperationsManager
    {
        private const string UnifiedAccountOperationsByNameRoute = "/UnifiedAccountOperations/{0}";
        private static readonly ILog _log = LogManager.GetLogger(typeof(OperationsManager));
        private readonly CompositeFilter _anyFilter;

        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly OpenFileDialog _ofd;
        private readonly SaveFileDialog _sfd;
        private readonly IOperationsRepository _operationsRepository;
        private readonly AccountCommandQueue _accountCommandQueue;

        private bool _autoDetectSourceKind;
        private AccountViewModel _currentAccount;

        private string _exportFilePath;

        private string _filePaths;

        private bool _isExporting;

        private bool _isFiltering;
        private bool _isImporting;
        private List<UnifiedAccountOperationModel> _operations;
        private SourceKind? _sourceKind;

        public OperationsManager(BusyIndicatorViewModel busyIndicator, IFileSystem fileSystem, IOperationsRepository operationsRepository, AccountCommandQueue accountCommandQueue)
        {
            _busyIndicator = busyIndicator;
            Fs = fileSystem;
            _operationsRepository = operationsRepository;
            _accountCommandQueue = accountCommandQueue;
            BeginImportCommand = new RelayCommand(BeginImport);
            BeginExportCommand = new RelayCommand(BeginExport);
            BeginDataQualityAnalysisCommand = new AsyncCommand(BeginDataQualityAnalysis);
            CommitImportCommand = new AsyncCommand(CommitImport);
            CommitExportCommand = new AsyncCommand(CommitExport);

            _ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "csv files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            _sfd = new SaveFileDialog
            {
                OverwritePrompt = true,
                Filter = "csv files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            SelectFilesToImportCommand = new RelayCommand(SelectFilesToImport);
            SelectTargetFileToExportCommand = new RelayCommand(SelectTargetFileToExport);
            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);
            DateFilter = new DateRangeFilter();
            NoteFilter = new TextFilter();
            _anyFilter = new CompositeFilter(DateFilter, NoteFilter);
            _anyFilter.FilterInvalidated += OnAnyFilterInvalidated;

            ResetFilterCommad = new RelayCommand(() => _anyFilter.Reset());
        }

        public AsyncCommand CommitExportCommand { get; }

        public TextFilter NoteFilter { get; }

        public DateRangeFilter DateFilter { get; }

        public RelayCommand ResetFilterCommad { get; }

        public RelayCommand CommitImportCommand { get; }

        public RelayCommand BeginImportCommand { get; }

        public RelayCommand BeginExportCommand { get; }

        public List<UnifiedAccountOperationModel> Operations
        {
            get => _operations;
            private set => Set(nameof(Operations), ref _operations, value);
        }

        public bool IsImporting
        {
            get => _isImporting;
            set => Set(nameof(IsImporting), ref _isImporting, value);
        }

        public bool IsExporting
        {
            get => _isExporting;
            set => Set(nameof(IsExporting), ref _isExporting, value);
        }

        public string FilePaths
        {
            get => _filePaths;
            set => Set(nameof(FilePaths), ref _filePaths, value);
        }

        public string ExportFilePath
        {
            get => _exportFilePath;
            set => Set(nameof(ExportFilePath), ref _exportFilePath, value);
        }

        public SourceKind? SourceKind
        {
            get => _sourceKind;
            set => Set(nameof(SourceKind), ref _sourceKind, value);
        }

        public IEnumerable<SourceKind> SourceKinds => Enum.GetValues(typeof(SourceKind)).Cast<SourceKind>();

        public AccountViewModel CurrentAccount
        {
            get => _currentAccount;
            set => Set(nameof(CurrentAccount), ref _currentAccount, value);
        }

        public RelayCommand SelectFilesToImportCommand { get; }

        public RelayCommand SelectTargetFileToExportCommand { get; }

        public RelayCommand BeginDataQualityAnalysisCommand { get; }

        public bool AutoDetectSourceKind
        {
            get => _autoDetectSourceKind;
            set
            {
                if (Set(nameof(AutoDetectSourceKind), ref _autoDetectSourceKind, value))
                {
                    if (value)
                    {
                        SourceKind = null;
                    }
                }
            }
        }

        public bool IsFiltering
        {
            get => _isFiltering;
            set
            {
                if (Set(nameof(IsFiltering), ref _isFiltering, value))
                {
                    RefreshIsFilteringState();
                }
            }
        }

        private IFileSystem Fs { get; }

        private FileBase Fb => Fs.File;

        public async Task<List<UnifiedAccountOperation>> GetTransformedUnifiedOperations(Guid accountId)
        {
            var result = await GetCacheEntry(accountId).GetOrAddAsync(
                () => _operationsRepository.GetAll(accountId));
            return result;
        }

        public async Task ReplayImports(IList<AccountViewModel> accounts)
        {
            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Replaying imports"))
            {
                foreach (var account in accounts)
                {
                    var commands = await _accountCommandQueue.GetAll(account.Id);
                    await _operationsRepository.ReplayCommand(account.Id, commands);
                }
            }
        }

        private static ICacheEntry<List<UnifiedAccountOperation>> GetCacheEntry(Guid accountId)
        {
            return CacheProvider.GetJSonCacheEntry<List<UnifiedAccountOperation>>(string.Format(UnifiedAccountOperationsByNameRoute, accountId));
        }

        private void OnAnyFilterInvalidated(object sender, EventArgs e)
        {
            RefreshIsFilteringState();
            RefreshOperations();
        }

        private void RefreshIsFilteringState()
        {
            IsFiltering = _anyFilter.IsActive();
        }

        private async Task BeginDataQualityAnalysis()
        {
            var operations = await GetTransformedUnifiedOperations(CurrentAccount.Id);

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

            Operations = result.Project()?.To<UnifiedAccountOperationModel>()?.ToList();
        }

        private void BeginImport()
        {
            IsImporting = true;
        }

        private void BeginExport()
        {
            IsExporting = true;
        }

        private async Task CommitExport()
        {
            if (IsExporting)
            {
                using (_busyIndicator.EncapsulateActiveJobDescription(this, "exporting active selection"))
                {
                    if (!string.IsNullOrEmpty(ExportFilePath))
                    {
                        var operations = GetFilteredAccountOperations();
                        var clonedOperations =
                            JsonConvert.DeserializeObject<List<UnifiedAccountOperation>>(JsonConvert.SerializeObject(operations));
                        foreach (var clonedOperation in clonedOperations)
                        {
                            clonedOperation.SourceKind = AccountOperations.Contracts.SourceKind.InternalCsvExport;
                        }

                        await _operationsRepository.Export(ExportFilePath, clonedOperations.Cast<AccountOperationBase>().ToList());
                    }
                }

                IsExporting = false;
            }
        }

        private async Task CommitImport()
        {
            if (IsImporting)
            {
                using (_busyIndicator.EncapsulateActiveJobDescription(this, "finalizing import"))
                {
                    var paths = FilePaths.Split(',');
                    var files = new HashSet<string>();
                    foreach (var path in paths)
                    {
                        if ((Fb.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            var dirFiles = Directory.GetFiles(path, "*.csv");
                            foreach (var dirFile in dirFiles)
                            {
                                files.Add(dirFile);
                            }
                        }
                        else if (Fb.Exists(path))
                        {
                            files.Add(path);
                        }
                    }

                    var account = CurrentAccount;
                    var someImportSucceeded = false;
                    if (account != null)
                    {
                        foreach (var file in files)
                        {
                            var sourceKind = AccountOperations.Contracts.SourceKind.Unknwon;

                            if (AutoDetectSourceKind)
                            {
                                sourceKind = CsvAccountOperationManager.DetectFileSourceKindFromFileName(file);
                            }
                            else if (SourceKind.HasValue)
                            {
                                sourceKind = SourceKind.Value;
                            }

                            using (var fs = Fb.OpenRead(file))
                            {
                                var importCommand = new ImportCommand(account.Id, Path.GetFileName(file), sourceKind);
                                if (await _operationsRepository.RequestImportExecution(importCommand, fs))
                                {
                                    await GetCacheEntry(account.Id).DeleteAsync();
                                    someImportSucceeded = true;
                                }
                            }
                        }
                    }

                    if (someImportSucceeded)
                    {
                        var operations = await GetTransformedUnifiedOperations(CurrentAccount.Id);
                        CurrentAccount.Operations = operations;
                        OnAccountViewModelReceived(CurrentAccount);
                    }
                }

                IsImporting = false;
            }
        }

        private void OnAccountViewModelReceived(AccountViewModel currentAccount)
        {
            CurrentAccount = currentAccount;
            RefreshOperations();
        }

        private void RefreshOperations()
        {
            var operations = GetFilteredAccountOperations();

            Operations = operations.Project().To<UnifiedAccountOperationModel>().ToList();
        }

        private IEnumerable<UnifiedAccountOperation> GetFilteredAccountOperations()
        {
            var operations = CurrentAccount?.Operations?.AsEnumerable() ?? new List<UnifiedAccountOperation>();
            operations = DateFilter.Apply(operations, op => op.ExecutionDate);
            operations = NoteFilter.Apply(operations, op => op.Note);
            return operations;
        }

        private void SelectFilesToImport()
        {
            if (string.IsNullOrEmpty(_ofd.InitialDirectory))
            {
                _ofd.InitialDirectory = Properties.Settings.Default.WorkingFolder;
            }

            if (_ofd.ShowDialog() == true)
            {
                FilePaths = string.Join(",", _ofd.FileNames);
            }
        }

        private void SelectTargetFileToExport()
        {
            if (_sfd.ShowDialog() == true)
            {
                ExportFilePath = _sfd.FileName;
            }
        }
    }
}