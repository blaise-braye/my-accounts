using System;

using Operations.Classification.AccountOperations.Contracts;

namespace Operations.Classification.AccountOperations.Unified
{
    public class UnifiedAccountOperation : AccountOperationBase
    {
        public string Account { get; set; }

        public string OperationId { get; set; }

        public string PatternName { get; set; }

        public DateTime ValueDate { get; set; }

        public string Currency { get; set; }

        public decimal Income { get; set; }

        public decimal Outcome { get; set; }

        public string ThirdParty { get; set; }

        public string IBAN { get; set; }

        public string BIC { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Mandat { get; set; }

        public string ThirdPartyOperationRef { get; set; }
        
        public string Communication { get; set; }

        public string Note { get; set; }
    }
}