using System;
using System.Collections.Generic;

using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.WpfUi.Managers.Reports.Models
{
    public class OperationSet
    {
        public OperationSet(DateTime day, decimal initialBalance)
        {
            Day = day;
            InitialBalance = initialBalance;
            Balance = initialBalance;
            Operations = new List<UnifiedAccountOperation>();
        }

        public DateTime Day { get; }

        public decimal InitialBalance { get; }

        public decimal Balance { get; private set; }

        public decimal Income { get; private set; }

        public decimal Outcome { get; private set; }

        public decimal NegativeOutcome => -Outcome;

        public decimal NetRevenue => Income - Outcome;

        public List<UnifiedAccountOperation> Operations { get; }

        public static OperationSet CreateForNextDay(OperationSet currentBpd)
        {
            return new OperationSet(currentBpd.Day.AddDays(1), currentBpd.Balance)
                       {
                           Balance = currentBpd.Balance
                       };
        }

        public void Add(UnifiedAccountOperation operation)
        {
            Operations.Add(operation);
            Income += operation.Income;
            Outcome += operation.Outcome;
            Balance = Balance + operation.Income - operation.Outcome;
        }

        public OperationSet AddRange(IEnumerable<UnifiedAccountOperation> operations)
        {
            foreach (var operation in operations)
            {
                Add(operation);
            }

            return this;
        }
    }
}