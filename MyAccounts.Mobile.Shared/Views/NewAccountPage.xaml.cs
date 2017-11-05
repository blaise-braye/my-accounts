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

            Item = new Item
            {
                Text = "Item name",
                Description = "This is an item description."
            };

            BindingContext = this;
        }

        public Item Item { get; set; }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "AddItem", Item);
            await Navigation.PopToRootAsync();
        }
    }
}
