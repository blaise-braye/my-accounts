using System.Globalization;
using MyAccounts.Business.AccountOperations.Unified;

namespace MyAccounts.Business.AccountOperations.Contracts
{
    public abstract class AccountToUnifiedOperationMapperBase
    {
        public abstract UnifiedAccountOperation Map(AccountOperationBase operationBase, CultureInfo culture);
    }
}