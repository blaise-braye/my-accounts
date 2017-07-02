using System.Globalization;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.AccountOperations.Sodexo
{
    public class SodexoToUnifiedAccountOperationMapper : AccountToUnifiedOperationMapperBase<SodexoOperation>
    {
        public override UnifiedAccountOperation Map(SodexoOperation operation)
        {
            var rawAmount = operation.Amount.Replace(" ", string.Empty);

            var culture = CultureInfo.GetCultureInfo("fr-BE");
            var numberformat = (NumberFormatInfo)culture.NumberFormat.Clone();
            numberformat.CurrencyDecimalSeparator = ".";

            var amount = decimal.Parse(
                rawAmount,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowCurrencySymbol,
                numberformat);

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