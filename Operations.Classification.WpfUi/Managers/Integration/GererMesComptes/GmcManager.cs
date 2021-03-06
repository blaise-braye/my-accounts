﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyAccounts.Business.AccountOperations.Unified;
using MyAccounts.Business.GererMesComptes;
using MyAccounts.Business.IO.Caching;
using MyAccounts.Business.Managers.Operations;
using MyAccounts.NetStandard.Collections;
using MyAccounts.NetStandard.Input;
using MyAccounts.NetStandard.Projections;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using QifApi.Transactions;
using OperationsRepository = MyAccounts.Business.GererMesComptes.OperationsRepository;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    public class GmcManager : ViewModelBase
    {
        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly ICacheProvider _cacheProvider;
        private readonly IOperationsManager _operationsManager;

        private Dictionary<Guid, TransactionDeltaSet> _loadedDeltas;
        private AccountViewModel _currentAccount;
        private bool _isDeltaAvailable;
        private List<BasicTransactionModel> _currentAccountBasicTransactions;
        private TransactionDeltaSet _transactionDelta;

        public GmcManager(BusyIndicatorViewModel busyIndicator, ICacheProvider cacheProvider, IOperationsManager operationsManager)
        {
            _busyIndicator = busyIndicator;
            _cacheProvider = cacheProvider;
            _operationsManager = operationsManager;
            Filter = new GmcManagerFilterViewModel();
            Filter.FilterInvalidated += OnFilterInvalidated;
            LocalTransactions = new SmartCollection<BasicTransactionModel>();
            RemoteTransactions = new SmartCollection<BasicTransactionModel>();

            RefreshRemoteKnowledgeCommand = new AsyncCommand(ComputeDeltaWithRemote);
            PushChangesToGmcCommand = new AsyncCommand(PushChangesToGmc);
            PullCategoriesFromGmcCommand = new AsyncCommand(PullCategoriesFromGmc);
            ClearCurrentAccountCacheAndResetCommand = new AsyncCommand(ClearCurrentAccountCacheAndReset);

            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);
        }

        public GmcManagerFilterViewModel Filter { get; }

        public AsyncCommand RefreshRemoteKnowledgeCommand { get; }

        public IAsyncCommand PushChangesToGmcCommand { get; }

        public IAsyncCommand PullCategoriesFromGmcCommand { get; }

        public RelayCommand ClearCurrentAccountCacheAndResetCommand { get; }

        public SmartCollection<BasicTransactionModel> RemoteTransactions { get; }

        public SmartCollection<BasicTransactionModel> LocalTransactions { get; }

        public TransactionDeltaSet TransactionDelta
        {
            get => _transactionDelta ?? (_transactionDelta = new TransactionDeltaSet());
            private set
            {
                if (Set(nameof(TransactionDelta), ref _transactionDelta, value))
                {
                    IsDeltaAvailable = _transactionDelta.LastDeltaDate.HasValue;
                }
            }
        }

        public bool IsDeltaAvailable
        {
            get => _isDeltaAvailable;
            private set
            {
                if (Set(nameof(IsDeltaAvailable), ref _isDeltaAvailable, value))
                {
                    Filter.IsDeltaFilterActive = value;
                }
            }
        }

        private AccountViewModel CurrentAccount => _currentAccount;

        public async Task InitializeAsync(IEnumerable<AccountViewModel> accounts)
        {
            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Loading account gmc transactions"))
            {
                var cachedDeltas = new Dictionary<Guid, TransactionDeltaSet>();
                foreach (var account in accounts)
                {
                    var delta = await GetDeltaCacheEntry(account).GetAsync();
                    if (delta != null)
                    {
                        cachedDeltas[account.Id] = new TransactionDeltaSet(delta);
                    }
                }

                _loadedDeltas = cachedDeltas;
            }
        }

        private static IEnumerable<BasicTransactionModel> ProjectToViewModelCollection(IEnumerable<BasicTransaction> basicTransactions)
        {
            var result = basicTransactions
                .Project().To<BasicTransactionModel>(
                    (sourceItem, targetItem) =>
                    {
                        targetItem.Memo = sourceItem.Memo;
                        targetItem.SourceItem = sourceItem;
                    })
                .OrderByDescending(d => d.Number)
                .ThenByDescending(d => d.Date);

            return result;
        }

        private async Task ClearCurrentAccountCacheAndReset()
        {
            if (CurrentAccount != null && _loadedDeltas.ContainsKey(CurrentAccount.Id))
            {
                _loadedDeltas.Remove(CurrentAccount.Id);
                await GetDeltaCacheEntry(CurrentAccount).DeleteAsync();
            }

            Reset(null);
        }

        private async Task<TransactionDeltaSet> ComputeDeltaWithRemote()
        {
            TransactionDeltaSet delta = null;

            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Computing delta with remote"))
            using (var client = new GererMesComptesClient())
            {
                if (await client.Connect(Properties.Settings.Default.GmcUserName, Properties.Settings.Default.GmcPassword))
                {
                    var accountsRepository = new AccountInfoRepository(client);
                    var account = await accountsRepository.GetByName(CurrentAccount.GmcAccountName);
                    var repository = new OperationsRepository(client);

                    var qifData = _currentAccountBasicTransactions.Select(t => t.SourceItem).ToQifData();

                    delta = TransactionDelta = await repository.DryRunImport(account.Id, qifData);
                    _loadedDeltas[CurrentAccount.Id] = delta;
                    await GetDeltaCacheEntry(CurrentAccount).SetAsync(delta.ToList());

                    RefreshTransactions();
                }
            }

            return delta;
        }

        private ICacheEntry<List<TransactionDelta>> GetDeltaCacheEntry(AccountViewModel account)
        {
            return _cacheProvider.GetJSonCacheEntry<List<TransactionDelta>>($"TransactionDeltas/{account.Name}");
        }

        private void OnAccountViewModelReceived(AccountViewModel currentAccount)
        {
            if (Set(nameof(CurrentAccount), ref _currentAccount, currentAccount))
            {
                TransactionDeltaSet delta = null;

                if (currentAccount != null && _loadedDeltas.ContainsKey(currentAccount.Id))
                {
                    delta = _loadedDeltas[currentAccount.Id];
                }

                Reset(delta);
            }
        }

        private void OnFilterInvalidated(object sender, EventArgs e)
        {
            RefreshTransactions();
        }

        private void RefreshTransactions()
        {
            if (IsDeltaAvailable)
            {
                var deltas = TransactionDelta.ToList();

                var locals = Filter.FilterDelta(deltas, t => t.Source);
                var localViewModels = ProjectToViewModelCollection(locals);
                LocalTransactions.Reset(localViewModels);

                var remotes = Filter.FilterDelta(deltas, d => d.Target);
                var remoteViewModels = ProjectToViewModelCollection(remotes);
                RemoteTransactions.Reset(remoteViewModels);
            }
            else
            {
                var locals = _currentAccountBasicTransactions;
                var filteredLocals = Filter.FilterBasicTransactions(locals);
                LocalTransactions.Reset(filteredLocals);
                RemoteTransactions.Clear();
            }
        }

        private void Reset(TransactionDeltaSet initialDeltaSet)
        {
            TransactionDelta = initialDeltaSet ?? new TransactionDeltaSet();

            var operations = CurrentAccount?.Operations ?? new List<UnifiedAccountOperation>();
            var basicTransactions = operations.ToBasicTransactions();
            _currentAccountBasicTransactions = ProjectToViewModelCollection(basicTransactions).ToList();

            RefreshTransactions();
        }

        private async Task PullCategoriesFromGmc()
        {
            var delta = TransactionDelta;
            if (!delta.LastDeltaDate.HasValue || delta.LastDeltaDate.Value < DateTime.Now.AddMinutes(-10))
            {
                delta = await ComputeDeltaWithRemote();
            }

            bool result;

            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Pulling operations categories from gmc"))
            {
                var uaoToUpdate = new List<UnifiedAccountOperation>();
                var uaoByOperationId = CurrentAccount.Operations.ToDictionary(op => op.OperationId);
                foreach (var transactionDelta in delta.ToList())
                {
                    if (transactionDelta.Action == DeltaAction.UpdateMemo
                        || transactionDelta.Action == DeltaAction.Nothing)
                    {
                        var operationId = transactionDelta.Source.Number;
                        var uao = uaoByOperationId[operationId];

                        var hasChanges = false;
                        var remoteCategory = transactionDelta.Target.Category;
                        var uselessPartIdx = remoteCategory.LastIndexOf("/", StringComparison.InvariantCulture);

                        var cleanedRemoteCategory = remoteCategory.Substring(0, uselessPartIdx).Trim('.', ' ');

                        if (uao.Category != cleanedRemoteCategory)
                        {
                            hasChanges = true;
                            transactionDelta.Source.Category = cleanedRemoteCategory;
                            uao.Category = cleanedRemoteCategory;
                        }
                        
                        if (hasChanges)
                        {
                            uaoToUpdate.Add(uao);
                        }
                    }
                }
                
                result = await _operationsManager.Update(CurrentAccount.Id, uaoToUpdate);
            }

            if (result)
            {
                RefreshTransactions();
            }
        }

        private async Task PushChangesToGmc()
        {
            var delta = TransactionDelta;
            if (!delta.LastDeltaDate.HasValue || delta.LastDeltaDate.Value < DateTime.Now.AddMinutes(-10))
            {
                delta = await ComputeDeltaWithRemote();
            }

            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Pushing new operations to gmc"))
            using (var client = new GererMesComptesClient())
            {
                if (await client.Connect(Properties.Settings.Default.GmcUserName, Properties.Settings.Default.GmcPassword))
                {
                    var accountsRepository = new AccountInfoRepository(client);
                    var account = await accountsRepository.GetByName(CurrentAccount.GmcAccountName);
                    var repository = new OperationsRepository(client);

                    var runImportResult = await repository.RunImportFromDeltaActions(account.Id, delta.ToList());
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