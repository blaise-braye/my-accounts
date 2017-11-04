using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using MyAccounts.Business.AccountOperations;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Unified;
using MyAccounts.Business.GeoLoc;
using MyAccounts.Business.IO.Caching;
using MyAccounts.Business.IO.Caching.InMemory;
using MyAccounts.Business.Managers;
using MyAccounts.Business.Managers.Imports;
using MyAccounts.Business.Managers.Operations;
using MyAccounts.Business.Managers.Persistence;
using MyAccounts.Tests.AutoFixtures;

namespace MyAccounts.Tests.Steps
{
    public class ClassificationStepsContext
    {
        public ClassificationStepsContext()
        {
            CsvAccountOperationManager = new CsvAccountOperationManager();
            Transformer = new UnifiedAccountOperationPatternTransformer(
                new PlaceInfoResolver(PlaceProvider.Load(new PlacesRepository())));
            WorkingCopy = new WorkingCopy(new FileSystemAdapter(new MockFileSystem()), @"c:\WorkingCopy");
            
            AccountCommandRepository = new AccountCommandRepository(WorkingCopy);
            var operationsRepository = new OperationsRepository(WorkingCopy, CsvAccountOperationManager, Transformer);
            
            var cacheManager = new CacheManager(new NoCache());
            OperationsManager = new OperationsManager(cacheManager, operationsRepository);
            ImportManager = new ImportManager(cacheManager, AccountCommandRepository, OperationsManager);

            AccountId = Guid.NewGuid();
        }

        public ImportManager ImportManager { get; set; }

        public Guid AccountId { get; set; }
        
        public OperationsManager OperationsManager { get; }

        public IAccountCommandRepository AccountCommandRepository { get; }

        public IWorkingCopy WorkingCopy { get; }

        public CsvAccountOperationManager CsvAccountOperationManager { get; }

        public UnifiedAccountOperationPatternTransformer Transformer { get; }

        public List<AccountOperationBase> ReadOperations { get; set; }

        public List<UnifiedAccountOperation> UnifiedOperations { get; set; }

        public byte[] AnOperationsFile { get; set; }
    }
}