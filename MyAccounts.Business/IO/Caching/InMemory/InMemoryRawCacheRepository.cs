using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace MyAccounts.Business.IO.Caching.InMemory
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
            var result = _objectCache.TryGetValue(cacheKey, out object _);
            return Task.FromResult(result);
        }

        public Task<bool> KeyDeleteAsync(string cacheKey)
        {
            var result = false;

            var exists = _objectCache.TryGetValue(cacheKey, out object _);

            if (exists)
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

        public Task<CacheValue> StringGetAsync(string cacheKey)
        {
            var result = CacheValue.Null;

            var exists = _objectCache.TryGetValue(cacheKey, out object value);

            if (exists)
            {
                result = value as string;
            }

            return Task.FromResult(result);
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
            _objectCache = new MemoryCache(new MemoryCacheOptions());
            previous?.Dispose();
        }
    }
}
