using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace ItemsControlDragDrop.Code
{
    public static class DragDropHelper
    {
        class DragInfo
        {
            public Point DragStart { get; }
            public ItemsControl DragSource { get; }
            public int SelectedItemCount { get; }

            public DragInfo(ItemsControl dragSource, MouseEventArgs e)
            {
                DragSource = dragSource;
                DragStart = e.GetPosition(dragSource);
                SelectedItemCount = GetSelectedItems(dragSource).Count;
            }
        }

        private static DropTargetInsertionAdorner _dropTargetAdorner;
        private static DragAdorner _dragAdorner;
        internal static DragEventArgs _lastDragEventArgs;
        private static DragInfo _dragInfo;
        private static bool _isDragging;

        public static void DragSource_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging) return;
            if (!(sender is ItemsControl itemsControl) || e.LeftButton == MouseButtonState.Released ||
                GetSelectedItems(itemsControl).Count == 0)
            {
                _dragInfo = null;
                return;
            }

            var mousePosition = e.GetPosition(itemsControl);
            var selectedItems = GetSelectedItems(itemsControl);
            /*var item = GetItemUnderMouse(itemsControl, mousePosition);
            if (!selectedItems.Contains(item))
            {
                _dragInfo = null;
                return;
            }*/

            var itemsHost = GetItemsHost(itemsControl);
            if (!itemsHost.IsMouseOverElement(e.GetPosition))
            {
                _dragInfo = null;
                return;
            }

            if (_dragInfo == null)
            {
                _dragInfo = new DragInfo(itemsControl, e);
                return;
            }

            /*if (GetSelectedItems(itemsControl).Count != _dragInfo.SelectedItemCount)
            {
                _dragInfo = null;
                return;
            }*/

            if (Math.Abs(mousePosition.X - _dragInfo.DragStart.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(mousePosition.Y - _dragInfo.DragStart.Y) > SystemParameters.MinimumVerticalDragDistance)
            {

                var dataObject = new DataObject();
                dataObject.SetData("Source", selectedItems.OfType<object>().ToArray());
                //var adLayer = AdornerLayer.GetAdornerLayer(item);
                // myAdornment = new DraggableAdorner(item);
                //adLayer.Add(myAdornment);
                // Debug.Print($"DragDrop.DoDragDrop");
                try
                {
                    _isDragging = true;
                    var result = DragDrop.DoDragDrop(itemsControl, dataObject, DragDropEffects.Copy);
                }
                finally
                {
                    _isDragging = false;
                    _dragInfo = null;
                    _lastDragEventArgs = null;
                }

                //adLayer.Remove(myAdornment);
                e.Handled = true;
            }
        }

        private static IList GetSelectedItems(ItemsControl itemsControl)
        {
            if (itemsControl is MultiSelector multiSelector)
                return multiSelector.SelectedItems;

            if (itemsControl is Selector selector)
            {
                if (selector.SelectedItem != null)
                    return new List<object> { selector.SelectedItem };
                return new List<object>();
            }

            if (itemsControl is TreeView treeView)
            {
                if (treeView.SelectedItem != null)
                    return new List<object> {treeView.SelectedItem};
                return new List<object>();
            }

            throw new Exception($"Trap! {itemsControl.GetType().Name} is not supported");
        }

        public static void DropTarget_OnPreviewDrop(object sender, DragEventArgs e)
        {
            var sourceData = e.Data.GetData("Source") as Array;
            var itemsControl = sender as ItemsControl;
            var insertIndex = GetInsertIndex(itemsControl, e, out int firstItemOffset);
            insertIndex += firstItemOffset;

            var targetData = (IList) itemsControl.ItemsSource ?? itemsControl.Items;
            if (e.Effects == DragDropEffects.Copy)
            {
                foreach (var o in sourceData)
                {
                    var index = targetData.IndexOf(o);
                    if (index != -1)
                    {
                        targetData.RemoveAt(index);
                        if (index < insertIndex) --insertIndex;
                    }
                }

                foreach (var o in sourceData)
                    targetData.Insert(insertIndex++, o);
            }
            ResetDragDrop(e);
        }

        private static PropertyInfo _piItemsHost;
        public static Panel GetItemsHost(ItemsControl itemsControl)
        {
            if (_piItemsHost == null)
                _piItemsHost = typeof(ItemsControl).GetProperty("ItemsHost", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (Panel)_piItemsHost.GetValue(itemsControl);
        }

        private static PropertyInfo _piDataGridHeaderHost;
        public static FrameworkElement GetHeaderHost(ItemsControl itemsControl)
        {
            if (itemsControl is DataGrid)
            {
                if (_piDataGridHeaderHost == null)
                    _piDataGridHeaderHost = typeof(DataGrid).GetProperty("ColumnHeadersPresenter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return (DataGridColumnHeadersPresenter) _piDataGridHeaderHost.GetValue(itemsControl);
            }
            return null;
        }

        public static void DropTarget_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            _lastDragLeaveObject = null;
            _lastDragEventArgs = e;
            var control = (ItemsControl) sender;

            var itemsHost = GetItemsHost(control);
            if (!itemsHost.IsMouseOverElement(e.GetPosition))
            {
                var headerHost = GetHeaderHost(control);
                var flag = false;
                if (headerHost != null)
                    flag = headerHost.IsMouseOverElement(e.GetPosition);

                if (!flag)
                {
                    // CheckScroll(control, e);
                    Debug.Print($"ResetDragDrop: {control.Name}");
                    ResetDragDrop(e);
                    return;
                }
            }

            if (_dropTargetAdorner == null)
                _dropTargetAdorner = new DropTargetInsertionAdorner(control);
            _dropTargetAdorner.InvalidateVisual();

            if (_dragAdorner == null)
            {
                var rootElement = Window.GetWindow(control).Content as UIElement;
                _dragAdorner = new DragAdorner(rootElement, e.Data.GetData("Source"));
            }

            _dragAdorner.UpdateUI(e, control);

            CheckScroll(control, e);
        }

        private static void CheckScroll(ItemsControl o, DragEventArgs e)
        {
            var scrollViewer = o.GetVisualChildren().OfType<ScrollViewer>().FirstOrDefault();
            if (scrollViewer != null)
            {
                if (scrollViewer.CanContentScroll)
                    scrollViewer.CanContentScroll = false;
                const double scrollMargin = 25.0;
                const double scrollStep = 8.0;
                var position = e.GetPosition(scrollViewer);
                if (position.X >= scrollViewer.ActualWidth - scrollMargin && scrollViewer.HorizontalOffset <
                    scrollViewer.ExtentWidth - scrollViewer.ViewportWidth)
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + scrollStep);
                else if (position.X < scrollMargin && scrollViewer.HorizontalOffset > 0)
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - scrollStep);
                else if (position.Y >= scrollViewer.ActualHeight - scrollMargin && scrollViewer.VerticalOffset <
                         scrollViewer.ExtentHeight - scrollViewer.ViewportHeight)
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollStep);
                else if (position.Y < scrollMargin && scrollViewer.VerticalOffset > 0)
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - scrollStep);
            }
        }

        //============================
        private static object _lastDragLeaveObject;
        public static void DropTarget_OnPreviewDragLeave(object sender, DragEventArgs e)
        {
            _lastDragLeaveObject = sender;
            ((FrameworkElement) sender).Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Equals(_lastDragLeaveObject, sender))
                    ResetDragDrop(e);
                _lastDragLeaveObject = null;
            }), DispatcherPriority.Normal);
        }

        public static Orientation GetItemsPanelOrientation(ItemsControl itemsControl)
        {
            if (itemsControl is TabControl)
            {
                var tabControl = (TabControl) itemsControl;
                return tabControl.TabStripPlacement == Dock.Left || tabControl.TabStripPlacement == Dock.Right
                    ? Orientation.Vertical
                    : Orientation.Horizontal;
            }

            var panel = GetItemsHost(itemsControl);
            var orientationProperty = panel.GetType().GetProperty("Orientation", typeof(Orientation));

            if (orientationProperty != null)
                return (Orientation) orientationProperty.GetValue(panel, null);

            throw new Exception("Trap! Can't define item panel orientation");
        }

        public static int GetInsertIndex(ItemsControl control, DragEventArgs e, out int firstItemOffset)
        {
            var orientation = GetItemsPanelOrientation(control);
            var panel = GetItemsHost(control);
            firstItemOffset = panel.Children.Count == 0 ? 0 : control.ItemContainerGenerator.IndexFromContainer(panel.Children[0]);
            for (var i = 0; i < panel.Children.Count; i++)
            {
                var item = panel.Children[i] as FrameworkElement;
                if (item.IsMouseOverElement(e.GetPosition))
                {
                    var itemBounds = item.GetBoundsOfElement();
                    var mousePos = e.GetPosition(item);
                    if (orientation == Orientation.Vertical)
                        return i + (mousePos.Y <= itemBounds.Bottom * 0.8 ? 0 : 1);
                    return i + (mousePos.X <= itemBounds.Right * 0.8 ? 0 : 1);
                }
            }
            return panel.IsMouseOverElement(e.GetPosition) ? panel.Children.Count : 0;
        }

        private static void ResetDragDrop(DragEventArgs e)
        {
            if (_dropTargetAdorner != null)
            {
                _dropTargetAdorner.Detach();
                _dropTargetAdorner = null;
            }
            if (_dragAdorner != null)
            {
                _dragAdorner.Detach();
                _dragAdorner = null;
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
    }
}
