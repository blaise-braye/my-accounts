using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Operations.Classification.WpfUi.Technical.Controls
{
    public class ContextMenuBehavior : Behavior<ButtonBase>
    {
        public static readonly DependencyProperty DisplayOnClickProperty = DependencyProperty.Register(
            "DisplayOnClick",
            typeof(bool),
            typeof(ContextMenuBehavior),
            new PropertyMetadata(default(bool)));

        public bool DisplayOnClick
        {
            get { return (bool)GetValue(DisplayOnClickProperty); }
            set { SetValue(DisplayOnClickProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= OnClick;
            base.OnDetaching();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (DisplayOnClick)
            {
                var menu = ContextMenuService.GetContextMenu(AssociatedObject);
                if (menu != null)
                {
                    menu.PlacementTarget = AssociatedObject;
                    menu.Placement = ContextMenuService.GetPlacement(AssociatedObject);
                    menu.IsOpen = true;
                }
            }
        }
    }
}