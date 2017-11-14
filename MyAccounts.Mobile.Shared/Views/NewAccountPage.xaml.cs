using System;
using MyAccounts.Mobile.Shared.Models;
using Xamarin.Forms;

namespace MyAccounts.Mobile.Shared.Views
{
    public partial class NewAccountPage : ContentPage
    {
        public NewAccountPage()
        {
            InitializeComponent();

            Account = new Account
            {
                Name = "Item name",
                Summary = "This is an item description."
            };

            BindingContext = this;
        }

        public Account Account { get; set; }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "AddItem", Account);
            await Navigation.PopToRootAsync();
        }
    }
}
