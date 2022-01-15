﻿using System;
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
            if (!IsMouseUnderElement(itemsHost, Mouse.GetPosition))
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
                    _dropInfo = null;
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
            _dropTargetAdorner?.Detatch();
            _dropTargetAdorner = null;

            var sourceData = e.Data.GetData("Source") as Array;
            var itemsControl = sender as ItemsControl;
            var insertIndex = GetInsertIndex(itemsControl, out int firstItemOffset);
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
            var control = (ItemsControl) sender;
            _dropInfo = new DropInfo(e);

            var itemsHost = GetItemsHost(control);
            var bounds = Helpers.GetBoundsOfElement(itemsHost);
            var mousePos = _dropInfo._currentEventArgs.GetPosition(itemsHost);
            if (!bounds.Contains(mousePos))
            {
                var headerHost = GetHeaderHost(control);
                var flag = false;
                if (headerHost != null)
                {
                    bounds = Helpers.GetBoundsOfElement(headerHost);
                    mousePos = _dropInfo._currentEventArgs.GetPosition(headerHost);
                    flag = bounds.Contains(mousePos);
                }

                if (!flag)
                {
                    // CheckScroll(control, e);
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }
            }

            if (_dropTargetAdorner == null)
            {
                var adornedElement = GetItemsHost(control);
                if (adornedElement != null)
                    _dropTargetAdorner = new DropTargetInsertionAdorner(control);
            }

            if (_dropTargetAdorner != null)
                _dropTargetAdorner.InvalidateVisual();

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
        private static DropTargetAdorner _dropTargetAdorner;

        public static void DropTarget_OnPreviewDragLeave(object sender, DragEventArgs e)
        {
            if (_dropTargetAdorner != null)
            {
                _dropTargetAdorner.Detatch();
                _dropTargetAdorner = null;
            }
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

        public static int GetInsertIndex(ItemsControl control, out int firstItemOffset)
        {
            var orientation = GetItemsPanelOrientation(control);
            var panel = GetItemsHost(control);
            firstItemOffset = panel.Children.Count == 0 ? 0 : control.ItemContainerGenerator.IndexFromContainer(panel.Children[0]);
            for (var i = 0; i < panel.Children.Count; i++)
            {
                var item = panel.Children[i] as FrameworkElement;
                var itemBounds = item.GetBoundsOfElement();
                var mousePos = _dropInfo._currentEventArgs.GetPosition(item);
                if (itemBounds.Contains(mousePos))
                {
                    if (orientation == Orientation.Vertical)
                        return i + (mousePos.Y <= itemBounds.Bottom / 2 ? 0 : 1);
                    return i + (mousePos.X <= itemBounds.Right / 2 ? 0 : 1);
                }
            }

            var panelBounds = panel.GetBoundsOfElement();
            var mousePanelPos = _dropInfo._currentEventArgs.GetPosition(panel);
            Debug.Print($"Index2: {(panelBounds.Contains(mousePanelPos) ? panel.Children.Count : 0)}");
            return panelBounds.Contains(mousePanelPos) ? panel.Children.Count : 0;
        }

        private static bool IsMouseUnderElement(IInputElement element, Func<IInputElement, Point> getMousePosition)
        {
            var p = Mouse.GetPosition(element);
            var bounds = VisualTreeHelper.GetDescendantBounds((Visual)element);
            return bounds.Contains(p);
        }

        //==============================
        //==============================
        //==============================
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

        /*private static object GetItemUnderMouse(ItemsControl itemsControl, Point mousePosition)
        {
            var hitTestResult = VisualTreeHelper.HitTest(itemsControl, mousePosition);
            var itemsUnderMouse = Helpers.GetVisualParents(hitTestResult.VisualHit);
            if (itemsControl is DataGrid)
            {
                var item = itemsUnderMouse.OfType<DataGridRow>().FirstOrDefault();
                return item?.DataContext;
            }

            if (itemsControl is TabControl)
            {
                var item = itemsUnderMouse.OfType<TabItem>().FirstOrDefault();
                return item;
            }

            throw new Exception($"Trap! {itemsControl.GetType().Name} is not supported in GetItemUnderMouse");
        }*/

        /*delegate Point GetPositionDelegate(IInputElement element);

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
}*/

        /*private static int? MouseOverTargetOffset(Visual target, GetPositionDelegate getPosition, out bool isUp)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(target);
            var mousePos = getPosition((IInputElement) target);
            isUp = false;
            if (bounds.Contains(mousePos))
                return mousePos.Y < bounds.Top + bounds.Height / 2 ? 0 : 1;

            isUp = mousePos.Y < 0;
            return null;
        }*/


    }
}
