using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using Operations.Classification.WpfUi.Data;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.NUnit3;

namespace Operations.Classification.WpfUi.Tests.Data.Accounts
{
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
            var settingsPath = fixture.Create<string>();
            mock.Setup(w => w.SettingsPath).Returns(settingsPath);
            fixture.Inject(mock.Object);

            return fixture;
        }
    }
}