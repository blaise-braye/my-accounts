using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyAccounts.Business.IO.Caching;

namespace MyAccounts.Business.Managers.Accounts
{
    public interface IAccountsManager
    {
        Task<bool> AddOrUpdate(AccountEntity entity);

        Task<IEnumerable<AccountEntity>> GetList();

        Task<bool> Delete(Guid accountId);
    }

    public class AccountsManager : IAccountsManager
    {
        private const string AccountsRoute = "/Accounts";
        private readonly AccountsRepository _accountsRepository;
        private readonly ICacheProvider _cacheProvider;

        public AccountsManager(ICacheProvider cacheProvider, AccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
            _cacheProvider = cacheProvider;
        }

        public async Task<bool> AddOrUpdate(AccountEntity entity)
        {
            var result = await _accountsRepository.AddOrUpdate(entity);
            if (result)
            {
                await GetCacheEntry().DeleteAsync();
            }

            return result;
        }

        public async Task<IEnumerable<AccountEntity>> GetList()
        {
            var result = await GetCacheEntry().GetOrAddAsync(
                () => _accountsRepository.GetList());

            return result;
        }

        public async Task<bool> Delete(Guid accountId)
        {
            var result = await _accountsRepository.Delete(accountId);
            if (result)
            {
                await GetCacheEntry().DeleteAsync();
            }

            return result;
        }

        private ICacheEntry<List<AccountEntity>> GetCacheEntry()
        {
            return _cacheProvider.GetJSonCacheEntry<List<AccountEntity>>(AccountsRoute);
        }
    }
}