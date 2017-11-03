using System;
using MyAccounts.Classification.MobileUi.Models;
using Xamarin.Forms;

namespace MyAccounts.Classification.MobileUi
{
    public partial class NewAccountPage : ContentPage
    {
        public Item Item { get; set; }

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

        async void Save_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "AddItem", Item);
            await Navigation.PopToRootAsync();
        }
    }
}
