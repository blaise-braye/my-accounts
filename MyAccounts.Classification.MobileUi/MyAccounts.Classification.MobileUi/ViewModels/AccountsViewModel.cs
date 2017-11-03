﻿using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using MyAccounts.Classification.MobileUi.Models;
using MyAccounts.Classification.MobileUi.Services;
using MyAccounts.NetStandard.Input;
using Xamarin.Forms;

namespace MyAccounts.Classification.MobileUi.ViewModels
{
    public class AccountsViewModel : ViewModelBase
    {
        private string _title;

        public AccountsViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new AsyncCommand(ExecuteLoadItemsCommand);
            
            MessagingCenter.Subscribe<NewAccountPage, Item>(this, "AddItem", async (obj, item) =>
            {
                Items.Add(item);
                await DataStore.AddItemAsync(item);
            });
        }
        
        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>() ?? new MockDataStore();

        public ObservableCollection<Item> Items { get; set; }

        public IAsyncCommand LoadItemsCommand { get; }
        
        public string Title
        {
            get => _title;
            set { Set(() => Title, ref _title, value); }
        }

        public BusyIndicatorViewModel BusyIndicator { get; } = new BusyIndicatorViewModel();

        async Task ExecuteLoadItemsCommand()
        {
            if (BusyIndicator.IsBusy)
                return;

            using (BusyIndicator.EncapsulateActiveJobDescription(this, "Loading items"))
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
