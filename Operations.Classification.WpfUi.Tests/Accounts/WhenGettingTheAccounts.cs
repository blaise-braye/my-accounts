using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Operations.Classification.WpfUi.Data;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.NUnit3;

namespace Operations.Classification.WpfUi.Tests.Accounts
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

        public class AccountsRepositoryAutoDataAttribute : AutoDataAttribute
        {
            public AccountsRepositoryAutoDataAttribute() : base(CreateFixture())
            {
            }

            private static IFixture CreateFixture()
            {
                var fixture = new Fixture();

                var fs = new MockFileSystem();
                fixture.Inject<IFileSystem>(fs);
                fixture.Inject<IMockFileDataAccessor>(fs);

                var mock = new Mock<IWorkingCopy>();
                mock.Setup(w => w.Fs).Returns(fs);
                fixture.Inject(mock.Object);

                return fixture;
            }
        }
    }
}
