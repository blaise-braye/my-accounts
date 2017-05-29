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

        private bool _isDeltaAvailable;

        private TransactionDeltaSet _transactionDelta;

        public GmgManager(BusyIndicatorViewModel busyIndicator)
        {
            _busyIndicator = busyIndicator;
            RefreshRemoteKnowledgeCommand = new AsyncCommand(ComputeDeltaWithRemote);
            SynchronizeCommand = new AsyncCommand(Synchronize);
            ResetCommand = new RelayCommand(Reset);
            Transactions = new SmartCollection<BasicTransactionModel>();
            RemoteTransactions = new SmartCollection<BasicTransactionModel>();
            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);
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
            Transactions.Reset(ComputeTransactions());
            RemoteTransactions.Clear();
            TransactionDelta = new TransactionDeltaSet();
        }

        private async Task<TransactionDeltaSet> ComputeDeltaWithRemote()
        {
            TransactionDeltaSet delta = null;

            using(_busyIndicator.EncapsulateActiveJobDescription(this, "Computing delta with remote"))
            using (var client = new GererMesComptesClient())
            {
                if (await client.Connect(Settings.Default.GmgUserName, Settings.Default.GmgPassword))
                {
                    var accountsRepository = new AccountInfoRepository(client);
                    var account = await accountsRepository.GetByName(CurrentAccount.GmgAccountName);
                    var repository = new OperationsRepository(client);

                    var qifData = Transactions.Select(t => t.SourceItem).ToQifData();

                    delta = TransactionDelta = await repository.DryRunImport(account.Id, qifData);
                    var vmRemotes = ProjectToViewModelCollection(TransactionDelta.GetRemoteTransactions());
                    RemoteTransactions.Clear();
                    RemoteTransactions.AddRange(vmRemotes);
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