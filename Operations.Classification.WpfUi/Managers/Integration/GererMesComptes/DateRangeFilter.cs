using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    public class DateRangeFilter : ObservableObject, IFilter
    {
        private DateTime? _fromDate;
        private DateTime? _toDate;

        public DateTime? FromDate
        {
            get { return _fromDate; }
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
            get { return _toDate; }
            set
            {
                if (Set(nameof(ToDate), ref _toDate, value))
                {
                    OnFilterChanged();
                }
            }
        }

        public event EventHandler FilterInvalidated;

        public bool IsActive()
        {
            return FromDate.HasValue || ToDate.HasValue;
        }

        public void Reset()
        {
            FromDate = null;
            ToDate = null;
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
            FromDate = ToDate = objDate.Date;
        }

        protected virtual void OnFilterChanged()
        {
            FilterInvalidated?.Invoke(this, EventArgs.Empty);
        }
    }
}