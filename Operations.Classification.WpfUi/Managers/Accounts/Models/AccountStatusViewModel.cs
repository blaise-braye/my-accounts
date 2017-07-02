using GalaSoft.MvvmLight;

namespace Operations.Classification.WpfUi.Managers.Accounts.Models
{
    public class AccountStatusViewModel : ViewModelBase
    {
        private decimal _balance;
        private string _lastImportedOperation;

        private int _operations;

        public string LastImportedOperation
        {
            get => _lastImportedOperation;
            set => Set(nameof(LastImportedOperation), ref _lastImportedOperation, value);
        }

        public int Operations
        {
            get => _operations;
            set => Set(nameof(Operations), ref _operations, value);
        }

        public decimal Balance
        {
            get => _balance;
            set => Set(nameof(Balance), ref _balance, value);
        }
    }
}