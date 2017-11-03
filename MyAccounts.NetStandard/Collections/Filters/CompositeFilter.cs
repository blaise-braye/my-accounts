using System.Linq;

namespace MyAccounts.NetStandard.Collections.Filters
{
    public class CompositeFilter : CompositeFilterBase, IFilter
    {
        private readonly IFilter[] _filters;

        public CompositeFilter(params IFilter[] filters)
        {
            _filters = filters;
            foreach (var filter in filters)
            {
                filter.FilterInvalidated += OnNestedFilterChanged;
            }
        }
        
        public override bool IsActive()
        {
            return _filters.Any(f => f.IsActive());
        }

        public override void Reset()
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
    }
}