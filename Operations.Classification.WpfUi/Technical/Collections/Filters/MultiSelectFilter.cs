using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Operations.Classification.WpfUi.Technical.Controls;

namespace Operations.Classification.WpfUi.Technical.Collections.Filters
{
    public class MultiSelectFilter : ObservableObject, IFilter
    {
        private bool _applying;
        private MenuItemViewModel[] _items;
        private object[] _selectedData;
        private Func<object, Func<object, bool>> _dataFilterBuilder;

        public event EventHandler FilterInvalidated;

        public MenuItemViewModel[] Items
        {
            get => _items ?? (_items = new MenuItemViewModel[0]);
            private set { Set(() => Items, ref _items, value); }
        }

        public object[] SelectedData
        {
            get => _selectedData ?? (_selectedData = Items.Where(i => i.IsChecked).Select(i => i.CommandParameter).ToArray());
            private set { Set(() => SelectedData, ref _selectedData, value); }
        }

        public void Initialize<TData>(
            IEnumerable<TData> source, 
            Func<TData, string> labelBuilder, 
            Func<TData, object> dataProvider = null,
            Func<object, Func<object, bool>> dataFilterBuilder = null)
        {
            var cmd = new RelayCommand<object>(RefreshFilterState);

            var allItem = new MenuItemViewModel
            {
                StaysOpenOnClick = true,
                Command = cmd,
                CommandParameter = null,
                Header = "all",
                IsCheckable = true,
                IsChecked = true,
            };

            var items = new[] { allItem }
                .Union(source.Select(data =>
                {
                    var item = new MenuItemViewModel
                    {
                        StaysOpenOnClick = true,
                        Command = cmd,
                        CommandParameter = dataProvider == null ? data : dataProvider(data),
                        Header = labelBuilder(data),
                        IsCheckable = true,
                        IsChecked = true
                    };

                    return item;
                }))
                .ToArray();

            Apply(() =>
            {
                _dataFilterBuilder = dataFilterBuilder;
                Items = items;
                SelectedData = null;
            });
        }

        public bool IsActive()
        {
            return !GetAllItem().IsChecked;
        }

        public void Reset()
        {
            GetAllItem().IsChecked = true;
            RefreshFilterState(GetAllItem().CommandParameter);
        }

        public IEnumerable<TData> Apply<TData>(IEnumerable<TData> source)
        {
            var filtered = source;
            if (IsActive())
            {
                if (_dataFilterBuilder == null)
                {
                    var selectedData = new HashSet<object>(GetExpandedSelectedData());
                    filtered = filtered.Where(d => selectedData.Contains(d));
                }
                else
                {
                    var filters = GetExpandedSelectedData().Select(_dataFilterBuilder);
                    filtered = filtered.Where(item => filters.Any(f => f(item)));
                }
            }

            return filtered;
        }

        public IEnumerable<TData> Apply<TData, TMember>(IEnumerable<TData> source, Expression<Func<TData, TMember>> selector)
        {
            var filtered = source;
            if (IsActive())
            {
                if (_dataFilterBuilder != null)
                {
                    throw new NotSupportedException();
                }

                if (GetExpandedSelectedData().Any())
                {
                    string propertyName = GetPropertyName(selector);

                    var selectedDataType = GetExpandedSelectedData().First().GetType();
                    var propertyAccessor = FastMember.TypeAccessor.Create(selectedDataType);

                    var memberCollection = GetExpandedSelectedData().Select(d => propertyAccessor[d, propertyName]);

                    var selectedData = new HashSet<object>(memberCollection);
                    var compiledSelector = selector.Compile();
                    filtered = filtered.Where(d => selectedData.Contains(compiledSelector(d)));
                }
                else
                {
                    filtered = filtered.Where(d => false);
                }
            }

            return filtered;
        }

        private static string GetPropertyName<TData, TMember>(Expression<Func<TData, TMember>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            MemberExpression body = propertyExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException(@"Invalid argument", nameof(propertyExpression));
            }

            PropertyInfo member = body.Member as PropertyInfo;
            if (member == null)
            {
                throw new ArgumentException(@"Argument is not a property", nameof(propertyExpression));
            }

            return member.Name;
        }

        private IEnumerable<object> GetExpandedSelectedData()
        {
            foreach (var data in SelectedData)
            {
                if (data is IList datalist)
                {
                    foreach (var datalistItem in datalist)
                    {
                        yield return datalistItem;
                    }
                }
                else
                {
                    yield return data;
                }
            }
        }

        private void Apply(Action action)
        {
            var isapplying = _applying;
            try
            {
                if (!isapplying)
                {
                    _applying = true;
                }

                action();
            }
            finally
            {
                if (!isapplying)
                {
                    _applying = false;
                }
            }

            if (!isapplying)
            {
                FilterInvalidated?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RefreshFilterState(object changedFilter)
        {
            Apply(() =>
            {
                var allItem = GetAllItem();

                if (Equals(allItem.CommandParameter, changedFilter))
                {
                    // check all
                    foreach (var item2 in Items)
                    {
                        item2.IsChecked = allItem.IsChecked;
                    }
                }
                else
                {
                    var allValues = Items.Select(i => i.CommandParameter).ToList();
                    var filteredValues = Items.Where(i => i.IsChecked).Select(i => i.CommandParameter).ToList();
                    if (allValues.Count == filteredValues.Count)
                    {
                        foreach (var item2 in Items)
                        {
                            item2.IsChecked = true;
                        }
                    }
                    else
                    {
                        allItem.IsChecked = false;
                    }
                }

                SelectedData = null;
            });
        }

        private MenuItemViewModel GetAllItem()
        {
            return Items[0];
        }
    }
}