using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.GererMesComptes;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Technical.Caching;
using Operations.Classification.WpfUi.Technical.Collections;
using Operations.Classification.WpfUi.Technical.Collections.Filters;
using Operations.Classification.WpfUi.Technical.Input;
using Operations.Classification.WpfUi.Technical.Projections;
using QifApi.Transactions;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    public class GmcManager : ViewModelBase
    {
        private readonly CompositeFilter _anyFilter;
        private readonly BusyIndicatorViewModel _busyIndicator;

        private Dictionary<Guid, TransactionDeltaSet> _loadedDeltas;
        private AccountViewModel _currentAccount;
        private bool _isDeltaAvailable;
        private bool _isFiltering;
        private List<BasicTransactionModel> _currentAccountBasicTransactions;
        private TransactionDeltaSet _transactionDelta;

        public GmcManager(BusyIndicatorViewModel busyIndicator)
        {
            _busyIndicator = busyIndicator;

            Transactions = new SmartCollection<BasicTransactionModel>();
            RemoteTransactions = new SmartCollection<BasicTransactionModel>();

            RefreshRemoteKnowledgeCommand = new AsyncCommand(ComputeDeltaWithRemote);
            SynchronizeCommand = new AsyncCommand(Synchronize);
            ClearCurrentAccountCacheAndResetCommand = new AsyncCommand(ClearCurrentAccountCacheAndReset);

            DeltaFilter = new TransactionDeltaFilter();
            DateFilter = new DateRangeFilter();
            MemoFilter = new TextFilter();
            _anyFilter = new CompositeFilter(DeltaFilter, DateFilter, MemoFilter);
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
            private set => Set(nameof(IsDeltaAvailable), ref _isDeltaAvailable, value);
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

        public TransactionDeltaFilter DeltaFilter { get; }

        public DateRangeFilter DateFilter { get; }

        public TextFilter MemoFilter { get; }

        public RelayCommand<BasicTransactionModel> FilterOnItemDateCommand { get; }

        public RelayCommand ResetFilterCommad { get; }

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

        private void FilterOnItemDate(BasicTransactionModel obj)
        {
            _anyFilter.Apply(
                () =>
                {
                    _anyFilter.Reset();
                    DateFilter.SetDayFilter(obj.Date);
                });
        }

        private ICacheEntry<List<TransactionDelta>> GetDeltaCacheEntry(AccountViewModel account)
        {
            return CacheProvider.GetJSonCacheEntry<List<TransactionDelta>>($"TransactionDeltas/{account.Name}");
        }

        private void OnAccountViewModelReceived(AccountViewModel currentAccount)
        {
            if (Set(nameof(CurrentAccount), ref _currentAccount, currentAccount))
            {
                TransactionDeltaSet delta = null;

                if (currentAccount!=null && _loadedDeltas.ContainsKey(currentAccount.Id))
                {
                    delta = _loadedDeltas[currentAccount.Id];
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
                locals = MemoFilter.Apply(locals, l => l.Memo);
                var vmLocals = ProjectToViewModelCollection(locals);
                Transactions.Reset(vmLocals);

                var remotes = deltas.Where(d => d.Target != null).Select(d => d.Target);
                var vmRemotes = ProjectToViewModelCollection(remotes);
                vmRemotes = DateFilter.Apply(vmRemotes, l => l.Date);
                vmRemotes = MemoFilter.Apply(vmRemotes, l => l.Memo);
                RemoteTransactions.Reset(vmRemotes);
            }
            else
            {
                var locals = _currentAccountBasicTransactions;
                var filteredLocals = DateFilter.Apply(locals, l => l.Date);
                filteredLocals = MemoFilter.Apply(filteredLocals, l => l.Memo);
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
                if (await client.Connect(Properties.Settings.Default.GmcUserName, Properties.Settings.Default.GmcPassword))
                {
                    var accountsRepository = new AccountInfoRepository(client);
                    var account = await accountsRepository.GetByName(CurrentAccount.GmcAccountName);
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