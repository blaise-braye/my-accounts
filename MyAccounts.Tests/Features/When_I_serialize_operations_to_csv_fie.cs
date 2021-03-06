﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MyAccounts.Business.AccountOperations;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Tests.AutoFixtures.Customizations;
using NUnit.Framework;

namespace MyAccounts.Tests.Features
{
    [TestFixture(Category = "UnitTest")]
    public class WhenISerializeOperationsToCsvFie
    {
        private string _testTempFile;

        [SetUp]
        public void Setup()
        {
            _testTempFile = Path.GetTempFileName();
        }

        [TearDown]
        public void Cleanup()
        {
            try
            {
                File.Delete(_testTempFile);
            }
            catch
            {
                // ignored
            }
        }

        [Test]
        [AccountOperationData(SourceKind.FortisCsvExport)]
        public async Task ThenDeserializationIsConsistentAndOriginalDataIsPreservedWithFortisCsvExport(
            CsvAccountOperationManager operationManager,
            List<AccountOperationBase> operations)
        {
            var ms = new MemoryStream();
            await operationManager.WriteAsync(ms, operations);
            var fileStructureMetadata = FileStructureMetadataFactory.CreateDefault(operations[0].SourceKind);
            ms.Seek(0, SeekOrigin.Begin);
            var operationsState2 = await operationManager.ReadAsync(ms, fileStructureMetadata);

            operationsState2.ShouldBeEquivalentTo(operations, o => o.IncludingAllRuntimeProperties());
        }

        [Test]
        [AccountOperationData(SourceKind.FortisCsvArchive)]
        public async Task ThenDeserializationIsConsistentAndOriginalDataIsPreservedWithFortisCsvArchive(
            CsvAccountOperationManager operationManager,
            List<AccountOperationBase> operations)
        {
            var ms = new MemoryStream();
            await operationManager.WriteAsync(ms, operations);
            var fileStructureMetadata = FileStructureMetadataFactory.CreateDefault(operations[0].SourceKind);
            ms.Seek(0, SeekOrigin.Begin);
            var operationsState2 = await operationManager.ReadAsync(ms, fileStructureMetadata);

            operationsState2.ShouldBeEquivalentTo(operations, o => o.IncludingAllRuntimeProperties());
        }
        
        [Test]
        [AccountOperationData(SourceKind.SodexoCsvExport)]
        public async Task ThenDeserializationIsConsistentAndOriginalDataIsPreservedWithSodexoExport(
            CsvAccountOperationManager operationManager,
            List<AccountOperationBase> operations)
        {
            var ms = new MemoryStream();
            await operationManager.WriteAsync(ms, operations);
            var fileStructureMetadata = FileStructureMetadataFactory.CreateDefault(operations[0].SourceKind);
            ms.Seek(0, SeekOrigin.Begin);
            var operationsState2 = await operationManager.ReadAsync(ms, fileStructureMetadata);

            operationsState2.ShouldBeEquivalentTo(operations, o => o.IncludingAllRuntimeProperties());
        }

        [Test]
        [AccountOperationData(SourceKind.InternalCsvExport)]
        public async Task ThenDeserializationIsConsistentAndOriginalDataIsPreservedWithInternalExport(
            CsvAccountOperationManager operationManager,
            List<AccountOperationBase> operations)
        {
            var ms = new MemoryStream();
            await operationManager.WriteAsync(ms, operations);
            var fileStructureMetadata = FileStructureMetadataFactory.CreateDefault(operations[0].SourceKind);
            ms.Seek(0, SeekOrigin.Begin);
            var operationsState2 = await operationManager.ReadAsync(ms, fileStructureMetadata);

            operationsState2.ShouldBeEquivalentTo(operations, o => o.IncludingAllRuntimeProperties());
        }
    }
}