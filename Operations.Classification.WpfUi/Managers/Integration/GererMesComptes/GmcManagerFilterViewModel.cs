using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyAccounts.Business.GererMesComptes;
using Operations.Classification.WpfUi.Technical.Collections.Filters;
using QifApi.Transactions;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    public class GmcManagerFilterViewModel : ObservableObject, IFilter
    {
        private readonly CompositeFilter _anyFilter;
        private bool _isFiltering;
        private bool _isDeltaFilterActive;

        public GmcManagerFilterViewModel()
        {
            FilterOnItemDateCommand = new RelayCommand<BasicTransactionModel>(FilterOnItemDate);

            DeltaFilter = new TransactionDeltaFilter();
            DateFilter = new DateRangeFilter();
            MemoFilter = new TextFilter();
            _anyFilter = new CompositeFilter(DeltaFilter, DateFilter, MemoFilter);
            _anyFilter.FilterInvalidated += OnAnyFilterInvalidated;

            ResetFilterCommad = new RelayCommand(Reset);
        }

        public event EventHandler FilterInvalidated;

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

        public bool IsDeltaFilterActive
        {
            get => _isDeltaFilterActive;
            set => Set(nameof(IsDeltaFilterActive), ref _isDeltaFilterActive, value);
        }

        public TransactionDeltaFilter DeltaFilter { get; }

        public DateRangeFilter DateFilter { get; }

        public TextFilter MemoFilter { get; }

        public RelayCommand<BasicTransactionModel> FilterOnItemDateCommand { get; }

        public RelayCommand ResetFilterCommad { get; }

        public bool IsActive()
        {
            return IsFiltering;
        }

        public void Reset()
        {
            _anyFilter.Reset();
        }

        public IEnumerable<BasicTransaction> FilterDelta(IEnumerable<TransactionDelta> deltas, Func<TransactionDelta, BasicTransaction> selector)
        {
            var filteredDelta = DeltaFilter.Apply(deltas, d => d.Action);
            var locals = filteredDelta.Where(d => d.Source != null).Select(selector);
            locals = DateFilter.Apply(locals, l => l.Date);
            locals = MemoFilter.Apply(locals, l => l.Memo);
            return locals;
        }

        public IEnumerable<BasicTransactionModel> FilterBasicTransactions(IEnumerable<BasicTransactionModel> basicTransactions)
        {
            var filterBasicTransactions = DateFilter.Apply(basicTransactions, l => l.Date);
            filterBasicTransactions = MemoFilter.Apply(filterBasicTransactions, l => l.Memo);
            return filterBasicTransactions;
        }

        private void RefreshIsFilteringState()
        {
            IsFiltering = _anyFilter.IsActive();
        }

        private void OnAnyFilterInvalidated(object sender, EventArgs e)
        {
            RefreshIsFilteringState();
            FilterInvalidated?.Invoke(this, e);
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
    }
}