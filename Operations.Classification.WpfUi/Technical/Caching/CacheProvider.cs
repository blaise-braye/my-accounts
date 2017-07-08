using System.Threading.Tasks;
using log4net;
using Operations.Classification.WpfUi.Properties;
using Operations.Classification.WpfUi.Technical.Caching.InMemory;
using Operations.Classification.WpfUi.Technical.Caching.Redis;

namespace Operations.Classification.WpfUi.Technical.Caching
{
    public class CacheProvider
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(CacheProvider));
        private static readonly IRawCacheRepository _repository;

        static CacheProvider()
        {
            if (string.IsNullOrEmpty(Settings.Default.RedisConnectionString))
            {
                _repository = new InMemoryRawCacheRepository();
            }
            else
            {
                _repository = new RedisRawCacheRepository(Settings.Default.RedisConnectionString);
            }

            _log.Info($"Cache provider initialized, working with repository {_repository.GetType()}");
        }
        
        public static ICacheEntry<TValue> GetJSonCacheEntry<TValue>(string route)
        {
            return new JSonCacheEntry<TValue>(_repository, route);
        }
        
        public static Task ClearCache()
        {
            return _repository.ClearCache();
        }
    }
}