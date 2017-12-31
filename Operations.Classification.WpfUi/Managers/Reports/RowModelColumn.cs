using System.Windows;
using System.Windows.Controls;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class RowModelColumn : DataGridBoundColumn
    {
        protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
        }
        
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateElement(cell, dataItem);
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var cc = new ContentControl
            {
                DataContext = dataItem
            };

            var header = cell.Column.Header;

            cc.SetBinding(ContentControl.ContentProperty, $"[{header}]");
            return cc;
        }
    }
}