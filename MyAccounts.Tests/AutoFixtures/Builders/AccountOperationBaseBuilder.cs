using System;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Fortis;
using MyAccounts.Business.AccountOperations.Sodexo;
using MyAccounts.Business.AccountOperations.Unified;
using Ploeh.AutoFixture.Kernel;

namespace MyAccounts.Tests.AutoFixtures.Builders
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
                case SourceKind.InternalCsvExport:
                    instance = (AccountOperationBase)context.Resolve(typeof(UnifiedAccountOperation));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            instance.SourceKind = _sourceKind;
            return instance;
        }
    }
}