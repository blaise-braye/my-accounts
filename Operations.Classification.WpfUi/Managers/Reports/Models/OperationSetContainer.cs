using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MyAccounts.Business.AccountOperations.Unified;
using Operations.Classification.WpfUi.Managers.Accounts.Models;

namespace Operations.Classification.WpfUi.Managers.Reports.Models
{
    public class OperationSetContainer
    {
        public List<OperationSet> DailyOperations { get; private set; }

        public List<OperationSet> MonthlyOperations { get; private set; }

        public ReadOnlyDictionary<Guid, Guid> AccountIdByOperationUId { get; private set; }
        
        public static OperationSetContainer Compute(IEnumerable<AccountViewModel> selection)
        {
            var accountIdByOperationId = new Dictionary<Guid, Guid>();
            List<OperationSet> mixedAccountOperationOperations = new List<OperationSet>();
            foreach (var account in selection.Where(a => a.Operations?.Any() == true))
            {
                foreach (var operation in account.Operations)
                {
                    accountIdByOperationId.Add(operation.UId, account.Id);
                }

                var accountOperationsPerDay = ComputeOperationsPerDay(account.InitialBalance, account.Operations);
                mixedAccountOperationOperations.AddRange(accountOperationsPerDay);
            }
            
            var aggregattedDailyOperations = GroupDailyOperations(mixedAccountOperationOperations);
            var aggregattedMonthlyOperations = ComputeMonthlyOperations(aggregattedDailyOperations);

            return new OperationSetContainer
            {
                AccountIdByOperationUId = new ReadOnlyDictionary<Guid, Guid>(accountIdByOperationId),
                DailyOperations = aggregattedDailyOperations,
                MonthlyOperations = aggregattedMonthlyOperations
            };
        }

        private static List<OperationSet> ComputeOperationsPerDay(decimal initialBalance, List<UnifiedAccountOperation> operations)
        {
            operations = operations.OrderBy(o => o.ExecutionDate).ToList();

            var seedBpd = new OperationSet(operations[0].ExecutionDate, initialBalance);

            var result = new List<OperationSet> { seedBpd };

            if (operations.Any())
            {
                operations
                    .Aggregate(
                        seedBpd,
                        (currentBpd, operation) =>
                        {
                            var operationDay = operation.ExecutionDate;

                            while (currentBpd.Day < operationDay.AddDays(-1))
                            {
                                currentBpd = OperationSet.CreateForNextDay(currentBpd);
                                result.Add(currentBpd);
                            }

                            OperationSet nextBpd;

                            if (currentBpd.Day.Equals(operation.ExecutionDate))
                            {
                                nextBpd = currentBpd;
                                currentBpd.Add(operation);
                            }
                            else
                            {
                                nextBpd = OperationSet.CreateForNextDay(currentBpd);
                                nextBpd.Add(operation);
                                result.Add(nextBpd);
                            }

                            return nextBpd;
                        });
            }

            return result;
        }

        private static List<OperationSet> GroupDailyOperations(List<OperationSet> operations)
        {
            var groupedByDay = operations.OrderBy(op => op.Day).GroupBy(op => op.Day);

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

        private static List<OperationSet> ComputeMonthlyOperations(List<OperationSet> dailyOperations)
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
    }
}
