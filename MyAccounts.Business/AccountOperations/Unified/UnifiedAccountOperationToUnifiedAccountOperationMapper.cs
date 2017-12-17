using System.Globalization;
using MyAccounts.Business.AccountOperations.Contracts;

namespace MyAccounts.Business.AccountOperations.Unified
{
    public class UnifiedAccountOperationToUnifiedAccountOperationMapper : AccountToUnifiedOperationMapperBase
    {
        public override UnifiedAccountOperation Map(AccountOperationBase operationBase, CultureInfo culture)
        {
            return (UnifiedAccountOperation)operationBase;
        }
    }
}