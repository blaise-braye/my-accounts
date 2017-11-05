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
                var item = new Item
                {
                    Text = "Item 1",
                    Description = "This is an item description."
                };

                BindingContext = new AccountDetailViewModel(item);
            }            
        }

        public AccountDetailPage(AccountDetailViewModel viewModel) 
            : this()
        {
            BindingContext = viewModel;
        }
    }
}
