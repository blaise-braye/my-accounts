using System;
using System.Collections.Generic;
using System.Linq;
using MyAccounts.Business.AccountOperations.Unified;

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
        
        public static List<OperationSet> GroupDailyOperations(List<OperationSet> operations)
        {
            var groupedByDay = operations.GroupBy(op => op.Day);

            var flattifiedDailyOperations = groupedByDay.Select(
                grp =>
                {
                    var bpd = new OperationSet(grp.Key, grp.Sum(pd => pd.InitialBalance));
                    var grpOperations = grp.SelectMany(p => p.Operations);
                    bpd.AddRange(grpOperations);
                    return bpd;
                }).ToList();

            return flattifiedDailyOperations;
        }

        public static List<OperationSet> ComputeMonthlyOperations(List<OperationSet> dailyOperations)
        {
            var result = new List<OperationSet>();

            var groupedByMonth = dailyOperations.OrderBy(db => db.Day).GroupBy(db => new DateTime(db.Day.Year, db.Day.Month, 1));
            foreach (var grp in groupedByMonth)
            {
                var initialBalance = grp.First().InitialBalance;
                var operations = grp.SelectMany(b => b.Operations);
                var osb = new OperationSet(grp.Key, initialBalance).AddRange(operations);
                result.Add(osb);
            }

            return result;
        }

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