using GalaSoft.MvvmLight;
using MyAccounts.Classification.MobileUi.Models;
using MyAccounts.Classification.MobileUi.ViewModels;
using Xamarin.Forms;

namespace MyAccounts.Classification.MobileUi
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

        public AccountDetailPage(AccountDetailViewModel viewModel) : this()
        {
            BindingContext = viewModel;
        }
    }
}
