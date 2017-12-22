using System;
using System.Threading.Tasks;
using log4net;
using MyAccounts.NetStandard.Logging;
using Newtonsoft.Json;

namespace MyAccounts.Business.IO.Caching
{
    public class JSonCacheEntry : ICacheEntry
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(JSonCacheEntry));
        private readonly string _cacheKey;

        private readonly Type _valueType;

        public JSonCacheEntry(IRawCacheRepository repository, string cacheKey, Type valueType)
        {
            Repository = repository;
            _cacheKey = cacheKey;
            _valueType = valueType;
        }

        private IRawCacheRepository Repository { get; }

        public async Task<object> GetOrSetAsync(Func<Task<object>> valueLoader)
        {
            var connected = Repository.IsConnected(_cacheKey);

            object result;

            if (!connected)
            {
                _log.Verbose($"Cache unavailable, computing value ({_cacheKey})");
                result = await valueLoader();
            }
            else
            {
                var keyExist = await Repository.KeyExistsAsync(_cacheKey);

                if (!keyExist)
                {
                    _log.Verbose($"Cache value not found, computing value ({_cacheKey})");
                    result = await valueLoader();

                    if (result != null && result.GetType() != _valueType)
                    {
                        _log.Verbose($"Won't cache computed value. Reason : invalid result type (expected : {_valueType}, actual {result.GetType()}");
                    }
                    else
                    {
                        await SetAsync(result);
                    }
                }
                else
                {
                    result = await GetAsync();
                }
            }

            return result;
        }

        public Task<bool> DeleteAsync()
        {
            var connected = Repository.IsConnected(_cacheKey);
            if (!connected)
            {
                _log.Verbose($"Cache unavailable, delete request ignored ({_cacheKey})");
                return Task.FromResult(false);
            }

            return Repository.KeyDeleteAsync(_cacheKey);
        }

        public async Task<bool> SetAsync(object value)
        {
            if (value != null && value.GetType() != _valueType)
            {
                _log.Verbose($"Won't cache value. Reason : invalid type (expected : {_valueType}, actual {value.GetType()}");
                return false;
            }

            var connected = Repository.IsConnected(_cacheKey);
            bool isSet;
            if (connected)
            {
                _log.Verbose($"Storing cache value ({_cacheKey})");
                var rawResult = JsonConvert.SerializeObject(value);

                isSet = await Repository.StringSetAsync(_cacheKey, rawResult);
                if (!isSet)
                {
                    _log.Warn($"failed to cache data ({_cacheKey})");
                }
            }
            else
            {
                _log.Verbose($"Cache unavailable, value won't be cached ({_cacheKey})");
                isSet = false;
            }

            return isSet;
        }

        public async Task<object> GetAsync()
        {
            var connected = Repository.IsConnected(_cacheKey);
            object result = null;

            if (!connected)
            {
                _log.Verbose($"Cache unavailable, returning null value ({_cacheKey})");
            }
            else
            {
                _log.Verbose($"Fetching cache value ({_cacheKey})");
                var cachedRawResult = await Repository.StringGetAsync(_cacheKey);
                _log.Verbose($"Fetched cache value ({_cacheKey})");
                result = Deserialize(cachedRawResult);
            }

            return result;
        }

        private object Deserialize(CacheValue cachedRawResult)
        {
            object result = null;

            if (cachedRawResult.HasValue)
            {
                try
                {
                    result = JsonConvert.DeserializeObject(cachedRawResult, _valueType);
                }
                catch (Exception exn)
                {
                    exn.Data["RawCache"] = (string)cachedRawResult;
                    _log.Error($"failed to deserialize cache raw value with key {_cacheKey}", exn);
                }
            }

            return result;
        }
    }
}