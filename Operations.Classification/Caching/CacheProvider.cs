using System;
using System.Threading.Tasks;
using log4net;

namespace Operations.Classification.Caching
{
    public class CacheProvider
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(CacheProvider));
        private static IRawCacheRepository _repository;

        public static void Init(IRawCacheRepository cacheRepository)
        {
            if (_repository != null)
            {
                throw new InvalidOperationException("Cache provider is already initialised");
            }

            _repository = cacheRepository;
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