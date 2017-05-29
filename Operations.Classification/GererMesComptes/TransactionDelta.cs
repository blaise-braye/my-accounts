using QifApi.Transactions;

namespace Operations.Classification.GererMesComptes
{
    public class TransactionDelta
    {
        public string DeltaKey { get; set; }

        public BasicTransaction Source { get; set; }

        public BasicTransaction Target { get; set; }

        public DeltaAction Action { get; set; }
    }
}