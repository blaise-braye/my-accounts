using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using MyAccounts.NetStandard.Collections.Filters;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class DirectionFilter : ObservableObject, IFilter
    {
        private bool _income;
        
        private bool _outgoing;

        public DirectionFilter()
        {
            Reset();
        }

        public event EventHandler FilterInvalidated;

        public bool Income
        {
            get => _income;
            set
            {
                if (Set(() => Income, ref _income, value))
                {
                    OnFilterChanged();
                }
            }
        }

        public bool Outgoing
        {
            get => _outgoing;
            set
            {
                if (Set(() => Outgoing, ref _outgoing, value))
                {
                    OnFilterChanged();
                }
            }
        }

        public bool IsActive()
        {
            return !(Income && Outgoing);
        }

        public void Reset()
        {
            Income = Outgoing = true;
        }
        
        public IEnumerable<T> Apply<T>(IEnumerable<T> locals, Func<T, decimal> amountSelector)
        {
            var filtered = locals;

            if (IsActive())
            {
                if (!Income && !Outgoing)
                {
                    filtered = filtered.Where(d => false);
                }
                else if (!Income)
                {
                    filtered = filtered.Where(d => amountSelector(d) > 0);
                }
                else
                {
                    filtered = filtered.Where(d => amountSelector(d) < 0);
                }
            }

            return filtered;
        }

        public IEnumerable<T> Apply<T>(IEnumerable<T> locals, Func<T, decimal> incomeSelector, Func<T, decimal> outgoingSelector)
        {
            var filtered = locals;

            if (IsActive())
            {
                if (!Income && !Outgoing)
                {
                    filtered = filtered.Where(d => false);
                }
                else if (!Income)
                {
                    filtered = filtered.Where(d => incomeSelector(d) == 0);
                }
                else
                {
                    filtered = filtered.Where(d => outgoingSelector(d) == 0);
                }
            }

            return filtered;
        }

        protected virtual void OnFilterChanged()
        {
            FilterInvalidated?.Invoke(this, EventArgs.Empty);
        }
    }
}