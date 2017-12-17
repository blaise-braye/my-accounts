using System.Collections.Generic;
using System.Globalization;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Fortis;
using MyAccounts.Business.AccountOperations.Sodexo;
using MyAccounts.Business.AccountOperations.Unified;

namespace MyAccounts.Business.AccountOperations
{
    public class AccountToUnifiedOperationMapper
    {
        private readonly Dictionary<SourceKind, AccountToUnifiedOperationMapperBase> _mappers =
            new Dictionary<SourceKind, AccountToUnifiedOperationMapperBase>();

        public AccountToUnifiedOperationMapper()
        {
            _mappers[SourceKind.FortisCsvArchive] = new FortisToUnifiedAccountOperationMapper();
            _mappers[SourceKind.FortisCsvExport] = _mappers[SourceKind.FortisCsvArchive];
            _mappers[SourceKind.SodexoCsvExport] = new SodexoToUnifiedAccountOperationMapper();
            _mappers[SourceKind.InternalCsvExport] = new UnifiedAccountOperationToUnifiedAccountOperationMapper();
        }

        public UnifiedAccountOperation Map(AccountOperationBase operationBase, CultureInfo culture)
        {
            var mapper = _mappers[operationBase.SourceKind];
            var unifiedOperation = mapper.Map(operationBase, culture);
            return unifiedOperation;
        }
    }
}