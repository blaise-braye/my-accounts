using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Operations.Classification.WpfUi.Technical.Collections
{
    /// <summary>
    /// A sync behaviour for a multiselector.
    /// </summary>
    public static class MultiSelectorBehaviours
    {
        public static readonly DependencyProperty SynchronizedSelectedItems = DependencyProperty.RegisterAttached(
            "SynchronizedSelectedItems", typeof(IList), typeof(MultiSelectorBehaviours), new PropertyMetadata(null, OnSynchronizedSelectedItemsChanged));

        private static readonly DependencyProperty _synchronizationManagerProperty = DependencyProperty.RegisterAttached(
            "_synchronizationManager", typeof(SynchronizationManager), typeof(MultiSelectorBehaviours), new PropertyMetadata(null));

        /// <summary>
        /// Gets the synchronized selected items.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns>The list that is acting as the sync list.</returns>
        public static IList GetSynchronizedSelectedItems(DependencyObject dependencyObject)
        {
            return (IList)dependencyObject.GetValue(SynchronizedSelectedItems);
        }

        /// <summary>
        /// Sets the synchronized selected items.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="value">The value to be set as synchronized items.</param>
        public static void SetSynchronizedSelectedItems(DependencyObject dependencyObject, IList value)
        {
            dependencyObject.SetValue(SynchronizedSelectedItems, value);
        }

        private static SynchronizationManager Get_synchronizationManager(DependencyObject dependencyObject)
        {
            return (SynchronizationManager)dependencyObject.GetValue(_synchronizationManagerProperty);
        }

        private static void Set_synchronizationManager(DependencyObject dependencyObject, SynchronizationManager value)
        {
            dependencyObject.SetValue(_synchronizationManagerProperty, value);
        }

        private static void OnSynchronizedSelectedItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                SynchronizationManager synchronizer = Get_synchronizationManager(dependencyObject);
                synchronizer.StopSynchronizing();

                Set_synchronizationManager(dependencyObject, null);
            }

            // check that this property is an IList, and that it is being set on a ListBox
            if (e.NewValue is IList && dependencyObject is Selector selector)
            {
                SynchronizationManager synchronizer = Get_synchronizationManager(dependencyObject);
                if (synchronizer == null)
                {
                    synchronizer = new SynchronizationManager(selector);
                    Set_synchronizationManager(dependencyObject, synchronizer);
                }

                synchronizer.StartSynchronizingList();
            }
        }

        /// <summary>
        /// A synchronization manager.
        /// </summary>
        private class SynchronizationManager
        {
            private readonly Selector _multiSelector;
            private TwoListSynchronizer _synchronizer;

            /// <summary>
            /// Initializes a new instance of the <see cref="SynchronizationManager"/> class.
            /// </summary>
            /// <param name="selector">The selector.</param>
            internal SynchronizationManager(Selector selector)
            {
                _multiSelector = selector;
            }

            /// <summary>
            /// Starts synchronizing the list.
            /// </summary>
            public void StartSynchronizingList()
            {
                IList list = GetSynchronizedSelectedItems(_multiSelector);

                if (list != null)
                {
                    _synchronizer = new TwoListSynchronizer(GetSelectedItemsCollection(_multiSelector), list);
                    _synchronizer.StartSynchronizing();
                }
            }

            /// <summary>
            /// Stops synchronizing the list.
            /// </summary>
            public void StopSynchronizing()
            {
                _synchronizer.StopSynchronizing();
            }

            private static IList GetSelectedItemsCollection(Selector selector)
            {
                if (selector is MultiSelector multiSelector)
                {
                    return multiSelector.SelectedItems;
                }

                if (selector is ListBox box)
                {
                    return box.SelectedItems;
                }

                throw new InvalidOperationException("Target object has no SelectedItems property to bind.");
            }
        }
    }
}
