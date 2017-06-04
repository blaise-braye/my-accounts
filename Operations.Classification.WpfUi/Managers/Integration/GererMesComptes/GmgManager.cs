using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.GererMesComptes;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Properties;
using Operations.Classification.WpfUi.Technical.Collections;
using Operations.Classification.WpfUi.Technical.Input;
using QifApi.Transactions;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    public class GmgManager : ViewModelBase
    {
        private readonly BusyIndicatorViewModel _busyIndicator;
        private AccountViewModel _currentAccount;
        private TransactionDeltaSet _transactionDelta;
        private bool _isDeltaAvailable;
        private bool _isFiltering;

        public GmgManager(BusyIndicatorViewModel busyIndicator)
        {
            _busyIndicator = busyIndicator;

            RefreshRemoteKnowledgeCommand = new AsyncCommand(ComputeDeltaWithRemote);
            SynchronizeCommand = new AsyncCommand(Synchronize);
            ResetCommand = new RelayCommand(Reset);
            Transactions = new SmartCollection<BasicTransactionModel>();
            RemoteTransactions = new SmartCollection<BasicTransactionModel>();
            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);

            TransactionDeltaFilterItems = new TransactionDeltaFilter();
            TransactionDeltaFilterItems.FilterItemChanged += OnTransactionDeltaFilterItemChanged;
        }

        public AccountViewModel CurrentAccount
        {
            get { return _currentAccount; }
            set
            {
                if (Set(nameof(CurrentAccount), ref _currentAccount, value))
                    Reset();
            }
        }

        public AsyncCommand RefreshRemoteKnowledgeCommand { get; }

        public IAsyncCommand SynchronizeCommand { get; }

        public RelayCommand ResetCommand { get; }

        public SmartCollection<BasicTransactionModel> RemoteTransactions { get; }

        public SmartCollection<BasicTransactionModel> Transactions { get; }

        public TransactionDeltaSet TransactionDelta
        {
            get { return _transactionDelta ?? (_transactionDelta = new TransactionDeltaSet()); }
            private set
            {
                if (Set(nameof(TransactionDelta), ref _transactionDelta, value))
                    IsDeltaAvailable = _transactionDelta.LastActionInsertTime.HasValue;
            }
        }

        public bool IsDeltaAvailable
        {
            get { return _isDeltaAvailable; }
            private set { Set(nameof(IsDeltaAvailable), ref _isDeltaAvailable, value); }
        }

        public bool IsFiltering
        {
            get { return _isFiltering; }
            set
            {
                if (Set(nameof(IsFiltering), ref _isFiltering, value))
                    RefreshIsFilteringState();
            }
        }

        public TransactionDeltaFilter TransactionDeltaFilterItems { get; }

        private void OnTransactionDeltaFilterItemChanged(object sender, EventArgs e)
        {
            RefreshIsFilteringState();
            RefreshTransactionsFromDelta();
        }

        private void RefreshIsFilteringState()
        {
            IsFiltering = TransactionDeltaFilterItems.IsActive();
        }

        private void RefreshTransactionsFromDelta()
        {
            var deltas = TransactionDelta.ToList();
            
            if (TransactionDeltaFilterItems.IsActive())
            {
                var filterScope = TransactionDeltaFilterItems.BuildFilterScope();
                deltas = deltas.Where(d => filterScope.Contains(d.Action)).ToList();
            }

            var locals = deltas.Where(d => d.Source != null).Select(d => d.Source);
            var vmLocals = ProjectToViewModelCollection(locals);
            Transactions.Reset(vmLocals);

            var remotes = deltas.Where(d => d.Target != null).Select(d => d.Target);
            var vmRemotes = ProjectToViewModelCollection(remotes);
            RemoteTransactions.Reset(vmRemotes);
        }

        private IEnumerable<BasicTransactionModel> ComputeTransactions()
        {
            var operations = CurrentAccount?.Operations ?? new List<UnifiedAccountOperation>();
            var basicTransactions = operations.ToBasicTransactions();

            var basicTransactionViewModels = ProjectToViewModelCollection(basicTransactions);
            return basicTransactionViewModels;
        }

        private static List<BasicTransactionModel> ProjectToViewModelCollection(IEnumerable<BasicTransaction> basicTransactions)
        {
            return basicTransactions.Select(
                    transaction =>
                    {
                        var vm = Mapper.Map<BasicTransaction, BasicTransactionModel>(transaction);
                        vm.Memo = transaction.Memo;
                        vm.SourceItem = transaction;
                        return vm;
                    })
                .OrderByDescending(d => d.Number)
                .ThenByDescending(d => d.Date)
                .ToList();
        }

        private void OnAccountViewModelReceived(AccountViewModel currentAccount)
        {
            CurrentAccount = currentAccount;
            Reset();
        }

        private void Reset()
        {
            TransactionDeltaFilterItems.Reset();
            Transactions.Reset(ComputeTransactions());
            RemoteTransactions.Clear();
            TransactionDelta = new TransactionDeltaSet();
        }

        private async Task<TransactionDeltaSet> ComputeDeltaWithRemote()
        {
            TransactionDeltaSet delta = null;

            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Computing delta with remote"))
            using (var client = new GererMesComptesClient())
            {
                if (await client.Connect(Settings.Default.GmgUserName, Settings.Default.GmgPassword))
                {
                    var accountsRepository = new AccountInfoRepository(client);
                    var account = await accountsRepository.GetByName(CurrentAccount.GmgAccountName);
                    var repository = new OperationsRepository(client);

                    var qifData = Transactions.Select(t => t.SourceItem).ToQifData();

                    delta = TransactionDelta = await repository.DryRunImport(account.Id, qifData);
                    RefreshTransactionsFromDelta();
                }
            }

            return delta;
        }

        private async Task Synchronize()
        {
            var delta = TransactionDelta;
            if (!delta.LastActionInsertTime.HasValue || delta.LastActionInsertTime.Value < DateTime.Now.AddMinutes(-10))
                delta = await ComputeDeltaWithRemote();

            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Synchronizing data"))
            using (var client = new GererMesComptesClient())
            {
                if (await client.Connect(Settings.Default.GmgUserName, Settings.Default.GmgPassword))
                {
                    var accountsRepository = new AccountInfoRepository(client);
                    var account = await accountsRepository.GetByName(CurrentAccount.GmgAccountName);
                    var repository = new OperationsRepository(client);

                    var runImportResult = await repository.RunImport(account.Id, delta.ToList());
                    if (runImportResult.Success)
                    {
                        using (_busyIndicator.EncapsulateActiveJobDescription(this, "Waiting synchronized data is available on remote"))
                        {
                            await repository.WaitExportAvailability(account.Id, runImportResult.ImportedQifData);
                        }

                        await ComputeDeltaWithRemote();
                    }
                }
            }
        }
    }
}