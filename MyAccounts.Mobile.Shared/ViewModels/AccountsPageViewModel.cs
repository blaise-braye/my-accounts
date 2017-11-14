using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using MyAccounts.Mobile.Shared.Models;
using MyAccounts.Mobile.Shared.Services;
using MyAccounts.Mobile.Shared.Views;
using MyAccounts.NetStandard.Input;
using Xamarin.Forms;

namespace MyAccounts.Mobile.Shared.ViewModels
{
    public class AccountsPageViewModel : ViewModelBase
    {
        private string _title;

        public AccountsPageViewModel()
        {
            Title = "Accounts";
            Items = new ObservableCollection<Account>();
            LoadItemsCommand = new AsyncCommand(ExecuteLoadItemsCommand);
            
            MessagingCenter.Subscribe<NewAccountPage, Account>(this, "AddItem", async (obj, item) =>
            {
                Items.Add(item);
                await DataStore.AddItemAsync(item);
            });
        }
        
        public IDataStore<Account, Guid> DataStore => DependencyService.Get<IDataStore<Account, Guid>>() ?? new MockDataStore();

        public ObservableCollection<Account> Items { get; set; }

        public IAsyncCommand LoadItemsCommand { get; }
        
        public string Title
        {
            get => _title;
            set { Set(() => Title, ref _title, value); }
        }

        public BusyIndicatorViewModel BusyIndicator { get; } = new BusyIndicatorViewModel();

        private async Task ExecuteLoadItemsCommand()
        {
            if (BusyIndicator.IsBusy)
            {
                return;
            }

            using (BusyIndicator.EncapsulateActiveJobDescription(this, "Loading accounts"))
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
        }
    }
}
