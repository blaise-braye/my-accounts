using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.Tests.AutoFixtures.Customizations;

namespace Operations.Classification.Tests.Features
{
    [TestFixture(Category = "UnitTest")]
    public class WhenISerializeOperationsToCsvFie
    {
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

        private string _testTempFile;

        [Test]
        [AccountOperationData(SourceKind.FortisCsvExport)]
        public async Task ThenDeserializationIsConsistentAndOriginalDataIsPreserved(
            CsvAccountOperationManager operationManager,
            List<AccountOperationBase> operations)
        {
            await operationManager.WriteAsync(_testTempFile, operations);
            var operationsState2 = await operationManager.ReadAsync(_testTempFile, operations[0].SourceKind);

            operationsState2.ShouldBeEquivalentTo(operations, o => o.IncludingAllRuntimeProperties());
        }
    }
}