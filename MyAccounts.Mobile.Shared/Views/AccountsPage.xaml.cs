using System;
using MyAccounts.Mobile.Shared.Models;
using MyAccounts.Mobile.Shared.ViewModels;
using Xamarin.Forms;

namespace MyAccounts.Mobile.Shared.Views
{
    public partial class AccountsPage : ContentPage
    {
        public AccountsPage()
        {
            InitializeComponent();
        }

        private AccountsPageViewModel ViewModel => BindingContext as AccountsPageViewModel;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel.Items.Count == 0)
            {
                ViewModel.LoadItemsCommand.Execute(null);
            }
        }

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as Account;
            if (item == null)
            {
                return;
            }

            await Navigation.PushAsync(new AccountDetailPage(new AccountDetailPageViewModel(item)));

            // Manually deselect item
            ItemsListView.SelectedItem = null;
        }

        private async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewAccountPage());
        }
    }
}