using System.Globalization;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Unified;

namespace MyAccounts.Business.AccountOperations.Sodexo
{
    public class SodexoToUnifiedAccountOperationMapper : AccountToUnifiedOperationMapperBase<SodexoOperation>
    {
        public override UnifiedAccountOperation Map(SodexoOperation operation, CultureInfo culture)
        {
            var rawAmount = operation.Amount.Replace(" ", string.Empty);
            
            var amount = decimal.Parse(
                rawAmount,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowCurrencySymbol,
                culture);

            decimal income = 0, outcome = 0;
            if (amount < 0)
            {
                outcome = -amount;
            }
            else
            {
                income = amount;
            }

            var trx = new UnifiedAccountOperation
            {
                ValueDate = operation.Date,
                ExecutionDate = operation.Date,
                Income = income,
                Outcome = outcome,
                Currency = "EUR",
                Note = operation.Detail,
                SourceKind = operation.SourceKind
            };

            return trx;
        }
    }
}