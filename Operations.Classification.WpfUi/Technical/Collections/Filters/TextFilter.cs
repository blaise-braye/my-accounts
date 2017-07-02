using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GalaSoft.MvvmLight;

namespace Operations.Classification.WpfUi.Technical.Collections.Filters
{
    public class TextFilter : ObservableObject, IFilter
    {
        private string _text;

        public string Text
        {
            get => _text;
            set
            {
                if (Set(nameof(Text), ref _text, value))
                    OnFilterChanged();
            }
        }

        public event EventHandler FilterInvalidated;

        public bool IsActive()
        {
            return !string.IsNullOrWhiteSpace(Text);
        }

        public void Reset()
        {
            Text = string.Empty;
        }

        public IEnumerable<T> Apply<T>(IEnumerable<T> locals, Func<T, string> selector)
        {
            var filtered = locals;

            if (IsActive())
                filtered = filtered.Where(d => CultureInfo.CurrentCulture.CompareInfo.IndexOf(selector(d), Text, CompareOptions.IgnoreCase) >= 0);

            return filtered;
        }

        protected virtual void OnFilterChanged()
        {
            FilterInvalidated?.Invoke(this, EventArgs.Empty);
        }
    }
}