using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MyAccounts.Business.IO.Caching;
using StackExchange.Redis;

namespace Operations.Classification.WpfUi.Technical.Caching.Redis
{
    public class RedisRawCacheRepository : IRawCacheRepository
    {
        private readonly ConnectionMultiplexer _connection;

        public RedisRawCacheRepository(string connectionString)
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;
            options.ClientName = Assembly.GetEntryAssembly().GetName().Name;
            options.AllowAdmin = true;
            _connection = ConnectionMultiplexer.Connect(options);
        }

        ~RedisRawCacheRepository()
        {
            _connection.Dispose();
        }

        private IDatabase Database => _connection.GetDatabase();

        public bool IsConnected(string cacheKey)
        {
            return Database.IsConnected(cacheKey);
        }

        public Task<bool> KeyExistsAsync(string cacheKey)
        {
            return Database.KeyExistsAsync(cacheKey);
        }

        public Task<bool> KeyDeleteAsync(string cacheKey)
        {
            return Database.KeyDeleteAsync(cacheKey);
        }

        public Task<bool> StringSetAsync(string cacheKey, string rawResult)
        {
            return Database.StringSetAsync(cacheKey, rawResult);
        }

        public async Task<CacheValue> StringGetAsync(string cacheKey)
        {
            return (string)await Database.StringGetAsync(cacheKey);
        }

        public async Task ClearCache()
        {
            var servers = _connection.GetEndPoints(true).Select(e => _connection.GetServer(e));
            foreach (var server in servers)
            {
                if (server.IsConnected)
                {
                    await server.FlushAllDatabasesAsync();
                }
            }
        }
    }
}