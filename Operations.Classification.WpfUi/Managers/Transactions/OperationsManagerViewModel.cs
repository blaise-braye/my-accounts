using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Unified;
using MyAccounts.Business.Managers;
using MyAccounts.Business.Managers.Operations;
using Newtonsoft.Json;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Technical.Collections.Filters;
using Operations.Classification.WpfUi.Technical.Input;
using Operations.Classification.WpfUi.Technical.Messages;
using Operations.Classification.WpfUi.Technical.Projections;

namespace Operations.Classification.WpfUi.Managers.Transactions
{
    public class OperationsManagerViewModel : ViewModelBase
    {
        private readonly CompositeFilter _anyFilter;

        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly IOperationsManager _operationsManager;
        private readonly SaveFileDialog _sfd;
        private readonly IImportManager _importManager;
        private readonly AsyncMessageReceiver _asyncMessageReceiver;

        private AccountViewModel _currentAccount;

        private string _exportFilePath;

        private bool _isExporting;

        private bool _isFiltering;
        private List<UnifiedAccountOperationModel> _operations;

        public OperationsManagerViewModel(
            BusyIndicatorViewModel busyIndicator,
            IOperationsManager operationsManager,
            IImportManager importManager)
        {
            _busyIndicator = busyIndicator;
            _operationsManager = operationsManager;
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

            _asyncMessageReceiver = new AsyncMessageReceiver(MessengerInstance);
            _asyncMessageReceiver.RegisterAsync<AccountImportDataChanged>(this, data => ReplayImports(data.AccountId));

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

        public async Task ReplayImports(params Guid[] accountIds)
        {
            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Replaying imports"))
            {
                foreach (var accountId in accountIds)
                {
                    await _importManager.ReplayCommands(accountId);
                }

                MessengerInstance.Send(new AccountDataInvalidated());
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _asyncMessageReceiver.Cleanup();
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
            var result = await _operationsManager.DetectPotentialDuplicates(CurrentAccount.Id);
            Operations = result
                .Project()
                .To<UnifiedAccountOperationModel>()
                .OrderByDescending(t => t.OperationId)
                .ToList();
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
                            clonedOperation.SourceKind = SourceKind.InternalCsvExport;
                        }

                        using (var fs = File.Open(ExportFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await _operationsManager.Export(fs, clonedOperations.Cast<AccountOperationBase>().ToList());
                        }
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

            Operations = operations
                .Project()
                .To<UnifiedAccountOperationModel>()
                .OrderByDescending(t => t.OperationId)
                .ToList();
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