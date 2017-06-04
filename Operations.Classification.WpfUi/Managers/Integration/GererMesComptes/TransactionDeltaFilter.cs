using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.CommandWpf;
using Operations.Classification.GererMesComptes;
using Operations.Classification.WpfUi.Technical.Controls;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    public class TransactionDeltaFilter : List<MenuItemViewModel>
    {
        public event EventHandler FilterItemChanged;

        public bool IsActive()
        {
            return !GetAllItem().IsChecked;
        }

        public void Reset()
        {
            GetAllItem().IsChecked = true;
            OnFilterItemChanged((IList<DeltaAction>)GetAllItem().CommandParameter);
        }

        public TransactionDeltaFilter()
        {
            var cmd = new RelayCommand<IList<DeltaAction>>(OnFilterItemChanged);

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


        private void OnFilterItemChanged(IList<DeltaAction> filter)
        {
            var allItem = this[0];

            if (Equals(allItem.CommandParameter, filter))
            {
                // check all
                foreach (var item2 in this)
                {
                    item2.IsChecked = allItem.IsChecked;
                }
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

            FilterItemChanged?.Invoke(this, EventArgs.Empty);
        }
        
        private MenuItemViewModel GetAllItem()
        {
            return this[0];
        }

        public List<DeltaAction> BuildFilterScope()
        {
            var actions = this.Where(i => i.IsChecked).Select(i => i.CommandParameter).Cast<IList<DeltaAction>>()
                .SelectMany(i => i).ToList();
            return actions;
        }
    }
}