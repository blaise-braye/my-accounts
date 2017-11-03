using GalaSoft.MvvmLight;
using MyAccounts.Classification.MobileUi.Models;

namespace MyAccounts.Classification.MobileUi.ViewModels
{
    public class AccountDetailViewModel : ViewModelBase
    {
        private string _title;

        public AccountDetailViewModel(Item item = null)
        {
            Title = item?.Text;
            Item = item;
        }
        
        public string Title
        {
            get => _title;
            set { Set(() => Title, ref _title, value); }
        }

        public Item Item { get; }
    }
}
