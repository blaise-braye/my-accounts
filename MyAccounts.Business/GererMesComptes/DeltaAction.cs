namespace MyAccounts.Business.GererMesComptes
{
    public enum DeltaAction
    {
        Nothing,
        Add,
        Remove,
        UpdateMemo,
        NotUniqueKeyInTarget,
        MultipleTargetsPossible
    }
}