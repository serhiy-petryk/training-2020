using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ItemsControlDragDrop.Code
{
    /// <summary>
    /// Provides extended support for the ListBox and ListView controls.
    /// </summary>
    public static class ListBoxExtension
    {
        #region Attached Property Declaration

        /// <summary>
        /// Identifies the ListBoxExtension.SelectedItemsSource attached property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemsSourceProperty =
           DependencyProperty.RegisterAttached(
              "SelectedItemsSource",
              typeof(IList),
              typeof(ListBoxExtension),
              new PropertyMetadata(
                  null,
                  new PropertyChangedCallback(OnSelectedItemsSourceChanged)));

        #endregion

        #region Attached Property Accessors

        /// <summary>
        /// Gets the IList that contains the values that should be selected.
        /// </summary>
        /// <param name="element">The ListBox to check.</param>
        public static IList GetSelectedItemsSource(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (IList)element.GetValue(SelectedItemsSourceProperty);
        }

        /// <summary>
        /// Sets the IList that contains the values that should be selected.
        /// </summary>
        /// <param name="element">The ListBox being set.</param>
        /// <param name="value">The value of the property.</param>
        public static void SetSelectedItemsSource(DependencyObject element, IList value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(SelectedItemsSourceProperty, value);
        }

        #endregion

        #region IsResynchingProperty

        // Used to set a flag on the ListBox to avoid reentry of SelectionChanged due to
        // a full syncronisation pass
        private static readonly DependencyPropertyKey IsResynchingPropertyKey =
           DependencyProperty.RegisterAttachedReadOnly(
                "IsResynching",
                typeof(bool),
                typeof(ListBoxExtension),
                new PropertyMetadata(false));

        #endregion

        #region Private Static Methods

        private static void OnSelectedItemsSourceChanged(DependencyObject d,
                            DependencyPropertyChangedEventArgs e)
        {
            ListBox listBox = d as ListBox;
            if (listBox == null)
            {
                throw new InvalidOperationException("The ListBoxExtension.SelectedItemsSource attached " +
                  "property can only be applied to ListBox controls.");
            }

            listBox.SelectionChanged -= new SelectionChangedEventHandler(OnListBoxSelectionChanged);

            if (e.NewValue != null)
            {
                ListenForChanges(listBox);
            }
        }

        private static void ListenForChanges(ListBox listBox)
        {
            // Wait until the element is initialised
            if (!listBox.IsInitialized)
            {
                EventHandler callback = null;
                callback = delegate
                {
                    listBox.Initialized -= callback;
                    ListenForChanges(listBox);
                };
                listBox.Initialized += callback;
                return;
            }

            listBox.SelectionChanged += new SelectionChangedEventHandler(OnListBoxSelectionChanged);
            ResynchList(listBox);
        }

        private static void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox != null)
            {
                bool isResynching = (bool)listBox.GetValue(IsResynchingPropertyKey.DependencyProperty);
                if (isResynching)
                {
                    return;
                }

                IList list = GetSelectedItemsSource(listBox);
                if (list != null)
                {
                    foreach (object obj in e.RemovedItems)
                    {
                        if (list.Contains(obj))
                        {
                            list.Remove(obj);
                        }
                    }

                    foreach (object obj in e.AddedItems)
                    {
                        if (!list.Contains(obj))
                        {
                            list.Add(obj);
                        }
                    }
                }
            }
        }

        private static void ResynchList(ListBox listBox)
        {
            ////DebugLogger.WriteLine("ListBoxExtension", "ResynchList");
            if (listBox != null)
            {
                listBox.SetValue(IsResynchingPropertyKey, true);
                IList list = GetSelectedItemsSource(listBox);

                if (listBox.SelectionMode == SelectionMode.Single)
                {
                    listBox.SelectedItem = null;
                    if (list != null)
                    {
                        if (list.Count > 1)
                        {
                            // There is more than one item selected, but the listbox is in Single selection mode
                            throw new InvalidOperationException("ListBox is in Single selection mode, but was given more than one selected value.");
                        }

                        if (list.Count == 1)
                        {
                            listBox.SelectedItem = list[0];
                        }
                    }
                }
                else
                {
                    listBox.SelectedItems.Clear();
                    if (list != null)
                    {
                        foreach (object obj in listBox.Items)
                        {
                            if (list.Contains(obj))
                            {
                                listBox.SelectedItems.Add(obj);
                            }
                        }
                    }
                }

                listBox.SetValue(IsResynchingPropertyKey, false);
            }
        }
        #endregion

        #region ScrollOnDragDropProperty

        public static readonly DependencyProperty ScrollOnDragDropProperty =
            DependencyProperty.RegisterAttached("ScrollOnDragDrop",
                typeof(bool),
                typeof(ListBoxExtension),
                new PropertyMetadata(false, HandleScrollOnDragDropChanged));

        public static bool GetScrollOnDragDrop(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (bool)element.GetValue(ScrollOnDragDropProperty);
        }

        public static void SetScrollOnDragDrop(DependencyObject element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(ScrollOnDragDropProperty, value);
        }

        private static void HandleScrollOnDragDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement container = d as FrameworkElement;

            if (d == null)
            {
                Debug.Fail("Invalid type!");
                return;
            }

            Unsubscribe(container);

            if (true.Equals(e.NewValue))
            {
                Subscribe(container);
            }
        }

        private static void Subscribe(FrameworkElement container)
        {
            container.PreviewDragOver += OnContainerPreviewDragOver;
        }

        private static void OnContainerPreviewDragOver(object sender, DragEventArgs e)
        {
            FrameworkElement container = sender as FrameworkElement;

            if (container == null)
            {
                return;
            }

            ScrollViewer scrollViewer = GetFirstVisualChild<ScrollViewer>(container);
            ;
            if (scrollViewer == null)
            {
                return;
            }

            double tolerance = 60;
            double verticalPos = e.GetPosition(container).Y;
            double offset = 20;

            if (verticalPos < tolerance) // Top of visible list? 
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset); //Scroll up. 
            }
            else if (verticalPos > container.ActualHeight - tolerance) //Bottom of visible list? 
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset); //Scroll down.     
            }
        }

        private static void Unsubscribe(FrameworkElement container)
        {
            container.PreviewDragOver -= OnContainerPreviewDragOver;
        }

        public static T GetFirstVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = GetFirstVisualChild<T>(child);
                    if (childItem != null)
                    {
                        return childItem;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}