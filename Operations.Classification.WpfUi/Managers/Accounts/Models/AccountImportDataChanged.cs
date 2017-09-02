using System;
using GalaSoft.MvvmLight.Messaging;

namespace Operations.Classification.WpfUi.Managers.Accounts.Models
{
    public class AccountImportDataChanged : MessageBase
    {
        public AccountImportDataChanged(Guid accountId)
        {
            AccountId = accountId;
        }

        public Guid AccountId { get; }
    }
}