using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyAccounts.Mobile.Shared.Services
{
    public interface IDataStore<TEntity, in TKey>
    {
        Task<bool> AddItemAsync(TEntity item);

        Task<bool> UpdateItemAsync(TEntity item);

        Task<bool> DeleteItemAsync(TKey key);

        Task<TEntity> GetItemAsync(TKey key);

        Task<IEnumerable<TEntity>> GetItemsAsync(bool forceRefresh = false);
    }
}
