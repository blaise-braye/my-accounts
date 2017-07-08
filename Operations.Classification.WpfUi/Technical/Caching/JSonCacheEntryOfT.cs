using System;
using System.Threading.Tasks;

namespace Operations.Classification.WpfUi.Technical.Caching
{
    public class JSonCacheEntry<TValue> : JSonCacheEntry, ICacheEntry<TValue>
    {
        public JSonCacheEntry(IRawCacheRepository repository, string cacheKey) : base(repository, cacheKey, typeof(TValue))
        {
        }

        public new async Task<TValue> GetAsync()
        {
            return (TValue)await base.GetAsync();
        }

        public async Task<TValue> GetOrAddAsync(Func<Task<TValue>> valueLoader)
        {
            return (TValue)await GetOrSetAsync(async () => await valueLoader());
        }

        public Task<bool> SetAsync(TValue value)
        {
            return base.SetAsync(value);
        }
    }
}