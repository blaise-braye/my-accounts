using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Xamarin.Forms;

namespace MyAccounts.Classification.MobileUi.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private string _title;

        public AboutViewModel()
        {
            Title = "About";

            OpenWebCommand = new RelayCommand(() => Device.OpenUri(new Uri("https://xamarin.com/platform")));
        }
        
        public string Title
        {
            get => _title;
            set { Set(() => Title, ref _title, value); }
        }

        public ICommand OpenWebCommand { get; }
    }
}