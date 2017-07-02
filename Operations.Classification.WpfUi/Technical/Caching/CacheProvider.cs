using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Operations.Classification.WpfUi.Properties;
using StackExchange.Redis;

namespace Operations.Classification.WpfUi.Technical.Caching
{
    public class CacheProvider
    {
        private static readonly CacheProvider _instance;
        private readonly ConnectionMultiplexer _connection;

        static CacheProvider()
        {
            var options = ConfigurationOptions.Parse(Settings.Default.RedisConnectionString);
            options.AbortOnConnectFail = false;
            options.ClientName = Assembly.GetEntryAssembly().GetName().Name;
            options.AllowAdmin = true;
            var connection = ConnectionMultiplexer.Connect(options);
            _instance = new CacheProvider(connection);
        }

        private CacheProvider(ConnectionMultiplexer connection)
        {
            _connection = connection;
        }

        private static ConnectionMultiplexer Connection => _instance._connection;
        
        public static JSonCacheEntry<TValue> GetJSonCacheEntry<TValue>(string route)
        {
            return new JSonCacheEntry<TValue>(Connection, route);
        }

        ~CacheProvider()
        {
            _connection.Dispose();
        }

        public static async Task ClearCache()
        {
            var servers = Connection.GetEndPoints(true).Select(e => Connection.GetServer(e));
            foreach (var server in servers)
            {
                await server.FlushAllDatabasesAsync();
            }
        }
    }
}