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
using Operations.Classification.WpfUi.Technical.Caching;
using Operations.Classification.WpfUi.Technical.Collections;
using Operations.Classification.WpfUi.Technical.Input;
using QifApi.Transactions;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    public class GmgManager : ViewModelBase
    {
        private readonly CompositeFilter _anyFilter;
        private readonly BusyIndicatorViewModel _busyIndicator;

        private Dictionary<Guid, TransactionDeltaSet> _loadedDeltas;
        private AccountViewModel _currentAccount;
        private bool _isDeltaAvailable;
        private bool _isFiltering;
        private List<BasicTransactionModel> _currentAccountBasicTransactions;
        private TransactionDeltaSet _transactionDelta;

        public GmgManager(BusyIndicatorViewModel busyIndicator)
        {
            _busyIndicator = busyIndicator;

            Transactions = new SmartCollection<BasicTransactionModel>();
            RemoteTransactions = new SmartCollection<BasicTransactionModel>();

            RefreshRemoteKnowledgeCommand = new AsyncCommand(ComputeDeltaWithRemote);
            SynchronizeCommand = new AsyncCommand(Synchronize);
            ClearCurrentAccountCacheAndResetCommand = new AsyncCommand(ClearCurrentAccountCacheAndReset);

            DeltaFilter = new TransactionDeltaFilter();
            DateFilter = new DateRangeFilter();
            _anyFilter = new CompositeFilter(DeltaFilter, DateFilter);
            _anyFilter.FilterInvalidated += OnAnyFilterInvalidated;

            ResetFilterCommad = new RelayCommand(() => _anyFilter.Reset());
            FilterOnItemDateCommand = new RelayCommand<BasicTransactionModel>(FilterOnItemDate);

            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);
        }

        public AccountViewModel CurrentAccount => _currentAccount;

        public AsyncCommand RefreshRemoteKnowledgeCommand { get; }

        public IAsyncCommand SynchronizeCommand { get; }

        public RelayCommand ClearCurrentAccountCacheAndResetCommand { get; }

        public SmartCollection<BasicTransactionModel> RemoteTransactions { get; }

        public SmartCollection<BasicTransactionModel> Transactions { get; }

        public TransactionDeltaSet TransactionDelta
        {
            get { return _transactionDelta ?? (_transactionDelta = new TransactionDeltaSet()); }
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
            get { return _isDeltaAvailable; }
            private set { Set(nameof(IsDeltaAvailable), ref _isDeltaAvailable, value); }
        }

        public bool IsFiltering
        {
            get { return _isFiltering; }
            set
            {
                if (Set(nameof(IsFiltering), ref _isFiltering, value))
                {
                    RefreshIsFilteringState();
                }
            }
        }

        public TransactionDeltaFilter DeltaFilter { get; }

        public DateRangeFilter DateFilter { get; }

        public RelayCommand<BasicTransactionModel> FilterOnItemDateCommand { get; }

        public RelayCommand ResetFilterCommad { get; }

        public async Task InitializeAsync(IEnumerable<AccountViewModel> accounts)
        {
            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Loading account gmg transactions"))
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
                if (await client.Connect(Settings.Default.GmgUserName, Settings.Default.GmgPassword))
                {
                    var accountsRepository = new AccountInfoRepository(client);
                    var account = await accountsRepository.GetByName(CurrentAccount.GmgAccountName);
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

        private void FilterOnItemDate(BasicTransactionModel obj)
        {
            _anyFilter.Apply(
                () =>
                {
                    _anyFilter.Reset();
                    DateFilter.SetDayFilter(obj.Date);
                });
        }

        private JSonCacheEntry<List<TransactionDelta>> GetDeltaCacheEntry(AccountViewModel account)
        {
            return CacheProvider.GetJSonCacheEntry<List<TransactionDelta>>($"TransactionDeltas/{account.Name}");
        }

        private void OnAccountViewModelReceived(AccountViewModel currentAccount)
        {
            if (Set(nameof(CurrentAccount), ref _currentAccount, currentAccount))
            {
                TransactionDeltaSet delta = null;

                if (_loadedDeltas.ContainsKey(CurrentAccount.Id))
                {
                    delta = _loadedDeltas[CurrentAccount.Id];
                }

                Reset(delta);
            }
        }

        private void OnAnyFilterInvalidated(object sender, EventArgs e)
        {
            RefreshIsFilteringState();
            RefreshTransactions();
        }

        private static IEnumerable<BasicTransactionModel> ProjectToViewModelCollection(IEnumerable<BasicTransaction> basicTransactions)
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
                .ThenByDescending(d => d.Date);
        }

        private void RefreshIsFilteringState()
        {
            IsFiltering = _anyFilter.IsActive();
        }

        private void RefreshTransactions()
        {
            if (IsDeltaAvailable)
            {
                var deltas = TransactionDelta.ToList();
                deltas = DeltaFilter.Apply(deltas, d => d.Action).ToList();

                var locals = deltas.Where(d => d.Source != null).Select(d => d.Source);
                locals = DateFilter.Apply(locals, l => l.Date);
                var vmLocals = ProjectToViewModelCollection(locals);
                Transactions.Reset(vmLocals);

                var remotes = deltas.Where(d => d.Target != null).Select(d => d.Target);
                var vmRemotes = ProjectToViewModelCollection(remotes);
                vmRemotes = DateFilter.Apply(vmRemotes, l => l.Date);
                RemoteTransactions.Reset(vmRemotes);
            }
            else
            {
                var locals = _currentAccountBasicTransactions;
                var filteredLocals = DateFilter.Apply(locals, l => l.Date);
                Transactions.Reset(filteredLocals);
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

        private async Task Synchronize()
        {
            var delta = TransactionDelta;
            if (!delta.LastDeltaDate.HasValue || delta.LastDeltaDate.Value < DateTime.Now.AddMinutes(-10))
            {
                delta = await ComputeDeltaWithRemote();
            }

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