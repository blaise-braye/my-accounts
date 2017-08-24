using System;
using System.Threading.Tasks;

namespace MyAccounts.Business.IO.Caching
{
    public interface ICacheEntry
    {
        Task<object> GetAsync();

        Task<object> GetOrSetAsync(Func<Task<object>> valueLoader);

        Task<bool> DeleteAsync();

        Task<bool> SetAsync(object value);
    }
}