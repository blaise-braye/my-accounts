using System;
using System.Linq;

namespace Operations.Classification.WpfUi.Technical.Collections.Filters
{
    public class CompositeFilter : IFilter
    {
        private readonly IFilter[] _filters;
        private bool _applying;

        public CompositeFilter(params IFilter[] filters)
        {
            _filters = filters;
            foreach (var filter in filters)
            {
                filter.FilterInvalidated += OnNestedFilterChanged;
            }
        }

        public event EventHandler FilterInvalidated;

        public bool IsActive()
        {
            return _filters.Any(f => f.IsActive());
        }

        public void Reset()
        {
            Apply(
                () =>
                {
                    foreach (var filter in _filters)
                    {
                        filter.Reset();
                    }
                });
        }

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

        private void OnNestedFilterChanged(object sender, EventArgs e)
        {
            if (!_applying)
            {
                FilterInvalidated?.Invoke(sender, e);
            }
        }
    }
}