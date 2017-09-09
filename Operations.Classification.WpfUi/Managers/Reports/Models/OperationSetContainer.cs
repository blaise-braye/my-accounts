using System.Collections.Generic;
using System.Linq;
using MyAccounts.Business.AccountOperations.Unified;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Technical.Collections.Filters;

namespace Operations.Classification.WpfUi.Managers.Reports.Models
{
    public class OperationSetContainer
    {
        public List<OperationSet> DailyOperations { get; private set; }

        public List<OperationSet> MonthlyOperations { get; private set; }

        public static OperationSetContainer Compute(IEnumerable<AccountViewModel> selection)
        {
            var mixedAccountOperationOperations = selection
                .Where(a => a.Operations?.Any() == true)
                .SelectMany(account => ComputeOperationsPerDay(account.InitialBalance, account.Operations))
                .OrderBy(op => op.Day).ToList();

            var aggregattedDailyOperations = OperationSet.GroupDailyOperations(mixedAccountOperationOperations);
            var aggregattedMonthlyOperations = OperationSet.ComputeMonthlyOperations(aggregattedDailyOperations);

            return new OperationSetContainer
            {
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
    }
}
