using System.Globalization;
using MyAccounts.Business.AccountOperations.Unified;

namespace MyAccounts.Business.AccountOperations.Contracts
{
    public abstract class AccountToUnifiedOperationMapperBase<T> : AccountToUnifiedOperationMapperBase
        where T : AccountOperationBase
    {
        public override UnifiedAccountOperation Map(AccountOperationBase operationBase, CultureInfo culture)
        {
            return Map((T)operationBase, culture);
        }

        public abstract UnifiedAccountOperation Map(T operationBase, CultureInfo culture);
    }
}