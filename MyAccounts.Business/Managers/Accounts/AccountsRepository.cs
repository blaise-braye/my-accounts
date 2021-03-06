﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MyAccounts.Business.IO;
using Newtonsoft.Json;

namespace MyAccounts.Business.Managers.Accounts
{
    public class AccountsRepository : IAccountsRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccountsRepository));

        private static readonly object _writeLock = new object();

        private readonly IWorkingCopy _workingCopy;

        public AccountsRepository(IWorkingCopy workingCopy)
        {
            _workingCopy = workingCopy;
        }

        private IFileSystem Fs => _workingCopy.Fs;

        public async Task<List<AccountEntity>> GetList()
        {
            List<AccountEntity> list;
            if (!Fs.FileExists(_workingCopy.SettingsPath))
            {
                list = new List<AccountEntity>();
            }
            else
            {
                try
                {
                    using (var destinationStream = Fs.FileOpenRead(_workingCopy.SettingsPath))
                    using (var sw = new StreamReader(destinationStream))
                    {
                        var rawSettings = await sw.ReadToEndAsync();
                        list = JsonConvert.DeserializeObject<List<AccountEntity>>(rawSettings);
                    }
                }
                catch (Exception exn)
                {
                    _logger.Error($"Could not read setting file ({_workingCopy.SettingsPath})", exn);
                    list = new List<AccountEntity>();
                }
            }

            return list;
        }

        public async Task<bool> AddOrUpdate(AccountEntity entity)
        {
            var entities = await GetList();
            var existingId = entities.FindIndex(e => e.Id == entity.Id);

            if (existingId >= 0)
            {
                entities.RemoveAt(existingId);
                entities.Insert(existingId, entity);
            }
            else
            {
                entities.Add(entity);
            }

            return await ReplaceAll(entities);
        }

        public async Task<AccountEntity> Find(Guid entityId)
        {
            var entites = await GetList();
            return entites.Find(e => e.Id == entityId);
        }

        public async Task<bool> Delete(Guid accountId)
        {
            var entities = await GetList();
            var idx = entities.FindIndex(a => a.Id == accountId);
            bool result = false;

            if (idx >= 0)
            {
                entities.RemoveAt(idx);
                result = await ReplaceAll(entities);
            }

            return result;
        }

        private async Task<bool> ReplaceAll(List<AccountEntity> entities)
        {
            await _workingCopy.CreateFolderIfDoesNotExistsYet(_workingCopy.Root);

            var rawJson = JsonConvert.SerializeObject(entities);
            try
            {
                Monitor.Enter(_writeLock);
                using (var destinationStream = Fs.FileCreate(_workingCopy.SettingsPath))
                using (var sw = new StreamWriter(destinationStream))
                {
                    await sw.WriteAsync(rawJson);
                }
            }
            finally
            {
                Monitor.Exit(_writeLock);
            }

            return true;
        }
    }
}