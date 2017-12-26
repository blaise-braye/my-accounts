using System;
using System.Collections.Generic;

namespace Operations.Classification.WpfUi.Managers.Reports.Models
{
    public class OperationSetGroup : List<GroupedOperationSet>
    {
        public OperationSetGroup(List<GroupedOperationSet> operations, DateRange range, RecurrenceFamily recurrence) 
            : base(operations)
        {
            Range = range;
            Recurrence = recurrence;
        }

        public DateRange Range { get; }

        public RecurrenceFamily Recurrence { get; }

        public DateTime FirstDate => Recurrence.GetPeriod(Range.Min);

        public DateTime LastDate => Recurrence.GetPeriod(Range.Max);

        public int PeriodsCount => this[0].PeriodicOperations.Count;
    }
}