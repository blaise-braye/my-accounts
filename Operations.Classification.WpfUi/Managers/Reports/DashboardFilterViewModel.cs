using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyAccounts.NetStandard.Collections.Filters;
using Operations.Classification.WpfUi.Managers.Accounts.Models;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class DashboardFilterViewModel : ObservableObject, IFilter
    {
        private readonly CompositeFilter _anyFilter;
        private bool _isFiltering;

        public DashboardFilterViewModel()
        {
            DateRangeFilter = new DateRangeFilter();
            NoteFilter = new TextFilter();
            AccountsFilter = new MultiSelectFilter();
            CategoryFilter = new MultiSelectFilter();
            DirectionFilter = new DirectionFilter();
            _anyFilter = new CompositeFilter(DateRangeFilter, NoteFilter, AccountsFilter, CategoryFilter, DirectionFilter);
            _anyFilter.FilterInvalidated += OnAnyFilterInvalidated;
            
            ResetFilterCommad = new RelayCommand(Reset, () => IsFiltering);
        }

        public event EventHandler FilterInvalidated;

        public RelayCommand ResetFilterCommad { get; }

        public TextFilter NoteFilter { get; }

        public DateRangeFilter DateRangeFilter { get;  }

        public MultiSelectFilter AccountsFilter { get; }

        public MultiSelectFilter CategoryFilter { get; set; }

        public DirectionFilter DirectionFilter { get; }

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

        public bool IsActive()
        {
            return IsFiltering;
        }
        
        public void Reset()
        {
            _anyFilter.Reset();
        }

        public void Initialize(IList<AccountViewModel> accounts)
        {
            _anyFilter.FilterInvalidated -= OnAnyFilterInvalidated;
            AccountsFilter.Initialize(accounts, account => account.Name);
            var categories = accounts.SelectMany(a => a.Operations).Select(a => a.GetCategoryByLevel(0)).Distinct();
            CategoryFilter.Initialize(categories, s => s);
            _anyFilter.FilterInvalidated += OnAnyFilterInvalidated;
            OnAnyFilterInvalidated(this, EventArgs.Empty);
        }

        private void RefreshIsFilteringState()
        {
            IsFiltering = _anyFilter.IsActive();
        }

        private void OnAnyFilterInvalidated(object sender, EventArgs e)
        {
            RefreshIsFilteringState();
            ResetFilterCommad.RaiseCanExecuteChanged();
            FilterInvalidated?.Invoke(sender, e);
        }
    }
}