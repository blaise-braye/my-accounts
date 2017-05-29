using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.Tests.Features.Classification.Customizations;

namespace Operations.Classification.Tests.Features.Classification
{
    [TestFixture]
    public class When_I_serialize_operations_to_csv_fie
    {
        private string _testTempFile;

        [Test, AccountOperationData(SourceKind.FortisCsvExport)]
        public async Task ThenDeserializationIsConsistentAndOriginalDataIsPreserved(CsvAccountOperationManager operationManager, List<AccountOperationBase> operations)
        {
            await operationManager.WriteAsync(_testTempFile, operations);
            var operationsState2 = await operationManager.ReadAsync(_testTempFile, operations[0].SourceKind);

            operationsState2.ShouldBeEquivalentTo(operations, o => o.IncludingAllRuntimeProperties());
        }

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

    }
}
