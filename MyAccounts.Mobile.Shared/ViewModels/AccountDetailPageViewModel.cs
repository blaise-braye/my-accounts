using GalaSoft.MvvmLight;
using MyAccounts.Mobile.Shared.Models;

namespace MyAccounts.Mobile.Shared.ViewModels
{
    public class AccountDetailPageViewModel : ViewModelBase
    {
        private string _title;

        public AccountDetailPageViewModel(Account account = null)
        {
            Title = account?.Name;
            Account = account;
        }
        
        public string Title
        {
            get => _title;
            set { Set(() => Title, ref _title, value); }
        }

        public Account Account { get; }
    }
}
