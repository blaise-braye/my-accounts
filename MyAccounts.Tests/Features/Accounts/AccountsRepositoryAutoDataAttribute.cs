using System.IO.Abstractions.TestingHelpers;
using Moq;
using MyAccounts.Business.Managers;
using MyAccounts.Tests.AutoFixtures;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.NUnit3;

namespace MyAccounts.Tests.Features.Accounts
{
    public class AccountsRepositoryAutoDataAttribute : AutoDataAttribute
    {
        public AccountsRepositoryAutoDataAttribute()
            : base(CreateFixture())
        {
        }

        private static IFixture CreateFixture()
        {
            var fixture = new Fixture();

            var fs = new MockFileSystem();
            var libfs = new FileSystemAdapter(fs);
            fixture.Inject<MyAccounts.Business.IO.IFileSystem>(libfs);
            fixture.Inject<IMockFileDataAccessor>(fs);

            var mock = new Mock<IWorkingCopy>();
            mock.Setup(w => w.Fs).Returns(libfs);
            var settingsPath = fixture.Create<string>();
            mock.Setup(w => w.SettingsPath).Returns(settingsPath);
            fixture.Inject(mock.Object);

            return fixture;
        }
    }
}