using QifApi.Transactions;

namespace Operations.Classification.GererMesComptes
{
    public static class QifDomExtensions
    {
        public static string GetBankTransactionLookupKey(this BasicTransaction t)
        {
            return $"{t.Date:yyyy-MM-dd}${t.Amount}";
        }
    }
}