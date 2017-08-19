using System.Globalization;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Unified;

namespace MyAccounts.Business.AccountOperations.Fortis
{
    public class FortisToUnifiedAccountOperationMapper : AccountToUnifiedOperationMapperBase<FortisOperation>
    {
        public override UnifiedAccountOperation Map(FortisOperation fortisOperation)
        {
            decimal income = 0, outcome = 0;
            if (!string.IsNullOrEmpty(fortisOperation.Amount))
            {
                var amount = decimal.Parse(
                    fortisOperation.Amount,
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                    CultureInfo.GetCultureInfo("fr-BE"));

                if (amount < 0)
                {
                    outcome = -amount;
                }
                else
                {
                    income = amount;
                }
            }

            var trx = new UnifiedAccountOperation
            {
                Account = fortisOperation.Account,
                OperationId = fortisOperation.Reference,
                ValueDate = fortisOperation.ValueDate,
                ExecutionDate = fortisOperation.ExecutionDate,
                Income = income,
                Outcome = outcome,
                Currency = fortisOperation.Currency,
                Note = fortisOperation.Detail,
                ThirdParty = fortisOperation.CounterpartyOfTheTransaction,
                SourceKind = fortisOperation.SourceKind
            };

            return trx;
        }
    }
}