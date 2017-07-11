using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Operations.Classification.WpfUi.Technical.Caching.InMemory
{
    public class InMemoryRawCacheRepository : IRawCacheRepository, IDisposable
    {
        private MemoryCache _objectCache;

        public InMemoryRawCacheRepository()
        {
            ResetCache();
        }

        public bool IsConnected(string cacheKey)
        {
            return true;
        }

        public Task<bool> KeyExistsAsync(string cacheKey)
        {
            var result = _objectCache.Contains(cacheKey);
            return Task.FromResult(result);
        }

        public Task<bool> KeyDeleteAsync(string cacheKey)
        {
            var result = false;

            if (_objectCache.Contains(cacheKey))
            {
                _objectCache.Remove(cacheKey);
                result = true;
            }

            return Task.FromResult(result);
        }

        public Task<bool> StringSetAsync(string cacheKey, string rawResult)
        {
            _objectCache.Set(cacheKey, rawResult, DateTimeOffset.MaxValue);
            return Task.FromResult(true);
        }

        public Task<RedisValue> StringGetAsync(string cacheKey)
        {
            var value = RedisValue.Null;

            if (_objectCache.Contains(cacheKey))
            {
                value = _objectCache.Get(cacheKey) as string;
            }

            return Task.FromResult(value);
        }

        public Task ClearCache()
        {
            ResetCache();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _objectCache.Dispose();
            }
        }

        private void ResetCache()
        {
            var previous = _objectCache;
            _objectCache = new MemoryCache(GetType().Name);
            previous?.Dispose();
        }
    }
}
