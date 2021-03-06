﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyAccounts.Business.Managers.Accounts
{
    public interface IAccountsRepository
    {
        Task<bool> AddOrUpdate(AccountEntity entity);

        Task<AccountEntity> Find(Guid entityId);

        Task<List<AccountEntity>> GetList();
    }
}