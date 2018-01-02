using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class DataGridRowModel
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached("ItemsSource", typeof(IEnumerable<RowModel>), typeof(DataGridRowModel), new PropertyMetadata(default(IEnumerable<RowModel>), ItemsSourcePropertyChanged));
        public static readonly DependencyProperty SortMemberPathProperty = DependencyProperty.RegisterAttached("SortMemberPath", typeof(object), typeof(DataGridRowModel), new PropertyMetadata(default(object), SortMemberPathPropertyChanged));
        public static readonly DependencyProperty CellTemplateSelectorProperty = DependencyProperty.RegisterAttached("CellTemplateSelector", typeof(DataTemplateSelector), typeof(DataGridRowModel), new PropertyMetadata(default(DataTemplateSelector)));

        public static IEnumerable<RowModel> GetItemsSource(UIElement element)
        {
            return (IEnumerable<RowModel>)element.GetValue(ItemsSourceProperty);
        }

        public static void SetItemsSource(UIElement element, IEnumerable<RowModel> value)
        {
            element.SetValue(ItemsSourceProperty, value);
        }
        
        public static string GetSortMemberPath(UIElement element)
        {
            return (string)element.GetValue(SortMemberPathProperty);
        }

        public static void SetSortMemberPath(UIElement element, string value)
        {
            element.SetValue(SortMemberPathProperty, value);
        }

        public static DataTemplateSelector GetCellTemplateSelector(DataGrid element)
        {
            return (DataTemplateSelector)element.GetValue(CellTemplateSelectorProperty);
        }

        public static void SetCellTemplateSelector(DataGrid element, DataTemplateSelector value)
        {
            element.SetValue(CellTemplateSelectorProperty, value);
        }

        private static void SortMemberPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (DataGrid)d;
            var existingCols = grid.Columns.OfType<IndexedDataGridTemplateColumn>().ToList();
            existingCols.ForEach(col => col.SetIndexedSortMemberPath(e.NewValue?.ToString()));
        }

        private static void ItemsSourcePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var grid = (DataGrid)dependencyObject;
            var itemsSource = (IList<RowModel>)eventArgs.NewValue;

            grid.AutoGenerateColumns = false;
            grid.ItemsSource = null;
            var existingCols = grid.Columns.OfType<IndexedDataGridTemplateColumn>().ToList();
            existingCols.ForEach(c => grid.Columns.Remove(c));
            
            if (itemsSource?.Any() == true)
            {
                var elt = itemsSource[0];
                foreach (var cell in elt.Cells)
                {
                    grid.Columns.Add(new IndexedDataGridTemplateColumn(cell.Title, GetCellTemplateSelector(grid), GetSortMemberPath(grid)));
                }
            }
            
            grid.ItemsSource = itemsSource;
        }
    }
}