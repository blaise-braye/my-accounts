using System;

namespace MyAccounts.NetStandard.Collections.Filters
{
    public interface IFilter
    {
        event EventHandler FilterInvalidated;

        bool IsActive();

        void Reset();
    }
}