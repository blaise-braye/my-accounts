using System;

namespace Operations.Classification.WpfUi.Technical.Collections.Filters
{
    public interface IFilter
    {
        event EventHandler FilterInvalidated;
        bool IsActive();
        void Reset();
    }
}