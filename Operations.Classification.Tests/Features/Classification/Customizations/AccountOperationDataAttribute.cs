using System;
using System.Reflection;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Fortis;
using Operations.Classification.AccountOperations.Sodexo;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.NUnit3;

namespace Operations.Classification.Tests.Features.Classification.Customizations
{
    public class AccountOperationDataAttribute : AutoDataAttribute
    {
        public AccountOperationDataAttribute(SourceKind sourceKind)
            : base(new Fixture()
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

        public class DateBuilder : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                var pi = request as PropertyInfo;
                if (pi != null && pi.PropertyType == typeof(DateTime) && pi.Name.EndsWith("Date"))
                {
                    return context.Create<DateTime>().Date;
                }

                return new NoSpecimen();
            }
        }
        public class AccountOperationBaseBuilder : ISpecimenBuilder
        {
            private readonly SourceKind _sourceKind;

            public AccountOperationBaseBuilder(SourceKind sourceKind)
            {
                _sourceKind = sourceKind;
            }

            public object Create(object request, ISpecimenContext context)
            {
                var t = request as Type;
                if (t == null || t != typeof(AccountOperationBase))
                    return new NoSpecimen();
                AccountOperationBase instance;

                switch (_sourceKind)
                {
                    case SourceKind.FortisCsvArchive:
                    case SourceKind.FortisCsvExport:
                        instance = (AccountOperationBase)context.Resolve(typeof(FortisOperation));
                        break;
                    case SourceKind.SodexoCsvExport:
                        instance = (AccountOperationBase)context.Resolve(typeof(SodexoOperation));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                instance.SourceKind = _sourceKind;
                return instance;
            }
        }
    }

}