using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Operations.Classification.WpfUi.Data;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Managers.Reports;
using Operations.Classification.WpfUi.Managers.Transactions;
using Operations.Classification.WpfUi.Technical.Input;
using Operations.Classification.WpfUi.Technical.Projections;

namespace Operations.Classification.WpfUi.Managers.Accounts
{
    public class AccountsManager : ViewModelBase
    {
        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly IAsyncCommand[] _commands;

        private readonly AccountsRepository _repository;
        private readonly ITransactionsManager _transactionsManager;

        private ObservableCollection<AccountViewModel> _accounts;

        private AccountViewModel _currentAccount;

        private bool _hasCurrentAccount;

        private bool _isEditing;

        private bool _isLoading;

        public AccountsManager(BusyIndicatorViewModel busyIndicatorViewModel, AccountsRepository repository, ITransactionsManager transactionsManager)
        {
            _busyIndicator = busyIndicatorViewModel;
            _repository = repository;
            _transactionsManager = transactionsManager;
            LoadCommand = new AsyncCommand(LoadAsync, () => !IsEditing);
            BeginNewCommand = new AsyncCommand(BeginNew, () => !IsEditing);
            BeginEditCommand = new AsyncCommand(BeginEdit, () => !IsEditing && CurrentAccount != null);
            DeleteCommand = new AsyncCommand(Delete, () => !IsEditing && CurrentAccount != null);
            CommitEditCommand = new AsyncCommand(CommitEdit, () => IsEditing);
            UpdateAccountSelectionCommand = new AsyncCommand<IEnumerable>(UpdateAccountSelection);
            UnifiedOperationsReporter = new UnifiedOperationsReporter();
            _commands = new[] { LoadCommand, BeginEditCommand, CommitEditCommand, BeginNewCommand, DeleteCommand };
        }

        public UnifiedOperationsReporter UnifiedOperationsReporter { get; }

        public ObservableCollection<AccountViewModel> Accounts
        {
            get => _accounts ?? (_accounts = new ObservableCollection<AccountViewModel>());
            private set => Set(nameof(Accounts), ref _accounts, value);
        }

        public AccountViewModel CurrentAccount
        {
            get => _currentAccount;
            set
            {
                if (IsEditing)
                {
                    RaisePropertyChanged();
                }
                else if (Set(nameof(CurrentAccount), ref _currentAccount, value))
                {
                    HasCurrentAccount = CurrentAccount != null;
                    InvalidateCommands();
                    MessengerInstance.Send(value);
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            private set
            {
                if (Set(nameof(IsEditing), ref _isEditing, value))
                {
                    InvalidateCommands();
                }
            }
        }

        public IAsyncCommand BeginEditCommand { get; }

        public IAsyncCommand BeginNewCommand { get; }

        public IAsyncCommand LoadCommand { get; }

        public IAsyncCommand CommitEditCommand { get; }

        public IAsyncCommand DeleteCommand { get; }

        public bool HasCurrentAccount
        {
            get => _hasCurrentAccount;
            private set => Set(nameof(HasCurrentAccount), ref _hasCurrentAccount, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(nameof(IsLoading), ref _isLoading, value);
        }

        public AsyncCommand<IEnumerable> UpdateAccountSelectionCommand { get; }

        public async Task<bool> BeginEdit()
        {
            if (CurrentAccount != null && await CommitEdit())
            {
                IsEditing = true;
            }

            return IsEditing;
        }

        public async Task BeginNew()
        {
            if (await CommitEdit())
            {
                var accountViewModel = new AccountViewModel();
                Accounts.Add(accountViewModel);
                CurrentAccount = accountViewModel;
                IsEditing = true;
            }
        }

        public async Task<bool> CommitEdit()
        {
            bool success;

            if (!IsEditing)
            {
                success = true;
            }
            else
            {
                using (_busyIndicator.EncapsulateActiveJobDescription(this, "Committing pending changes"))
                {
                    var account = CurrentAccount;

                    var entity = account.Map().To<AccountEntity>();

                    if (account.IsNew)
                    {
                        entity.Id = Guid.NewGuid();
                    }

                    success = await _repository.AddOrUpdate(entity);
                    if (success)
                    {
                        account.Id = entity.Id;
                    }

                    IsEditing = !success;
                }
            }

            return success;
        }

        public async Task LoadAsync()
        {
            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Loading accounts"))
            {
                var currentAccountId = CurrentAccount?.Id;
                var result = new List<AccountViewModel>();

                foreach (var entity in await _repository.GetList())
                {
                    var vm = entity.Map().To<AccountViewModel>();
                    var operations = await _transactionsManager.GetTransformedUnifiedOperations(entity.Name);
                    vm.Operations = operations;

                    result.Add(vm);
                }

                Accounts = new ObservableCollection<AccountViewModel>(result);
                CurrentAccount = Accounts.FirstOrDefault(a => a.Id == currentAccountId);
                await UpdateAccountSelection(CurrentAccount == null ? new AccountViewModel[0] : new[] { CurrentAccount });
            }
        }

        private async Task UpdateAccountSelection(IEnumerable obj)
        {
            var selection = obj.Cast<AccountViewModel>().ToList();
            if (selection.Count == 0)
            {
                selection.AddRange(Accounts);
            }

            await UnifiedOperationsReporter.UpdateAccountSelection(selection);
        }

        private async Task<bool> Delete()
        {
            var account = CurrentAccount;
            if (account == null)
            {
                return true;
            }

            if (IsEditing)
            {
                return false;
            }

            using (_busyIndicator.EncapsulateActiveJobDescription(this, $"Deleting current account ({account.Name})"))
            {
                if (await _repository.Delete(account.Id))
                {
                    var accountIdx = Accounts.IndexOf(account);
                    if (accountIdx >= 0)
                    {
                        Accounts.RemoveAt(accountIdx);

                        if (Accounts.Count <= accountIdx)
                        {
                            accountIdx = Accounts.Count - 1;
                        }

                        if (accountIdx >= 0)
                        {
                            CurrentAccount = Accounts.ElementAt(accountIdx);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private void InvalidateCommands()
        {
            foreach (var asyncCommand in _commands)
            {
                asyncCommand.RaiseCanExecuteChanged();
            }
        }
    }
}