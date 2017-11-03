using System;
using MyAccounts.Classification.MobileUi.Models;
using MyAccounts.Classification.MobileUi.ViewModels;
using Xamarin.Forms;

namespace MyAccounts.Classification.MobileUi
{
    public partial class AccountsPage : ContentPage
    {
        public AccountsPage()
        {
            InitializeComponent();
        }

        private AccountsViewModel ViewModel => BindingContext as AccountsViewModel;

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (!(args.SelectedItem is Item item))
                return;

            await Navigation.PushAsync(new AccountDetailPage(new AccountDetailViewModel(item)));

            // Manually deselect item
            ItemsListView.SelectedItem = null;
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewAccountPage());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel.Items.Count == 0)
                ViewModel.LoadItemsCommand.Execute(null);
        }
    }
}