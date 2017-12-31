using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class DataGridRowModel
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached("ItemsSource", typeof(IEnumerable<RowModel>), typeof(DataGridRowModel), new PropertyMetadata(default(IEnumerable<RowModel>), PropertyChangedCallback));

        public static IEnumerable<RowModel> GetItemsSource(UIElement element)
        {
            return (IEnumerable<RowModel>) element.GetValue(ItemsSourceProperty);
        }

        public static void SetItemsSource(UIElement element, IEnumerable<RowModel> value)
        {
            element.SetValue(ItemsSourceProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var grid = (DataGrid)dependencyObject;
            var itemsSource = (IList<RowModel>)eventArgs.NewValue;

            grid.AutoGenerateColumns = false;
            grid.ItemsSource = null;
            grid.Columns.Clear();

            if (itemsSource?.Any() == true)
            {
                var elt = itemsSource[0];
                foreach (var cell in elt.Cells)
                {
                    grid.Columns.Add(new RowModelColumn
                    {
                        Header = cell.Title,
                    });
                }
            }
            
            grid.ItemsSource = itemsSource;
        }
    }
}