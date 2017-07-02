using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using Operations.Classification.GererMesComptes;
using Operations.Classification.WpfUi.Technical.Controls;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    public class TransactionDeltaFilter : List<MenuItemViewModel>, IFilter
    {
        public TransactionDeltaFilter()
        {
            var cmd = new RelayCommand<IList<DeltaAction>>(RefreshFilterState);

            var mappings = new Dictionary<string, IList<DeltaAction>>
            {
                { "all", new DeltaAction[0] },
                { "to add", new[] { DeltaAction.Add } },
                { "to update", new[] { DeltaAction.UpdateMemo } },
                { "to remove", new[] { DeltaAction.Remove } },
                { "undeterministic status", new[] { DeltaAction.MultipleTargetsPossible, DeltaAction.NotUniqueKeyInTarget } },
                { "up to date", new[] { DeltaAction.Nothing } }
            };

            foreach (var mapping in mappings)
            {
                var item = new MenuItemViewModel
                {
                    StaysOpenOnClick = true,
                    Command = cmd,
                    CommandParameter = mapping.Value,
                    Header = mapping.Key,
                    IsCheckable = true,
                    IsChecked = true
                };

                Add(item);
            }
        }

        public event EventHandler FilterInvalidated;

        public bool IsActive()
        {
            return !GetAllItem().IsChecked;
        }

        public void Reset()
        {
            GetAllItem().IsChecked = true;
            RefreshFilterState((IList<DeltaAction>)GetAllItem().CommandParameter);
        }

        public IEnumerable<T> Apply<T>(IEnumerable<T> deltas, Func<T, DeltaAction> selector)
        {
            var filtered = deltas;

            if (IsActive())
            {
                var filterScope = BuildFilterScope();
                filtered = filtered.Where(d => filterScope.Contains(selector(d))).ToList();
            }

            return filtered;
        }

        public List<DeltaAction> BuildFilterScope()
        {
            var actions = this.Where(i => i.IsChecked).Select(i => i.CommandParameter).Cast<IList<DeltaAction>>()
                .SelectMany(i => i).ToList();
            return actions;
        }

        private MenuItemViewModel GetAllItem()
        {
            return this[0];
        }

        private void RefreshFilterState(IList<DeltaAction> changedFilter)
        {
            var allItem = this[0];

            if (Equals(allItem.CommandParameter, changedFilter))
            {
                // check all
                foreach (var item2 in this)
                    item2.IsChecked = allItem.IsChecked;
            }
            else
            {
                var allValues = this.Select(i => i.CommandParameter).Cast<IList<DeltaAction>>().SelectMany(d => d).ToList();
                var filteredValues = this.Where(i => i.IsChecked).Select(i => i.CommandParameter).Cast<IList<DeltaAction>>().SelectMany(d => d).ToList();
                if (allValues.Count == filteredValues.Count)
                {
                    foreach (var item2 in this)
                        item2.IsChecked = true;
                }
                else
                {
                    allItem.IsChecked = false;
                }
            }

            FilterInvalidated?.Invoke(this, EventArgs.Empty);
        }
    }
}