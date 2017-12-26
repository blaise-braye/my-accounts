using System;
using System.Collections.Generic;

namespace Operations.Classification.WpfUi.Managers.Reports.Models
{
    public class GroupedOperationSet
    {
        public string Key { get; set; }

        public List<OperationSet> PeriodicOperations { get; set; }

        public Tuple<DateTime, DateTime> GetRange()
        {
            DateTime min = DateTime.MaxValue;
            DateTime max = DateTime.MinValue;
            
            foreach (var periodicOperation in PeriodicOperations)
            {
                if (periodicOperation.MinExecutionDate.HasValue && min < periodicOperation.MinExecutionDate)
                {
                    min = periodicOperation.MinExecutionDate.Value;
                }

                if (periodicOperation.MaxExecutionDate.HasValue && max > periodicOperation.MaxExecutionDate)
                {
                    max = periodicOperation.MaxExecutionDate.Value;
                }
            }

            return Tuple.Create(min, max);
        }
    }
}
