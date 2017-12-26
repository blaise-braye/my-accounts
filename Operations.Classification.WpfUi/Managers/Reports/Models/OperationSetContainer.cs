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

        public OperationSetGroup DailyCategories { get; private set; }

        public OperationSetGroup MonthlyCategories { get; private set; }

        public static OperationSetContainer Compute(IList<AccountViewModel> accounts)
        {
            var accountIdByOperationId = accounts.Where(a => a.Operations?.Any() == true).SelectMany(a =>
                    a.Operations.Select(o => new { AccountId = a.Id, OperationUid = o.UId }))
                .ToDictionary(o => o.OperationUid, o => o.AccountId);

            var alloperations = accounts
                .Where(a => a.Operations?.Any() == true)
                .SelectMany(a => a.Operations)
                .OrderBy(o => o.ExecutionDate).ToList();
            
            var compute = new OperationSetContainer();
            
            if (alloperations.Any())
            {
                var range = new DateRange
                {
                    Max = alloperations[alloperations.Count - 1].ExecutionDate,
                    Min = alloperations[0].ExecutionDate
                };
                
                var initialBalance = accounts.Sum(a => a.InitialBalance);

                var dailyOperations = AggregateOperations(initialBalance, range, RecurrenceFamily.Daily, alloperations);
                var monthlyOperations = AggregateOperations(initialBalance, range, RecurrenceFamily.Monthly, alloperations);
                var categoryDailyOperations = AggregateOperationsByCategory(range, RecurrenceFamily.Daily, alloperations);
                var categoryMonthlyOperations = AggregateOperationsByCategory(range, RecurrenceFamily.Monthly, alloperations);

                compute.AccountIdByOperationUId = new ReadOnlyDictionary<Guid, Guid>(accountIdByOperationId);
                compute.DailyOperations = dailyOperations;
                compute.MonthlyOperations = monthlyOperations;
                compute.DailyCategories = new OperationSetGroup(categoryDailyOperations, range, RecurrenceFamily.Daily);
                compute.MonthlyCategories = new OperationSetGroup(categoryMonthlyOperations, range, RecurrenceFamily.Monthly);
            }
            
            return compute;
        }

        private static List<GroupedOperationSet> AggregateOperationsByCategory(DateRange range, RecurrenceFamily recurrence, IEnumerable<UnifiedAccountOperation> operations)
        {
            var categoryGroups = operations.GroupBy(op => op.GetCategoryByLevel(0));
            var result = categoryGroups.Select(grp => new GroupedOperationSet
            {
                Key = grp.Key,
                PeriodicOperations = AggregateOperations(0, range, recurrence, grp)
            }).ToList();

            return result;
        }
        
        private static List<OperationSet> AggregateOperations(
            decimal initialBalance, 
            DateRange range,
            RecurrenceFamily recurrence,
            IEnumerable<UnifiedAccountOperation> orderedOperations)
        {
            var result = new List<OperationSet>();
            
            var start = recurrence.GetPeriod(range.Min);

            var currentBpd = new OperationSet(recurrence, start, initialBalance);
            result.Add(currentBpd);

            using (var operationEnumerator = orderedOperations.GetEnumerator())
            {
                while (operationEnumerator.MoveNext())
                {
                    var operation = operationEnumerator.Current;
                    if (operation == null) continue;

                    var operationPeriod = recurrence.GetPeriod(operation.ExecutionDate);

                    while (currentBpd.Period < operationPeriod)
                    {
                        currentBpd = OperationSet.CreateForNextStep(currentBpd);
                        result.Add(currentBpd);
                    }

                    currentBpd.Add(operation);
                }
            }
            
            start = currentBpd.Period;
            var end = recurrence.GetPeriod(range.Max);
            while (start < end)
            {
                currentBpd = OperationSet.CreateForNextStep(currentBpd);
                result.Add(currentBpd);
                start = currentBpd.Period;
            }

            return result;
        }
    }
}
