using System.Threading.Tasks;
using FluentAssertions;
using MyAccounts.Business.Managers.Accounts;
using NUnit.Framework;

namespace Operations.Classification.Tests.Features.Accounts
{
    [TestFixture(Category = "UnitTest")]
    public class WhenDeletingAccount
    {
        [Test]
        [AccountsRepositoryAutoData]
        public async Task ThenAccountIsDeleted(
            AccountsRepository sut,
            AccountEntity newEntity)
        {
            // Arrange
            var expectedCountAfterDelete = (await sut.GetList()).Count;
            await sut.AddOrUpdate(newEntity);
            
            // Act
            var deleted = await sut.Delete(newEntity.Id);

            // Assert
            var actualCountAfterDelete = (await sut.GetList()).Count;
            deleted.Should().BeTrue();
            actualCountAfterDelete.Should().Be(expectedCountAfterDelete);
        }

        [Test]
        [AccountsRepositoryAutoData]
        public async Task ThenAccountIsNotDeletedIfAccountDoesNotExist(
            AccountsRepository sut,
            AccountEntity newEntity)
        {
            // Act
            var deleted = await sut.Delete(newEntity.Id);

            // Assert
            deleted.Should().BeFalse();
        }
    }
}