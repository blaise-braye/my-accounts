using System;

namespace Operations.Classification.WpfUi.Managers.Reports.Models
{
    public enum RecurrenceFamily
    {
        Daily,
        Monthly
    }
    
    public static class RecurrenceFamilyExtensions
    {
        public static DateTime GetPeriod(this RecurrenceFamily recurrence, DateTime day)
        {
            switch (recurrence)
            {
                case RecurrenceFamily.Daily:
                    return new DateTime(day.Year, day.Month, day.Day);
                case RecurrenceFamily.Monthly:
                    return new DateTime(day.Year, day.Month, 1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static DateTime GetNextPeriod(this RecurrenceFamily recurrence, DateTime day)
        {
            switch (recurrence)
            {
                case RecurrenceFamily.Daily:
                    return GetPeriod(recurrence, day).AddDays(1);
                case RecurrenceFamily.Monthly:
                    return GetPeriod(recurrence, day).AddMonths(1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static DateTime GetPreviousPeriod(this RecurrenceFamily recurrence, DateTime day)
        {
            switch (recurrence)
            {
                case RecurrenceFamily.Daily:
                    return GetPeriod(recurrence, day).AddDays(-1);
                case RecurrenceFamily.Monthly:
                    return GetPeriod(recurrence, day).AddMonths(-1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}