using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAccounts.NetStandard.Collections.Filters
{
    public class DateRangeFilter : CompositeFilterBase
    {
        private DateTime? _fromDate;
        private DateTime? _toDate;
        
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                if (Set(nameof(FromDate), ref _fromDate, value))
                {
                    OnFilterChanged();
                }
            }
        }

        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                if (Set(nameof(ToDate), ref _toDate, value))
                {
                    OnFilterChanged();
                }
            }
        }

        public override bool IsActive()
        {
            return FromDate.HasValue || ToDate.HasValue;
        }

        public override void Reset()
        {
            Apply(() =>
            {
                FromDate = null;
                ToDate = null;
            });
        }

        public IEnumerable<T> Apply<T>(IEnumerable<T> locals, Func<T, DateTime> selector)
        {
            var filtered = locals;
            if (IsActive())
            {
                if (FromDate.HasValue)
                {
                    var fDate = FromDate.Value;
                     filtered = filtered.Where(d => selector(d) >= fDate);
                }

                if (ToDate.HasValue)
                {
                    var tDate = ToDate.Value;
                    filtered = filtered.Where(d => selector(d) <= tDate);
                }
            }

            return filtered;
        }

        public void SetDayFilter(DateTime objDate)
        {
            if (FromDate != objDate.Date || ToDate != objDate.Date)
            {
                Apply(() =>
                {
                    FromDate = ToDate = objDate.Date;
                });
            }
        }

        public void SetPeriod(DateTime fromDate, DateTime toDate)
        {
            if (FromDate != fromDate || ToDate != toDate)
            {
                Apply(() =>
                {
                    FromDate = fromDate.Date;
                    ToDate = toDate.Date;
                });
            }
        }
    }
}