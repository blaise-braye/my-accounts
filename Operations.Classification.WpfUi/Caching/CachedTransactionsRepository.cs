using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.WpfUi.Data;
using Operations.Classification.WpfUi.Technical.Caching;

namespace Operations.Classification.WpfUi.Caching
{
    public class CachedTransactionsRepository : ITransactionsRepository
    {
        private const string UnifiedAccountOperationsByNameRoute = "/UnifiedAccountOperations/{0}";
        private readonly TransactionsRepository _repository;

        public CachedTransactionsRepository(TransactionsRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Import(string accountName, Stream importData, SourceKind sourceKind)
        {
            var result = await _repository.Import(accountName, importData, sourceKind);
            if (result)
            {
                await GetCacheEntry(accountName).DeleteAsync();
            }

            return result;
        }

        public async Task<List<UnifiedAccountOperation>> GetTransformedUnifiedOperations(string accountName)
        {
            var result = await GetCacheEntry(accountName).GetOrAddAsync(
                () => _repository.GetTransformedUnifiedOperations(accountName));

            return result;
        }

        private static JSonCacheEntry<List<UnifiedAccountOperation>> GetCacheEntry(string accountName)
        {
            return CacheProvider.GetJSonCacheEntry<List<UnifiedAccountOperation>>(string.Format(UnifiedAccountOperationsByNameRoute, accountName));
        }
    }
}