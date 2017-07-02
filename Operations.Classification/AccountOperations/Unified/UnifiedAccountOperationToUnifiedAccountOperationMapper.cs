using Operations.Classification.AccountOperations.Contracts;

namespace Operations.Classification.AccountOperations.Unified
{
    public class UnifiedAccountOperationToUnifiedAccountOperationMapper : AccountToUnifiedOperationMapperBase
    {
        public override UnifiedAccountOperation Map(AccountOperationBase operationBase)
        {
            return (UnifiedAccountOperation) operationBase;
        }
    }
}