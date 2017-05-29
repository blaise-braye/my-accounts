using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GalaSoft.MvvmLight;
using Operations.Classification.WpfUi.Data;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Technical.Input;

namespace Operations.Classification.WpfUi.Managers.Accounts
{
    public class AccountsManager : ViewModelBase
    {
        private readonly IAsyncCommand[] _commands;

        private readonly AccountsRepository _repository;
        private readonly TransactionsRepository _transactionsRepository;

        private ObservableCollection<AccountViewModel> _accounts;

        private AccountViewModel _currentAccount;

        private bool _hasCurrentAccount;

        private bool _isEditing;

        private bool _isLoading;

        private readonly BusyIndicatorViewModel _busyIndicator;

        public AccountsManager(
            BusyIndicatorViewModel busyIndicatorViewModel,
            AccountsRepository repository, 
            TransactionsRepository transactionsRepository)
        {
            _busyIndicator = busyIndicatorViewModel;
            _repository = repository;
            _transactionsRepository = transactionsRepository;

            LoadCommand = new AsyncCommand(LoadAsync, () => !IsEditing);
            BeginNewCommand = new AsyncCommand(BeginNew, () => !IsEditing);
            BeginEditCommand = new AsyncCommand(BeginEdit, () => !IsEditing && CurrentAccount != null);
            DeleteCommand = new AsyncCommand(Delete, () => !IsEditing && CurrentAccount != null);
            CommitEditCommand = new AsyncCommand(CommitEdit, () => IsEditing);

            _commands = new[] { LoadCommand, BeginEditCommand, CommitEditCommand, BeginNewCommand, DeleteCommand };
        }

        public ObservableCollection<AccountViewModel> Accounts
        {
            get { return _accounts ?? (_accounts = new ObservableCollection<AccountViewModel>()); }
            private set { Set(nameof(Accounts), ref _accounts, value); }
        }

        public AccountViewModel CurrentAccount
        {
            get { return _currentAccount; }
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
            get { return _isEditing; }
            private set
            {
                if (Set(nameof(IsEditing), ref _isEditing, value))
                    InvalidateCommands();
            }
        }

        public IAsyncCommand BeginEditCommand { get; }

        public IAsyncCommand BeginNewCommand { get; }

        public IAsyncCommand LoadCommand { get; }

        public IAsyncCommand CommitEditCommand { get; }

        public IAsyncCommand DeleteCommand { get; }

        public bool HasCurrentAccount
        {
            get { return _hasCurrentAccount; }
            private set { Set(nameof(HasCurrentAccount), ref _hasCurrentAccount, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(nameof(IsLoading), ref _isLoading, value); }
        }

        private async Task LoadAsync()
        {
            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Loading accounts"))
            {
                var currentAccountId = CurrentAccount?.Id;
                var result = new List<AccountViewModel>();

                foreach (var entity in await _repository.GetList())
                {
                    var vm = Mapper.Map<AccountEntity, AccountViewModel>(entity);
                    var operations = await _transactionsRepository.GetTransformedUnifiedOperations(entity.Name);
                    vm.Operations = operations;

                    result.Add(vm);
                }

                Accounts = new ObservableCollection<AccountViewModel>(result);
                CurrentAccount = Accounts.FirstOrDefault(a => a.Id == currentAccountId);
            }
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

        public async Task<bool> BeginEdit()
        {
            if (CurrentAccount != null && await CommitEdit())
                IsEditing = true;

            return IsEditing;
        }

        public async Task<bool> CommitEdit()
        {
            bool success;

            if (!IsEditing)
                success = true;
            else
                using (_busyIndicator.EncapsulateActiveJobDescription(this, "Committing pending changes"))
                {
                    var account = CurrentAccount;

                    var entity = Mapper.Map<AccountViewModel, AccountEntity>(account);
                    if (account.IsNew)
                        entity.Id = Guid.NewGuid();

                    success = await _repository.AddOrUpdate(entity);
                    if (success)
                        account.Id = entity.Id;

                    IsEditing = !success;
                }

            return success;
        }

        private async Task<bool> Delete()
        {
            var account = CurrentAccount;
            if (account == null)
                return true;

            if (IsEditing)
                return false;

            using (_busyIndicator.EncapsulateActiveJobDescription(this, $"Deleting current account ({account.Name})"))
            {
                if (await _repository.Delete(account.Id))
                {
                    var accountIdx = Accounts.IndexOf(account);
                    if (accountIdx >= 0)
                    {
                        Accounts.RemoveAt(accountIdx);

                        if (Accounts.Count <= accountIdx)
                            accountIdx = Accounts.Count - 1;

                        if (accountIdx >= 0)
                            CurrentAccount = Accounts.ElementAt(accountIdx);
                    }

                    return true;
                }
            }

            return false;
        }

        private void InvalidateCommands()
        {
            foreach (var asyncCommand in _commands)
                asyncCommand.RaiseCanExecuteChanged();
        }
    }
}