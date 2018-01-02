using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class IndexedDataGridTemplateColumn : DataGridTemplateColumn
    {
        public IndexedDataGridTemplateColumn(string header, DataTemplateSelector templateSelector, string sortMemberPath)
        {
            Header = header;
            CellTemplateSelector = templateSelector;
            SetIndexedSortMemberPath(sortMemberPath);
        }

        public void SetIndexedSortMemberPath(string sortMemberPath)
        {
            SortMemberPath = $"[{Header}].{sortMemberPath}";
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var elt = base.GenerateEditingElement(cell, dataItem);
            SetBindingFromIndexedHeader(elt);
            return elt;
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var elt = base.GenerateElement(cell, dataItem);
            SetBindingFromIndexedHeader(elt);
            
            return elt;
        }

        private void SetBindingFromIndexedHeader(DependencyObject elt)
        {
            BindingOperations.SetBinding(elt, ContentPresenter.ContentProperty, new Binding($"[{Header}]"));
        }
    }
}