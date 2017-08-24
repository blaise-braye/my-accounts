using System.Threading.Tasks;

namespace MyAccounts.Business.IO.Caching
{
    public interface IRawCacheRepository
    {
        bool IsConnected(string cacheKey);

        Task<bool> KeyExistsAsync(string cacheKey);

        Task<bool> KeyDeleteAsync(string cacheKey);

        Task<bool> StringSetAsync(string cacheKey, string rawResult);

        Task<CacheValue> StringGetAsync(string cacheKey);

        Task ClearCache();
    }

    public struct CacheValue
    {
        public static readonly CacheValue Null = new CacheValue(null);

        public CacheValue(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public bool IsNull => Value == null;

        public bool HasValue => Value != null;

        public static implicit operator string(CacheValue value)
        {
            if (value.IsNull)
            {
                return string.Empty;
            }

            return value.Value.ToString();
        }

        public static implicit operator CacheValue(string value)
        {
            return new CacheValue(value);
        }
    }
}