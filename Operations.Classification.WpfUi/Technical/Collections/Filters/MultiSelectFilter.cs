using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GalaSoft.MvvmLight.CommandWpf;
using Operations.Classification.WpfUi.Technical.Controls;

namespace Operations.Classification.WpfUi.Technical.Collections.Filters
{
    public class MultiSelectFilter : CompositeFilterBase
    {
        private MenuItemViewModel[] _items;
        private MenuItemViewModel[] _dataitems;
        private object[] _selectedData;
        
        public MenuItemViewModel[] Items
        {
            get => _items ?? (_items = new MenuItemViewModel[0]);
            private set { Set(() => Items, ref _items, value); }
        }

        public MenuItemViewModel[] DataItems
        {
            get => _dataitems ?? (_dataitems = new MenuItemViewModel[0]);
            private set { Set(() => DataItems, ref _dataitems, value); }
        }

        public object[] SelectedData
        {
            get => _selectedData ?? (_selectedData = DataItems.Where(i => i.IsChecked).Select(i => i.CommandParameter).ToArray());
            private set { Set(() => SelectedData, ref _selectedData, value); }
        }
        
        public void Initialize<TSource>(
            IEnumerable<TSource> source, 
            Func<TSource, string> labelBuilder, 
            Func<TSource, object> dataProvider = null)
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

            var dataItems = source.Select(data =>
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
            }).ToArray();

            var items = new[] { allItem }.Union(dataItems).ToArray();

            Apply(() =>
            {
                Items = items;
                DataItems = dataItems;
                SelectedData = null;
            });
        }

        public override bool IsActive()
        {
            return !GetAllItem().IsChecked;
        }

        public override void Reset()
        {
            GetAllItem().IsChecked = true;
            RefreshFilterState(GetAllItem().CommandParameter);
        }

        public IEnumerable<TData> Apply<TData>(IEnumerable<TData> source)
        {
            var filtered = Apply<TData, object>(source, null);
            return filtered;
        }

        public MenuItemViewModel GetAllItem()
        {
            return Items[0];
        }

        public IEnumerable<TData> Apply<TData, TMember>(
            IEnumerable<TData> source, 
            Expression<Func<TData, TMember>> selector)
        {
            var filtered = source;
            if (IsActive())
            {
                var expandedSelectedData = GetExpandedSelectedData().ToList();
                if (expandedSelectedData.Any())
                {
                    var selectedDataType = expandedSelectedData[0].GetType();
                    if (selector != null)
                    {
                        if (typeof(TMember).IsAssignableFrom(selectedDataType))
                        {
                            var selectedData = new HashSet<object>(expandedSelectedData);
                            var compiledSelector = selector.Compile();
                            filtered = filtered.Where(d => selectedData.Contains(compiledSelector(d)));
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                    else if (typeof(TMember).IsAssignableFrom(selectedDataType))
                    {
                        var selectedData = new HashSet<object>(expandedSelectedData);
                        filtered = filtered.Where(d => selectedData.Contains(d));
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
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

            var body = propertyExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException(@"Invalid argument", nameof(propertyExpression));
            }

            var member = body.Member as PropertyInfo;
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
                    var allValues = DataItems.Select(i => i.CommandParameter).ToList();
                    var filteredValues = DataItems.Where(i => i.IsChecked).Select(i => i.CommandParameter).ToList();
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
    }
}