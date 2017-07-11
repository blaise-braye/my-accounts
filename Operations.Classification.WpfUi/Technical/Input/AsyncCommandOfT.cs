using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Helpers;
using log4net;

namespace Operations.Classification.WpfUi.Technical.Input
{
    public interface IAsyncCommand<in TArgType> : IAsyncCommand
    {
        Task ExecuteAsync(TArgType @object);
    }

    public class AsyncCommand<TArgType> : RelayCommand<TArgType>, IAsyncCommand<TArgType>
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AsyncCommand<TArgType>));

        private static readonly Action<TArgType> _emptyAction = arg => { };

        private readonly WeakFunc<TArgType, Task> _taskBuilder;

        public AsyncCommand(Func<TArgType, Task> taskBuilder)
            : this(taskBuilder, null)
        {
        }

        public AsyncCommand(Func<TArgType, Task> taskBuilder, Func<TArgType, bool> canExecute)
            : base(_emptyAction, canExecute)
        {
            if (taskBuilder == null)
            {
                throw new ArgumentNullException(nameof(taskBuilder));
            }

            _taskBuilder = new WeakFunc<TArgType, Task>(taskBuilder);
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

            if (input == null && typeof(TArgType).IsValueType)
            {
                return ExecuteAsync(default(TArgType));
            }

            return ExecuteAsync((TArgType)input);
        }

        public virtual Task ExecuteAsync(TArgType input)
        {
            return _taskBuilder.Execute(input);
        }
    }
}