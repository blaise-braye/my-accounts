using System;
using System.Collections.Generic;
using System.Linq;

namespace Operations.Classification.WpfUi.Technical.Collections.Filters
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
                     filtered = filtered.Where(d => selector(d) >= FromDate.Value);
                }

                if (ToDate.HasValue)
                {
                    filtered = filtered.Where(d => selector(d) <= ToDate.Value);
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