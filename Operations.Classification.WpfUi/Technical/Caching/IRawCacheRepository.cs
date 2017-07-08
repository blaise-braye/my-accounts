using System.Threading.Tasks;
using StackExchange.Redis;

namespace Operations.Classification.WpfUi.Technical.Caching
{
    public interface IRawCacheRepository
    {
        bool IsConnected(string cacheKey);
        Task<bool> KeyExistsAsync(string cacheKey);
        Task<bool> KeyDeleteAsync(string cacheKey);
        Task<bool> StringSetAsync(string cacheKey, string rawResult);
        Task<RedisValue> StringGetAsync(string cacheKey);
        Task ClearCache();
    }
}