using System;
using GalaSoft.MvvmLight;

namespace MyAccounts.NetStandard.Collections.Filters
{
    public abstract class CompositeFilterBase : ObservableObject, IFilter
    {
        private bool _applying;

        public event EventHandler FilterInvalidated;

        public abstract bool IsActive();

        public abstract void Reset();

        public void Apply(Action action)
        {
            var isapplying = _applying;
            try
            {
                if (!isapplying)
                {
                    _applying = true;
                }

                action();
            }
            finally
            {
                if (!isapplying)
                {
                    _applying = false;
                }
            }

            if (!isapplying)
            {
                FilterInvalidated?.Invoke(this, EventArgs.Empty);
            }
        }
        
        protected virtual void OnFilterChanged()
        {
            if (!_applying)
            {
                FilterInvalidated?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void OnNestedFilterChanged(object sender, EventArgs e)
        {
            if (!_applying)
            {
                FilterInvalidated?.Invoke(sender, e);
            }
        }
    }
}