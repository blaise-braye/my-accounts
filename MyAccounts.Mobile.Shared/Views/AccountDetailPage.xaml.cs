using GalaSoft.MvvmLight;
using MyAccounts.Mobile.Shared.Models;
using MyAccounts.Mobile.Shared.ViewModels;
using Xamarin.Forms;

namespace MyAccounts.Mobile.Shared.Views
{
    public partial class AccountDetailPage : ContentPage
    {
        public AccountDetailPage()
        {
            InitializeComponent();

            if (ViewModelBase.IsInDesignModeStatic)
            {
                var item = new Account
                {
                    Name = "Item 1",
                    Summary = "This is an item description."
                };

                BindingContext = new AccountDetailPageViewModel(item);
            }            
        }

        public AccountDetailPage(AccountDetailPageViewModel viewModel) 
            : this()
        {
            BindingContext = viewModel;
        }
    }
}
