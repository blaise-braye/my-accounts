using QifApi.Transactions;

namespace MyAccounts.Business.GererMesComptes
{
    public static class QifDomExtensions
    {
        public static string GetBankTransactionLookupKey(this BasicTransaction t)
        {
            return $"{t.Date:yyyy-MM-dd}${t.Amount:N2}";
        }
    }
}