using System;
using System.Collections.Generic;
using System.IO;
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
        private const string UnifiedAccountOperationsByAccountRoute = "/UnifiedAccountOperations/{0}";
        private static readonly ILog _log = LogManager.GetLogger(typeof(OperationsManager));
        private readonly CompositeFilter _anyFilter;

        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly SaveFileDialog _sfd;
        private readonly IOperationsRepository _operationsRepository;
        private readonly IImportManager _importManager;

        private AccountViewModel _currentAccount;

        private string _exportFilePath;

        private bool _isExporting;

        private bool _isFiltering;
        private List<UnifiedAccountOperationModel> _operations;

        public OperationsManager(BusyIndicatorViewModel busyIndicator, IOperationsRepository operationsRepository, IImportManager importManager)
        {
            _busyIndicator = busyIndicator;
            _operationsRepository = operationsRepository;
            _importManager = importManager;
            BeginExportCommand = new RelayCommand(BeginExport);
            BeginDataQualityAnalysisCommand = new AsyncCommand(BeginDataQualityAnalysis);
            CommitExportCommand = new AsyncCommand(CommitExport);

            _sfd = new SaveFileDialog
            {
                OverwritePrompt = true,
                Filter = "csv files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

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

        public RelayCommand BeginExportCommand { get; }

        public List<UnifiedAccountOperationModel> Operations
        {
            get => _operations;
            private set => Set(nameof(Operations), ref _operations, value);
        }

        public bool IsExporting
        {
            get => _isExporting;
            set => Set(nameof(IsExporting), ref _isExporting, value);
        }

        public string ExportFilePath
        {
            get => _exportFilePath;
            set => Set(nameof(ExportFilePath), ref _exportFilePath, value);
        }

        public AccountViewModel CurrentAccount
        {
            get => _currentAccount;
            private set => Set(nameof(CurrentAccount), ref _currentAccount, value);
        }

        public RelayCommand SelectTargetFileToExportCommand { get; }

        public RelayCommand BeginDataQualityAnalysisCommand { get; }

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
                    var commands = await _importManager.GetAll(account.Id);
                    await _importManager.ReplayCommand(account.Id, commands);
                }
            }
        }

        private static ICacheEntry<List<UnifiedAccountOperation>> GetCacheEntry(Guid accountId)
        {
            return CacheProvider.GetJSonCacheEntry<List<UnifiedAccountOperation>>(string.Format(UnifiedAccountOperationsByAccountRoute, accountId));
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

        private void SelectTargetFileToExport()
        {
            if (_sfd.ShowDialog() == true)
            {
                ExportFilePath = _sfd.FileName;
            }
        }
    }
}