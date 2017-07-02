using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using Microsoft.Win32;
using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.WpfUi.Data;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Technical.Input;
using Operations.Classification.WpfUi.Technical.Projections;

namespace Operations.Classification.WpfUi.Managers.Transactions
{
    public class TransactionsManager : ViewModelBase
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(TransactionsManager));

        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly OpenFileDialog _ofd;
        private readonly ITransactionsRepository _transactionsRepository;

        private bool _autoDetectSourceKind;
        private AccountViewModel _currentAccount;

        private string _filePaths;
        private bool _isImporting;
        private List<UnifiedAccountOperationModel> _operations;
        private SourceKind? _sourceKind;

        public TransactionsManager(BusyIndicatorViewModel busyIndicator, ITransactionsRepository transactionsRepository)
        {
            _busyIndicator = busyIndicator;
            _transactionsRepository = transactionsRepository;
            BeginImportCommand = new RelayCommand(BeginImport);
            BeginDataQualityAnalysisCommand = new AsyncCommand(BeginDataQualityAnalysis);
            CommitImportCommand = new AsyncCommand(CommitImport);

            _ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "csv files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            SelectFilesToImportCommand = new RelayCommand(SelectFilesToImport);
            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);
        }

        public RelayCommand CommitImportCommand { get; }

        public RelayCommand BeginImportCommand { get; }

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

        public string FilePaths
        {
            get => _filePaths;
            set => Set(nameof(FilePaths), ref _filePaths, value);
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

        public RelayCommand BeginDataQualityAnalysisCommand { get; }

        public bool AutoDetectSourceKind
        {
            get => _autoDetectSourceKind;
            set
            {
                if (Set(nameof(AutoDetectSourceKind), ref _autoDetectSourceKind, value))
                    if (value)
                        SourceKind = null;
            }
        }

        private async Task BeginDataQualityAnalysis()
        {
            var operations = await _transactionsRepository.GetTransformedUnifiedOperations(CurrentAccount.Name);

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
                      .OrderBy(op=>op.OperationId);
            int[] previousOperationId = null;
            foreach (var fortisOperation in fortisOperations)
            {
                var operationIdParts = fortisOperation.OperationId.Split('-');
                var operationYear = int.Parse(operationIdParts[0]);
                var operationYearNumber = int.Parse(operationIdParts[1]);

                if (previousOperationId!=null)
                {
                    var prevOpYear = previousOperationId[0];
                    var prevOpYearNumber = previousOperationId[1];
                    bool sequenceAsExpectated = true;
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
                        _log.Error($"operation id sequence mismatch (previous {string.Join("-", previousOperationId)}, current {string.Join("-", operationIdParts)}");
                    }
                }
                
                previousOperationId = new[] { operationYear, operationYearNumber };
            }

            Operations = result.Project()?.To<UnifiedAccountOperationModel>()?.ToList();
        }

        private void BeginImport()
        {
            IsImporting = true;
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
                        if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            var dirFiles = Directory.GetFiles(path, "*.csv");
                            foreach (var dirFile in dirFiles)
                                files.Add(dirFile);
                        }
                        else if (File.Exists(path))
                        {
                            files.Add(path);
                        }

                    var account = CurrentAccount;
                    var someImportSucceeded = false;
                    if (account != null)
                        foreach (var file in files)
                        {
                            var sourceKind = AccountOperations.Contracts.SourceKind.Unknwon;

                            if (AutoDetectSourceKind)
                                sourceKind = CsvAccountOperationManager.DetectFileSourceKindFromFileName(file);
                            else if (SourceKind.HasValue)
                                sourceKind = SourceKind.Value;

                            using (var fs = File.OpenRead(file))
                            {
                                if (await _transactionsRepository.Import(account.Name, fs, sourceKind))
                                    someImportSucceeded = true;
                            }
                        }

                    if (someImportSucceeded)
                    {
                        var operations = await _transactionsRepository.GetTransformedUnifiedOperations(CurrentAccount.Name);
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
            Operations = currentAccount?.Operations?.Project()?.To<UnifiedAccountOperationModel>()?.ToList();
        }

        private void SelectFilesToImport()
        {
            if (string.IsNullOrEmpty(_ofd.InitialDirectory))
                _ofd.InitialDirectory = Properties.Settings.Default.WorkingFolder;

            if (_ofd.ShowDialog() == true)
                FilePaths = string.Join(",", _ofd.FileNames);
        }
    }
}