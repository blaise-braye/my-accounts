using System;
using System.Collections.Generic;
using MyAccounts.Business.AccountOperations.Unified;

namespace Operations.Classification.WpfUi.Managers.Reports.Models
{
    public class OperationSet
    {
        public OperationSet(RecurrenceFamily recurrence, DateTime period, decimal initialBalance)
        {
            Recurrence = recurrence;
            Period = period;
            InitialBalance = initialBalance;
            Balance = initialBalance;
            Operations = new List<UnifiedAccountOperation>();
        }
        
        public RecurrenceFamily Recurrence { get; }

        public DateTime Period { get; }

        public decimal InitialBalance { get; }

        public decimal Balance { get; private set; }

        public decimal Income { get; private set; }

        public decimal Outcome { get; private set; }

        public decimal NegativeOutcome => -Outcome;

        public decimal NetRevenue => Income - Outcome;

        public DateTime? MinExecutionDate { get; private set; }

        public DateTime? MaxExecutionDate { get; private set; }

        public List<UnifiedAccountOperation> Operations { get; }

        public static OperationSet CreateForNextStep(OperationSet currentBpd)
        {
            DateTime nextStepDay;
            switch (currentBpd.Recurrence)
            {
                case RecurrenceFamily.Daily:
                    nextStepDay = currentBpd.Period.AddDays(1);
                    break;
                case RecurrenceFamily.Monthly:
                    nextStepDay = currentBpd.Period.AddMonths(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new OperationSet(currentBpd.Recurrence, nextStepDay, currentBpd.Balance)
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

            if (!MinExecutionDate.HasValue || operation.ExecutionDate < MinExecutionDate)
            {
                MinExecutionDate = operation.ExecutionDate;
            }

            if (!MaxExecutionDate.HasValue || operation.ExecutionDate > MaxExecutionDate)
            {
                MaxExecutionDate = operation.ExecutionDate;
            }
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