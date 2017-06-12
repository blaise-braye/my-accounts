using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Operations.Classification.WpfUi.Technical.Controls
{
    public class SetupDataGridContextMenuBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ContextMenuOpening += OnContextMenuOpening;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ContextMenuOpening -= OnContextMenuOpening;
            base.OnDetaching();
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (AssociatedObject.ContextMenu == null)
            {
                e.Handled = true;
                return;
            }

            var clickedRow = MyVisualTreeHelper.GetVisualParent<DataGridRow>(e.OriginalSource);

            if (clickedRow == null)
            {
                // until I need to handle more than rows (headers...? cells...?)
                e.Handled = true;
                return;
            }

            var contextMenuItems = AssociatedObject.ContextMenu.Items.Cast<MenuItem>();
            SetupItems(contextMenuItems, clickedRow);
        }

        private void SetupItems(IEnumerable<MenuItem> menuItems, DataGridRow clickedRow)
        {
            foreach (var menuItem in menuItems)
            {
                menuItem.CommandParameter = clickedRow.DataContext;
                SetupItems(menuItem.Items.Cast<MenuItem>(), clickedRow);
            }
        }
    }
}