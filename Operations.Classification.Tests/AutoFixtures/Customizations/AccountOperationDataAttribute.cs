using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.Tests.AutoFixtures.Builders;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.NUnit3;

namespace Operations.Classification.Tests.AutoFixtures.Customizations
{
    public class AccountOperationDataAttribute : AutoDataAttribute
    {
        public AccountOperationDataAttribute(SourceKind sourceKind)
            : base(
                new Fixture()
                    .Customize(new FortisOperationCustomization(sourceKind)))
        {
        }

        public class FortisOperationCustomization : ICustomization
        {
            private readonly SourceKind _sourceKind;

            public FortisOperationCustomization(SourceKind sourceKind)
            {
                _sourceKind = sourceKind;
            }

            public void Customize(IFixture fixture)
            {
                fixture.Customizations.Add(new AccountOperationBaseBuilder(_sourceKind));
                fixture.Customizations.Add(new DateBuilder());
            }
        }
    }
}