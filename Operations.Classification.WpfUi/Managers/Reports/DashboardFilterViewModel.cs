using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyAccounts.Business.GererMesComptes;
using MyAccounts.NetStandard.Collections.Filters;

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
            DirectionFilter = new DirectionFilter();
            _anyFilter = new CompositeFilter(DateRangeFilter, NoteFilter, AccountsFilter, DirectionFilter);
            _anyFilter.FilterInvalidated += OnAnyFilterInvalidated;
            ResetFilterCommad = new RelayCommand(Reset, () => IsFiltering);
        }

        public event EventHandler FilterInvalidated;
        
        public RelayCommand ResetFilterCommad { get; }

        public TextFilter NoteFilter { get; }

        public DateRangeFilter DateRangeFilter { get;  }

        public MultiSelectFilter AccountsFilter { get; }

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