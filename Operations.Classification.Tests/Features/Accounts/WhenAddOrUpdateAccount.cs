using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Operations.Classification.Managers.Accounts;

namespace Operations.Classification.Tests.Features.Accounts
{
    [TestFixture(Category = "UnitTest")]
    public class WhenAddOrUpdateAccount
    {
        [Test]
        [AccountsRepositoryAutoData]
        public async Task ThenAccountIsAddedIfNew(
            AccountsRepository sut,
            AccountEntity newEntity)
        {
            var expectedCountAfterAdd = (await sut.GetList()).Count + 1;

            // Act
            await sut.AddOrUpdate(newEntity);

            // Assert
            var actualCountAfterAdd = (await sut.GetList()).Count;
            actualCountAfterAdd.Should().Be(expectedCountAfterAdd);
        }


        [Test]
        [AccountsRepositoryAutoData]
        public async Task ThenAccountIsUpdatedIfExisting(AccountsRepository sut,
            AccountEntity newEntity)
        {
            // Arrange
            await sut.AddOrUpdate(newEntity);
            var expectedCountAfterUpdate = (await sut.GetList()).Count;

            // Act
            await sut.AddOrUpdate(newEntity);

            // Assert
            var actualCountAfterAdd = (await sut.GetList()).Count;
            actualCountAfterAdd.Should().Be(expectedCountAfterUpdate);
        }
    }
}