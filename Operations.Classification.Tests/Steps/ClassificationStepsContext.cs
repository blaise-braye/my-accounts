using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.GeoLoc;
using Operations.Classification.Managers;
using Operations.Classification.Managers.Imports;
using Operations.Classification.Managers.Operations;
using Operations.Classification.Managers.Persistence;

namespace Operations.Classification.Tests.Steps
{
    public class ClassificationStepsContext
    {
        public ClassificationStepsContext()
        {
            CsvAccountOperationManager = new CsvAccountOperationManager();
            Transformer = new UnifiedAccountOperationPatternTransformer(
                new PlaceInfoResolver(PlaceProvider.Load(new PlacesRepository())));
            WorkingCopy = new WorkingCopy(new MockFileSystem(), @"c:\WorkingCopy");
            
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