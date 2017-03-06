using System.Globalization;

using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.AccountOperations.Fortis
{
    public class FortisToUnifiedAccountOperationMapper : AccountToUnifiedOperationMapperBase<FortisOperation>
    {
        public override UnifiedAccountOperation Map(FortisOperation fortisOperation)
        {
            decimal amount = decimal.Parse(
                fortisOperation.Amount,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                CultureInfo.GetCultureInfo("fr-BE"));

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
                              Account = fortisOperation.Account,
                              OperationId = fortisOperation.Reference,
                              ValueDate = fortisOperation.ValueDate,
                              Income = income,
                              Outcome = outcome,
                              Currency = fortisOperation.Currency,
                              Note = fortisOperation.Detail,
                              SourceKind = fortisOperation.SourceKind
                          };

            return trx;
        }
    }
}
