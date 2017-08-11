using System.Collections.Generic;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Fortis;
using Operations.Classification.AccountOperations.Sodexo;
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.AccountOperations
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

        public UnifiedAccountOperation Map(AccountOperationBase operationBase)
        {
            var mapper = _mappers[operationBase.SourceKind];
            var unifiedOperation = mapper.Map(operationBase);
            return unifiedOperation;
        }
    }
}