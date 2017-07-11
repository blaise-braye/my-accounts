using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Helpers;
using log4net;

namespace Operations.Classification.WpfUi.Technical.Input
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object input);

        void RaiseCanExecuteChanged();
    }

    public class AsyncCommand : RelayCommand, IAsyncCommand
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AsyncCommand));

        private static readonly Action _emptyAction = () => { };

        private readonly WeakFunc<Task> _taskBuilder;

        public AsyncCommand(Func<Task> taskBuilder)
            : this(taskBuilder, null)
        {
        }

        public AsyncCommand(Func<Task> taskBuilder, Func<bool> canExecute)
            : base(_emptyAction, canExecute)
        {
            if (taskBuilder == null)
            {
                throw new ArgumentNullException(nameof(taskBuilder));
            }

            _taskBuilder = new WeakFunc<Task>(taskBuilder);
        }

        public override async void Execute(object input)
        {
            try
            {
                await ExecuteAsync(input);
            }
            catch (Exception exn)
            {
                _log.Error("Command execution failed", exn);
            }
        }

        public virtual Task ExecuteAsync(object input)
        {
            if (!CanExecute(input) || _taskBuilder == null || (!_taskBuilder.IsStatic && !_taskBuilder.IsAlive))
            {
                return Task.CompletedTask;
            }

            return _taskBuilder.Execute();
        }
    }
}