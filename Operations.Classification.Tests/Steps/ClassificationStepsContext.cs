using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using MyAccounts.Business.AccountOperations;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Unified;
using MyAccounts.Business.GeoLoc;
using MyAccounts.Business.Managers;
using MyAccounts.Business.Managers.Imports;
using MyAccounts.Business.Managers.Operations;
using MyAccounts.Business.Managers.Persistence;
using Operations.Classification.Tests.AutoFixtures;

namespace Operations.Classification.Tests.Steps
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
            OperationsRepository = new OperationsRepository(WorkingCopy, CsvAccountOperationManager, Transformer);
            ImportManager = new ImportManager(AccountCommandRepository, OperationsRepository);

            AccountId = Guid.NewGuid();
        }

        public ImportManager ImportManager { get; set; }

        public Guid AccountId { get; set; }

        public IOperationsRepository OperationsRepository { get; }

        public IAccountCommandRepository AccountCommandRepository { get; }

        public IWorkingCopy WorkingCopy { get; }

        public CsvAccountOperationManager CsvAccountOperationManager { get; }

        public UnifiedAccountOperationPatternTransformer Transformer { get; }

        public List<AccountOperationBase> ReadOperations { set; get; }

        public List<UnifiedAccountOperation> UnifiedOperations { set; get; }

        public byte[] AnOperationsFile { set; get; }
    }
}