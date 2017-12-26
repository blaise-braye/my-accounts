using System;
using MyAccounts.Business.AccountOperations.Contracts;

namespace MyAccounts.Business.AccountOperations.Unified
{
    public class UnifiedAccountOperation : AccountOperationBase
    {
        public UnifiedAccountOperation()
        {
            UId = Guid.NewGuid();
        }
        
        public Guid UId { get; set; }

        public string Account { get; set; }

        public string OperationId { get; set; }

        public string PatternName { get; set; }

        public DateTime ExecutionDate { get; set; }

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

        public string Category { get; set; }

        public string ThirdPartyOperationRef { get; set; }

        public string Communication { get; set; }

        public string Note { get; set; }

        public string GetCategoryByLevel(int level)
        {
            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                return string.Empty;
            }

            var categs = Category?.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            
            var blank = categs == null || level >= categs.Length;
            return blank ? string.Empty : categs[level];
        }
    }
}