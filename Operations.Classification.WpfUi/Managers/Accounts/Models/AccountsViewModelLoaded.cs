using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;

namespace Operations.Classification.WpfUi.Managers.Accounts.Models
{
    public class AccountsViewModelLoaded : MessageBase
    {
        public AccountsViewModelLoaded(IList<AccountViewModel> accounts)
        {
            Accounts = accounts;
        }

        public IList<AccountViewModel> Accounts { get; }
    }
}