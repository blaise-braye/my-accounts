using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;

namespace MyAccounts.NetStandard.Input
{
    public class BusyIndicatorViewModel : ObservableObject
    {
        private readonly List<ActiveJobDescription> _encapsulators = new List<ActiveJobDescription>();

        private bool _isBusy;

        private string _reason;

        public bool IsBusy
        {
            get => _isBusy;
            private set => Set(nameof(IsBusy), ref _isBusy, value);
        }

        public string Reason
        {
            get => _reason;
            private set => Set(nameof(Reason), ref _reason, value);
        }

        public IDisposable EncapsulateActiveJobDescription(object source, string reason)
        {
            var jobEncapsulator = new ActiveJobDescription(source, reason, DecapsulateActiveJobDescription);
            _encapsulators.Insert(0, jobEncapsulator);
            RefreshStatus();
            return jobEncapsulator;
        }

        public bool IsBusySource(object source)
        {
            return _encapsulators.Any(j => j.Source == source);
        }

        private void DecapsulateActiveJobDescription(ActiveJobDescription activeJobDescription)
        {
            _encapsulators.Remove(activeJobDescription);
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            var activeJobDescription = _encapsulators.FirstOrDefault();
            IsBusy = activeJobDescription != null;
            Reason = activeJobDescription?.Reason ?? string.Empty;
        }

        private class ActiveJobDescription : IDisposable
        {
            private readonly Action<ActiveJobDescription> _disposeActionCallback;

            public ActiveJobDescription(object source, string reason, Action<ActiveJobDescription> disposeActionCallback)
            {
                _disposeActionCallback = disposeActionCallback;
                Source = source;
                Reason = reason;
            }

            public object Source { get; }

            public string Reason { get; }

            public void Dispose()
            {
                _disposeActionCallback(this);
                GC.SuppressFinalize(this);
            }
        }
    }
}