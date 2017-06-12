using System.Windows;
using System.Windows.Media;

namespace Operations.Classification.WpfUi.Technical.Controls
{
    public class MyVisualTreeHelper
    {
        public static T GetVisualParent<T>(object childObject) where T : Visual
        {
            var child = childObject as DependencyObject;
            while (child != null && !(child is T))
                child = VisualTreeHelper.GetParent(child);
            return child as T;
        }
    }
}