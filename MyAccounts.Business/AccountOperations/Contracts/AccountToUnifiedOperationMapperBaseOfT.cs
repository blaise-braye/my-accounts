using MyAccounts.Business.AccountOperations.Unified;

namespace MyAccounts.Business.AccountOperations.Contracts
{
    public abstract class AccountToUnifiedOperationMapperBase<T> : AccountToUnifiedOperationMapperBase
        where T : AccountOperationBase
    {
        public override UnifiedAccountOperation Map(AccountOperationBase operationBase)
        {
            return Map((T)operationBase);
        }

        public abstract UnifiedAccountOperation Map(T operationBase);
    }
}