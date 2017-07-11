using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Operations.Classification.WpfUi.Technical.Controls
{
    public static class MyVisualTreeHelper
    {
        public static T GetVisualParent<T>(object childObject)
            where T : Visual
        {
            var child = childObject as DependencyObject;
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }

            return child as T;
        }

        public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject dependencyObject)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                yield return VisualTreeHelper.GetChild(dependencyObject, i);
            }
        }

        public static IEnumerable<DependencyObject> EnumerateVisualDescendents(this DependencyObject dependencyObject)
        {
            yield return dependencyObject;

            foreach (DependencyObject child in dependencyObject.EnumerateVisualChildren())
            {
                foreach (DependencyObject descendent in child.EnumerateVisualDescendents())
                {
                    yield return descendent;
                }
            }
        }

        public static void UpdateBindingTargets(this DependencyObject dependencyObject)
        {
            foreach (DependencyObject element in dependencyObject.EnumerateVisualDescendents())
            {
                LocalValueEnumerator localValueEnumerator = element.GetLocalValueEnumerator();
                while (localValueEnumerator.MoveNext())
                {
                    BindingExpressionBase bindingExpression = BindingOperations.GetBindingExpressionBase(element, localValueEnumerator.Current.Property);
                    bindingExpression?.UpdateTarget();
                }

                var itemsControl = element as ItemsControl;
                itemsControl?.Items.Refresh();
            }
        }

        public static void UpdateBindingTargets(this Application application)
        {
            foreach (Window window in application.Windows)
            {
                window.UpdateBindingTargets();
            }
        }
    }
}