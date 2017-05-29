using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.WpfUi.Managers.Accounts.Models
{
    public class AccountViewModel : ViewModelBase
    {
        private string _gmgAccountName;

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
            get { return _id; }
            set { Set(nameof(Id), ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(nameof(Name), ref _name, value); }
        }

        public string GmgAccountName
        {
            get { return _gmgAccountName; }
            set { Set(nameof(GmgAccountName), ref _gmgAccountName, value); }
        }

        public decimal InitialBalance
        {
            get { return _initialBalance; }
            set
            {
                if (Set(nameof(InitialBalance), ref _initialBalance, value))
                    RefreshStatus();
            }
        }

        public List<UnifiedAccountOperation> Operations
        {
            get { return _operations; }
            set
            {
                if (Set(nameof(Operations), ref _operations, value))
                    RefreshStatus();
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