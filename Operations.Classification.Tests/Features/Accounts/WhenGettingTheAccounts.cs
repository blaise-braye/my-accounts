using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Operations.Classification.Managers;
using Operations.Classification.Managers.Accounts;
using Operations.Classification.Tests.AutoFixtures;

namespace Operations.Classification.Tests.Features.Accounts
{
    [TestFixture(Category = "UnitTest")]
    public class WhenGettingTheAccounts
    {
        [Test]
        [AccountsRepositoryAutoData]
        public async Task ThenStoredAccountsAreReturned(
            AccountsRepository sut,
            IWorkingCopy workingCopy,
            IMockFileDataAccessor mockFileDataAccessor,
            Wrapper<string> existingSettingPath,
            List<AccountEntity> existingEntities)
        {
            //Arrange
            var rawEntities = JsonConvert.SerializeObject(existingEntities);
            mockFileDataAccessor.AddFile(existingSettingPath, new MockFileData(rawEntities));
            Mock.Get(workingCopy).Setup(w => w.SettingsPath).Returns(existingSettingPath);
            
            //Act
            var accounts = await sut.GetList();

            // Assert
            accounts.Count.Should().Be(existingEntities.Count);
        }

        [Test]
        [AccountsRepositoryAutoData]
        public async Task ThenAnEmptyListIsReturnedWhenSettingsFileDoesNotExist(
            AccountsRepository sut,
            IWorkingCopy workingCopy,
            Wrapper<string> inexistingSettingPath)
        {
            //Arrange
            Mock.Get(workingCopy).Setup(w => w.SettingsPath).Returns(inexistingSettingPath);

            //Act
            var accounts = await sut.GetList();

            // Assert
            accounts.Should().BeEmpty();
        }

        [Test]
        [AccountsRepositoryAutoData]
        public async Task ThenAnEmptyListIsReturnedWhenReadingTheFileFails(
            AccountsRepository sut,
            IWorkingCopy workingCopy,
            IMockFileDataAccessor mockFileDataAccessor,
            Wrapper<string> existingSettingPath)
        {
            //Arrange
            mockFileDataAccessor.AddFile(existingSettingPath, new MockFileData("{ invalid json }"));
            Mock.Get(workingCopy).Setup(w => w.SettingsPath).Returns(existingSettingPath);

            //Act
            var accounts = await sut.GetList();

            // Assert
            accounts.Should().BeEmpty();
        }
    }
}
