using System.Threading.Tasks;
using FluentAssertions;
using MyAccounts.Business.Managers.Accounts;
using NUnit.Framework;

namespace MyAccounts.Tests.Features.Accounts
{
    [TestFixture(Category = "UnitTest")]
    public class WhenSearchingAccount
    {
        [Test]
        [AccountsRepositoryAutoData]
        public async Task ThenAccountCanBeFoundByIndex(
            AccountsRepository sut,
            AccountEntity newEntity)
        {
            // Arrange
            await sut.AddOrUpdate(newEntity);

            // Act
            var entity = await sut.Find(newEntity.Id);

            // Assert
            entity.Should().NotBe(newEntity);
            entity.Id.Should().Be(newEntity.Id);
            entity.Name.Should().Be(newEntity.Name);
        }

        [Test]
        [AccountsRepositoryAutoData]
        public async Task ThenNullIsReturnedIfCanNotFindByIndex(
            AccountsRepository sut,
            AccountEntity newEntity)
        {
            // Act
            var entity = await sut.Find(newEntity.Id);

            // Assert
            entity.Should().BeNull();
        }
    }
}