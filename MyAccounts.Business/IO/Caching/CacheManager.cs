using System.Threading.Tasks;

namespace MyAccounts.Business.IO.Caching
{
    public interface ICacheProvider
    {
        ICacheEntry<TValue> GetJSonCacheEntry<TValue>(string route);
    }

    public interface ICachemanager : ICacheProvider
    {
        Task ClearCache();
    }

    public class CacheManager : ICachemanager
    {
        private readonly IRawCacheRepository _repository;

        public CacheManager(IRawCacheRepository cacheRepository)
        {
            _repository = cacheRepository;
        }

        public ICacheEntry<TValue> GetJSonCacheEntry<TValue>(string route)
        {
            return new JSonCacheEntry<TValue>(_repository, route);
        }

        public Task ClearCache()
        {
            return _repository.ClearCache();
        }
    }
}