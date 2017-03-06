using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.AccountOperations.Contracts
{
    public abstract class AccountToUnifiedOperationMapperBase
    {
        public abstract UnifiedAccountOperation Map(AccountOperationBase operationBase);
    }
}