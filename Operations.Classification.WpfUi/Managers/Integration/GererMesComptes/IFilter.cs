using System;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    public interface IFilter
    {
        event EventHandler FilterInvalidated;
        bool IsActive();
        void Reset();
    }
}