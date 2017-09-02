using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Helpers;
using GalaSoft.MvvmLight.Messaging;
using log4net;

namespace Operations.Classification.WpfUi.Technical.Messages
{
    public class AsyncMessageReceiver
    {
        private readonly IMessenger _messenger;

        private readonly List<IDisposable> _asyncReceivers = new List<IDisposable>();

        public AsyncMessageReceiver(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public void Cleanup()
        {
            foreach (var asyncReceiver in _asyncReceivers)
            {
                asyncReceiver.Dispose();
            }

            _asyncReceivers.Clear();
        }

        public void RegisterAsync<TMessage>(
            object recipient,
            Func<TMessage, Task> taskBuilder)
        {
            var weakTaskBuilder = new WeakFunc<TMessage, Task>(recipient, taskBuilder);
            var receiver = new AsyncMessageReceiverOfT<TMessage>(_messenger, weakTaskBuilder);
            _asyncReceivers.Add(receiver);
        }

        private class AsyncMessageReceiverOfT<TMessage> : IDisposable
        {
            private static readonly ILog _log = LogManager.GetLogger(typeof(AsyncMessageReceiverOfT<TMessage>));
            private IMessenger _messenger;
            private WeakFunc<TMessage, Task> _taskBuilder;

            public AsyncMessageReceiverOfT(
                IMessenger messenger,
                WeakFunc<TMessage, Task> taskBuilder)
            {
                _messenger = messenger;
                _taskBuilder = taskBuilder;
                messenger.Register<TMessage>(this, ReceiveAsyncMessage);
            }

            public void Dispose()
            {
                if (_taskBuilder == null)
                {
                    return;
                }

                _messenger.Unregister(this);
                _taskBuilder.MarkForDeletion();
                _taskBuilder = null;
                _messenger = null;
            }

            private async void ReceiveAsyncMessage(TMessage m)
            {
                try
                {
                    await _taskBuilder.Execute(m);
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }
    }
}