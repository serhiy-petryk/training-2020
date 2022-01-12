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
using System.Windows.Media;

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

        internal class DropInfo
        {
            private DragEventArgs _previousEventArgs;
            internal DragEventArgs _currentEventArgs;

            public DropInfo(DragEventArgs args)
            {
                _currentEventArgs = args;
            }

            public void Update(DragEventArgs args)
            {
                _previousEventArgs = _currentEventArgs;
                _currentEventArgs = args;
            }

        }

        private static DragInfo _dragInfo;
        internal static DropInfo _dropInfo;
        private static int cnt;
        private static bool _isDragging;

        public static void DragSource_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Debug.Print($"DragSource_OnPreviewMouseMove: {cnt++}");
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

            /*var itemsHost = GetItemsHost(itemsControl);
            var a1 = e.GetPosition(itemsHost);
            var isMouseUnderHost = Helpers.IsMouseOverTarget(itemsHost, e.GetPosition);
            if (!isMouseUnderHost)
            {
                _dragInfo = null;
                return;
            }*/
            if (!IsMouseUnderHost(itemsControl, mousePosition))
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

                Debug.Print($"DragStart: {cnt++}, {GetSelectedItems(itemsControl).Count}");
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
                }

                //adLayer.Remove(myAdornment);
                e.Handled = true;
            }
        }

        private static IList GetSelectedItems(ItemsControl itemsControl)
        {
            if (itemsControl is DataGrid dataGrid)
            {
                return dataGrid.SelectedItems;
            }

            if (itemsControl is ListBox listBox)
                return listBox.SelectedItems;

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
            _dropTargetAdorner?.Detatch();
            _dropTargetAdorner = null;

            var sourceData = e.Data.GetData("Source") as Array;
            var itemsControl = sender as ItemsControl;
            var insertIndex = GetInsertIndex(itemsControl, e.GetPosition);
            var targetData = itemsControl.ItemsSource as IList;

            var mousePosition = e.GetPosition(itemsControl);
            var item = GetItemUnderMouse(itemsControl, mousePosition);


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
        }

        //======================================
        //======================================
        //======================================
        private static object GetItemUnderMouse(ItemsControl itemsControl, Point mousePosition)
        {
            var hitTestResult = VisualTreeHelper.HitTest(itemsControl, mousePosition);
            var itemsUnderMouse = Helpers.GetVisualParents(hitTestResult.VisualHit);
            if (itemsControl is DataGrid)
            {
                var row = itemsUnderMouse.OfType<DataGridRow>().FirstOrDefault();
                return row?.DataContext;
            }

            throw new Exception($"Trap! {itemsControl.GetType().Name} is not supported in GetItemUnderMouse");
        }

        private static bool IsMouseUnderHost(ItemsControl itemsControl, Point mousePosition)
        {
            var hitTestResult = VisualTreeHelper.HitTest(itemsControl, mousePosition);
            if (hitTestResult == null)
                return false;

            var itemsUnderMouse = Helpers.GetVisualParents(hitTestResult.VisualHit).ToArray();
            var itemsHost = GetItemsHost(itemsControl);
            return itemsUnderMouse.Contains(itemsHost);
        }

        delegate Point GetPositionDelegate(IInputElement element);

        private static int GetInsertIndex(ItemsControl control, GetPositionDelegate getPosition)
        {
            bool isUp = false;
            for (var i = 0; i < control.Items.Count; i++)
            {
                var item = (Visual) control.ItemContainerGenerator.ContainerFromIndex(i);
                if (item == null) continue;
                var offset = MouseOverTargetOffset(item, getPosition, out isUp);
                if (offset.HasValue)
                    return i + offset.Value;
            }

            return isUp ? 0 : control.Items.Count;
        }

        private static int? MouseOverTargetOffset(Visual target, GetPositionDelegate getPosition, out bool isUp)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(target);
            var mousePos = getPosition((IInputElement) target);
            isUp = false;
            if (bounds.Contains(mousePos))
                return mousePos.Y < bounds.Top + bounds.Height / 2 ? 0 : 1;

            isUp = mousePos.Y < 0;
            return null;
        }


        private static PropertyInfo _piItemsHost;

        private static Panel GetItemsHost(ItemsControl itemsControl)
        {
            if (_piItemsHost == null)
                _piItemsHost = typeof(ItemsControl).GetProperty("ItemsHost",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (Panel) _piItemsHost.GetValue(itemsControl);
        }

        /*public static void DragSource_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            return;
            var aa1 = Helpers.GetElementsUnderMouseClick((UIElement)sender, e);
            if (sender is ItemsControl control)
            {
                var itemsHost = GetItemsHost(control);
                var a1 = aa1.Contains(itemsHost);
                Debug.Print($"Hit: {a1}");
            }

            if (e.ClickCount == 1 && sender is ItemsControl itemsControl)
                _dragInfo = new DragInfo(itemsControl, e);
            else
                _dragInfo = null;
        }*/

        public static void DropTarget_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            var scrolls = ((ItemsControl) sender).GetVisualChildren().OfType<ScrollBar>();
            foreach (var sb in scrolls)
            {
                var bounds = VisualTreeHelper.GetDescendantBounds(sb);
                var p = e.GetPosition(sb);
                if (bounds.Contains(p))
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }
            }

            _dropInfo = new DropInfo(e);
            if (_dropTargetAdorner == null && sender is ItemsControl control)
            {
                var adornedElement = GetItemsHost(control);
                if (adornedElement != null)
                    _dropTargetAdorner = new DropTargetInsertionAdorner(adornedElement);
            }

            if (_dropTargetAdorner != null)
                _dropTargetAdorner.InvalidateVisual();

            CheckScroll((FrameworkElement) sender, e);
        }

        private static void CheckScroll(FrameworkElement o, DragEventArgs e)
        {
            var scrollViewer = o.GetVisualChildren().OfType<ScrollViewer>().FirstOrDefault();
            if (scrollViewer != null)
            {
                var position = e.GetPosition(scrollViewer);
                var scrollMargin = Math.Min(scrollViewer.FontSize * 2, scrollViewer.ActualHeight / 2);
                if (position.X >= scrollViewer.ActualWidth - scrollMargin && scrollViewer.HorizontalOffset < scrollViewer.ExtentWidth - scrollViewer.ViewportWidth)
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + scrollMargin);
                else if (position.X < scrollMargin && scrollViewer.HorizontalOffset > 0)
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - scrollMargin);
                else if (position.Y >= scrollViewer.ActualHeight - scrollMargin && scrollViewer.VerticalOffset < scrollViewer.ExtentHeight - scrollViewer.ViewportHeight)
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 1.0);
                else if (position.Y < scrollMargin && scrollViewer.VerticalOffset > 0)
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 1.0);
                /*var tolerance = Math.Min(scrollViewer.FontSize * 2, scrollViewer.ActualHeight / 2);
                var verticalPos = e.GetPosition(scrollViewer).Y;
                var offset = 1.0;

                //Debug.Print($"Drag: {verticalPos}");
                if (verticalPos < tolerance) // Top of visible list?
                {
                    //Debug.Print($"ScrollUp");
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset); //Scroll up.
                }
                else if (verticalPos > o.ActualHeight - tolerance) //Bottom of visible list?
                {
                    //Debug.Print($"ScrollDown");
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset); //Scroll down.    
                }*/
            }
        }

        //============================
        private static DropTargetAdorner _dropTargetAdorner;

        public static void DropTarget_OnPreviewDragLeave(object sender, DragEventArgs e)
        {
            if (_dropTargetAdorner != null)
            {
                _dropTargetAdorner.Detatch();
                _dropTargetAdorner = null;
            }
        }
    }
}
