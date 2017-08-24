using System.Threading.Tasks;

namespace MyAccounts.Business.IO.Caching.InMemory
{
    public class NoCache : IRawCacheRepository
    {
        public bool IsConnected(string cacheKey)
        {
            return false;
        }

        public Task<bool> KeyExistsAsync(string cacheKey)
        {
            return Task.FromResult(false);
        }

        public Task<bool> KeyDeleteAsync(string cacheKey)
        {
            return Task.FromResult(false);
        }

        public Task<bool> StringSetAsync(string cacheKey, string rawResult)
        {
            return Task.FromResult(false);
        }

        public Task<CacheValue> StringGetAsync(string cacheKey)
        {
            return Task.FromResult(CacheValue.Null);
        }

        public Task ClearCache()
        {
            return Task.CompletedTask;
        }
    }
}