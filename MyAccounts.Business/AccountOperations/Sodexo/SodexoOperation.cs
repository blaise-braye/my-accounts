using System;
using MyAccounts.Business.AccountOperations.Contracts;

namespace MyAccounts.Business.AccountOperations.Sodexo
{
    public class SodexoOperation : AccountOperationBase
    {
        public DateTime Date { get; set; }

        public string Detail { get; set; }

        public string Amount { get; set; }
    }
}