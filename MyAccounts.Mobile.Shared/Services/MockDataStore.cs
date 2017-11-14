using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyAccounts.Mobile.Shared.Models;
using MyAccounts.Mobile.Shared.Services;

[assembly:Xamarin.Forms.Dependency(typeof(MockDataStore))]
namespace MyAccounts.Mobile.Shared.Services
{
    public class MockDataStore : IDataStore<Account, Guid>
    {
        private readonly List<Account> _items;

        public MockDataStore()
        {
            _items = new List<Account>();
            var mockItems = new List<Account>
            {
                new Account { Id = Guid.NewGuid(), Name = "Aggregate cc", Summary = "Balance xxxx.xx €" },
                new Account { Id = Guid.NewGuid(), Name = "Blaise cc", Summary = "Balance  xxx.xx €" },
                new Account { Id = Guid.NewGuid(), Name = "Sylvie cc", Summary = "Balance  xxx.xx €" },
                new Account { Id = Guid.NewGuid(), Name = "Sodexo Blaise", Summary = "Balance   xx.xx €" }
            };

            foreach (var item in mockItems)
            {
                _items.Add(item);
            }
        }

        public async Task<bool> AddItemAsync(Account account)
        {
            _items.Add(account);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Account account)
        {
            var idx = _items.FindIndex(arg => arg.Id == account.Id);
            var result = idx >= 0;
            if (result)
            {
                _items.RemoveAt(idx);
                _items.Insert(idx, account);
            }
            
            return await Task.FromResult(result);
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            var idx = _items.FindIndex(arg => arg.Id == id);
            var result = idx >= 0;
            if (result)
            {
                _items.RemoveAt(idx);
            }
            
            return await Task.FromResult(result);
        }

        public async Task<Account> GetItemAsync(Guid id)
        {
            return await Task.FromResult(_items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Account>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(_items);
        }
    }
}
