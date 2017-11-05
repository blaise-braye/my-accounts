using MyAccounts.Mobile.Shared.Views;
using Xamarin.Forms;

namespace MyAccounts.Mobile.Shared
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                MainPage = new MainPage();
            }
            else
            {
                MainPage = new NavigationPage(new MainPage());
            }
        }
    }
}