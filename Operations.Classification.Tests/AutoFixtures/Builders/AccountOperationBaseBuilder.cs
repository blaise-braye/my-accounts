using System;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Fortis;
using Operations.Classification.AccountOperations.Sodexo;
using Ploeh.AutoFixture.Kernel;

namespace Operations.Classification.Tests.AutoFixtures.Builders
{
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
            {
                return new NoSpecimen();
            }

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