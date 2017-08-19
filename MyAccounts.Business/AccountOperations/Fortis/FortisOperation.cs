using System;
using MyAccounts.Business.AccountOperations.Contracts;

namespace MyAccounts.Business.AccountOperations.Fortis
{
    public class FortisOperation : AccountOperationBase
    {
        public string Reference { get; set; }

        public DateTime ExecutionDate { get; set; }

        public DateTime ValueDate { get; set; }

        public string Amount { get; set; }

        public string Currency { get; set; }

        public string CounterpartyOfTheTransaction { get; set; }

        public string Detail { get; set; }

        public string Account { get; set; }
    }
}