using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;

namespace Operations.Classification.WpfUi.Technical.Input
{
    public class BusyIndicatorViewModel : ObservableObject
    {
        private readonly List<ActiveJobDescription> _encapsulators = new List<ActiveJobDescription>();

        private bool _isBusy;

        private string _reason;

        public bool IsBusy
        {
            get { return _isBusy; }
            private set { Set(nameof(IsBusy), ref _isBusy, value); }
        }

        public string Reason
        {
            get { return _reason; }
            private set { Set(nameof(Reason), ref _reason, value); }
        }

        public IDisposable EncapsulateActiveJobDescription(object source, string reason)
        {
            var jobEncapsulator = new ActiveJobDescription(source, reason, DecapsulateActiveJobDescription);
            _encapsulators.Insert(0, jobEncapsulator);
            RefreshStatus();
            return jobEncapsulator;
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

        public bool IsBusySource(object source)
        {
            return _encapsulators.Any(j => j.Source == source);
        }

        private class ActiveJobDescription : IDisposable
        {
            private readonly Action<ActiveJobDescription> _onDisposeAction;

            public ActiveJobDescription(object source, string reason, Action<ActiveJobDescription> onDisposeAction)
            {
                _onDisposeAction = onDisposeAction;
                Source = source;
                Reason = reason;
            }

            public object Source { get; }

            public string Reason { get; }

            public void Dispose()
            {
                _onDisposeAction(this);
                GC.SuppressFinalize(this);
            }
        }
    }
}