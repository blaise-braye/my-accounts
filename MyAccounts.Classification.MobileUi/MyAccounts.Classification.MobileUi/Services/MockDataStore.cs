using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyAccounts.Classification.MobileUi.Models;

[assembly:Xamarin.Forms.Dependency(typeof(MyAccounts.Classification.MobileUi.Services.MockDataStore))]
namespace MyAccounts.Classification.MobileUi.Services
{
    public class MockDataStore : IDataStore<Item>
    {
        readonly List<Item> _items;

        public MockDataStore()
        {
            _items = new List<Item>();
            var mockItems = new List<Item>
            {
                new Item { Id = Guid.NewGuid().ToString(), Text = "First item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "Second item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "Third item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "Fourth item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "Fifth item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "Sixth item", Description="This is an item description." },
            };

            foreach (var item in mockItems)
            {
                _items.Add(item);
            }
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            _items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            var idx = _items.FindIndex(arg => arg.Id == item.Id);
            var result = idx >= 0;
            if (result)
            {
                _items.RemoveAt(idx);
                _items.Insert(idx, item);
            }
            
            return await Task.FromResult(result);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var idx = _items.FindIndex(arg => arg.Id == id);
            var result = idx >= 0;
            if (result)
            {
                _items.RemoveAt(idx);
            }
            
            return await Task.FromResult(result);
        }

        public async Task<Item> GetItemAsync(string id)
        {
            return await Task.FromResult(_items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(_items);
        }
    }
}
