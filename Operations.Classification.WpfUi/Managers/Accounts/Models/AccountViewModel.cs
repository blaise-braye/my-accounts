using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.WpfUi.Technical.Collections;

namespace Operations.Classification.WpfUi.Managers.Accounts.Models
{
    public class AccountViewModel : ViewModelBase
    {
        private string _gmcAccountName;

        private Guid _id;

        private decimal _initialBalance;

        private string _name;

        private List<UnifiedAccountOperation> _operations;
        
        public AccountViewModel()
        {
            Status = new AccountStatusViewModel();
        }
        
        public bool IsNew => Id == default(Guid);

        public Guid Id
        {
            get => _id;
            set => Set(nameof(Id), ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => Set(nameof(Name), ref _name, value);
        }

        public string GmcAccountName
        {
            get => _gmcAccountName;
            set => Set(nameof(GmcAccountName), ref _gmcAccountName, value);
        }

        public decimal InitialBalance
        {
            get => _initialBalance;
            set
            {
                if (Set(nameof(InitialBalance), ref _initialBalance, value))
                {
                    RefreshStatus();
                }
            }
        }

        public List<UnifiedAccountOperation> Operations
        {
            get => _operations;
            set
            {
                if (Set(nameof(Operations), ref _operations, value))
                {
                    RefreshStatus();
                }
            }
        }

        public AccountStatusViewModel Status { get; }

        private void RefreshStatus()
        {
            Status.Balance = InitialBalance;
            Status.LastImportedOperation = string.Empty;
            Status.Operations = 0;

            if (Operations?.Count > 0)
            {
                var totalOut = Operations.Sum(op => op.Outcome);
                var totalIn = Operations.Sum(op => op.Income);
                Status.Balance += totalIn - totalOut;
                Status.LastImportedOperation = Operations.FirstOrDefault()?.OperationId;
                Status.Operations = Operations.Count;
            }
        }
    }
}