using System;
using QifApi.Transactions;

namespace MyAccounts.Business.GererMesComptes
{
    public class TransactionDelta
    {
        public string DeltaKey { get; set; }

        public BasicTransaction Source { get; set; }

        public BasicTransaction Target { get; set; }

        public DeltaAction Action { get; set; }

        public DateTime CreationDate { get; set; }
    }
}