using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Xamarin.Forms;

namespace MyAccounts.Mobile.Shared.ViewModels
{
    public class AboutPageViewModel : ViewModelBase
    {
        private string _title;

        public AboutPageViewModel()
        {
            Title = "About";

            OpenWebCommand = new RelayCommand(() => Device.OpenUri(new Uri("https://datadigest.com")));
        }
        
        public string Title
        {
            get => _title;
            set { Set(() => Title, ref _title, value); }
        }

        public ICommand OpenWebCommand { get; }
    }
}