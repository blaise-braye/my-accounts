namespace MyAccounts.Business.AccountOperations.Contracts
{
    public abstract class AccountOperationBase
    {
        public SourceKind SourceKind { get; set; }
    }
}