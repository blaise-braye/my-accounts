using System;

namespace Operations.Classification.WpfUi.Managers.Reports.Models
{
    public enum RecurrenceFamily
    {
        Daily,
        Monthly,
        Yearly
    }
    
    public static class RecurrenceFamilyExtensions
    {
        public static string Format(this RecurrenceFamily recurrence, DateTime day)
        {
            switch (recurrence)
            {
                case RecurrenceFamily.Daily:
                    return day.ToString("d");
                case RecurrenceFamily.Monthly:
                    return day.ToString("yyyyMM");
                case RecurrenceFamily.Yearly:
                    return day.ToString("yyyy");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static DateTime GetPeriod(this RecurrenceFamily recurrence, DateTime day)
        {
            switch (recurrence)
            {
                case RecurrenceFamily.Daily:
                    return new DateTime(day.Year, day.Month, day.Day);
                case RecurrenceFamily.Monthly:
                    return new DateTime(day.Year, day.Month, 1);
                case RecurrenceFamily.Yearly:
                    return new DateTime(day.Year, 1, 1);
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
                    case RecurrenceFamily.Yearly:
                    return GetPeriod(recurrence, day).AddYears(1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}