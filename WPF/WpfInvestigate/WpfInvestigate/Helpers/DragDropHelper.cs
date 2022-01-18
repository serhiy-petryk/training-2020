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
using WpfInvestigate.Common;

namespace WpfInvestigate.Helpers
{
    public static class DragDropHelper
    {
        #region ==============  Event handlers  ==============
        public static void DragSource_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging) return;
            if (!(sender is ItemsControl itemsControl) || e.LeftButton == MouseButtonState.Released ||
                GetSelectedItems(itemsControl).Count == 0)
            {
                _startDragInfo.Clear();
                return;
            }

            var itemsHost = GetItemsHost(itemsControl);
            if (!itemsHost.IsMouseOverElement(e.GetPosition))
            {
                _startDragInfo.Clear();
                return;
            }

            if (!Equals(itemsControl, _startDragInfo.DragSource))
            {
                _startDragInfo.Init(itemsControl, e);
                return;
            }

            var mousePosition = e.GetPosition(itemsControl);
            if (Math.Abs(mousePosition.X - _startDragInfo.DragStart.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(mousePosition.Y - _startDragInfo.DragStart.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var dataObject = new DataObject();
                dataObject.SetData(sender.GetType().Name, GetSelectedItems(itemsControl).OfType<object>().ToArray());
                //var adLayer = AdornerLayer.GetAdornerLayer(item);
                // myAdornment = new DraggableAdorner(item);
                //adLayer.Add(myAdornment);
                // Debug.Print($"DragDrop.DoDragDrop");
                try
                {
                    _isDragging = true;
                    var result = DragDrop.DoDragDrop(itemsControl, dataObject, DragDropEffects.Copy);
                    Debug.Print($"EndDrag: {result}");
                }
                finally
                {
                    _isDragging = false;
                    _startDragInfo.Clear();
                    _dragInfo.Clear();
                    ResetDragDrop(null);
                }

                //adLayer.Remove(myAdornment);
                e.Handled = true;
            }
        }

        public static void DropTarget_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            Debug.Print($"DragOver:");
            _dragInfo.LastDragLeaveObject = null;

            var a1 = e.Data.GetData(sender.GetType().Name);
            if (a1 == null)
            {
                ResetDragDrop(e);
                return;
            }

            var control = (ItemsControl)sender;
            DefineInsertIndex(control, e);
            if (!_dragInfo.InsertIndex.HasValue)
            {
                ResetDragDrop(e);
                return;
            }

            if (_dropTargetAdorner == null)
                _dropTargetAdorner = new DropTargetInsertionAdorner(control);
            _dropTargetAdorner.InvalidateVisual();

            if (_dragAdorner == null)
                _dragAdorner = new DragAdorner(Window.GetWindow(control).Content as UIElement, e.Data.GetData(sender.GetType().Name));
            _dragAdorner.UpdateUI(e, control);

            CheckScroll(control, e);
        }

        public static void DropTarget_OnPreviewDragLeave(object sender, DragEventArgs e)
        {
            _dragInfo.LastDragLeaveObject = sender;
            ((FrameworkElement)sender).Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Equals(_dragInfo.LastDragLeaveObject, sender))
                    ResetDragDrop(e);
                _dragInfo.LastDragLeaveObject = null;
            }), DispatcherPriority.Normal);
        }

        public static void DropTarget_OnPreviewDrop(object sender, DragEventArgs e)
        {
            Debug.Print($"Drop:");
            var sourceData = e.Data.GetData(sender.GetType().Name) as Array;
            var itemsControl = sender as ItemsControl;
            if (!_dragInfo.InsertIndex.HasValue) return;

            var insertIndex = _dragInfo.InsertIndex.Value + _dragInfo.FirstItemOffset;
            var targetData = (IList)itemsControl.ItemsSource ?? itemsControl.Items;
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
                {
                    if (o is TabItem tabItem)
                        ((TabControl) tabItem.Parent)?.Items.Remove(tabItem);
                    targetData.Insert(insertIndex++, o);
                }
            }
            ResetDragDrop(e);
        }
        #endregion

        private static DropTargetInsertionAdorner _dropTargetAdorner;
        private static DragAdorner _dragAdorner;
        internal static StartDragInfo _startDragInfo = new StartDragInfo();
        internal static DragInfo _dragInfo = new DragInfo();
        private static bool _isDragging;

        #region =============  Private methods  ================
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

        private static PropertyInfo _piItemsHost;
        internal static Panel GetItemsHost(ItemsControl itemsControl)
        {
            if (_piItemsHost == null)
                _piItemsHost = typeof(ItemsControl).GetProperty("ItemsHost", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (Panel)_piItemsHost.GetValue(itemsControl);
        }

        private static PropertyInfo _piDataGridHeaderHost;
        private static FrameworkElement GetHeaderHost(ItemsControl itemsControl)
        {
            if (itemsControl is DataGrid)
            {
                if (_piDataGridHeaderHost == null)
                    _piDataGridHeaderHost = typeof(DataGrid).GetProperty("ColumnHeadersPresenter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return (DataGridColumnHeadersPresenter) _piDataGridHeaderHost.GetValue(itemsControl);
            }
            return null;
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

        internal static Orientation GetItemsPanelOrientation(ItemsControl itemsControl)
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

        private static void DefineInsertIndex(ItemsControl control, DragEventArgs e)
        {
            var orientation = GetItemsPanelOrientation(control);
            var panel = GetItemsHost(control);
            _dragInfo.FirstItemOffset = panel.Children.Count == 0 ? 0 : control.ItemContainerGenerator.IndexFromContainer(panel.Children[0]);
            for (var i = 0; i < panel.Children.Count; i++)
            {
                var item = panel.Children[i] as FrameworkElement;
                if (item.IsMouseOverElement(e.GetPosition))
                {
                    var itemBounds = item.GetBoundsOfElement();
                    var mousePos = e.GetPosition(item);
                    if (orientation == Orientation.Vertical)
                        _dragInfo.InsertIndex = i + (mousePos.Y <= itemBounds.Bottom * 0.8 ? 0 : 1);
                    else
                        _dragInfo.InsertIndex = i + (mousePos.X <= itemBounds.Right * 0.8 ? 0 : 1);
                    return;
                }
            }

            var headerHost = GetHeaderHost(control);
            if (headerHost != null && headerHost.IsMouseOverElement(e.GetPosition))
            {
                _dragInfo.InsertIndex = 0;
                return;
            }

            _dragInfo.InsertIndex = panel.IsMouseOverElement(e.GetPosition) ? panel.Children.Count : (int?)null;
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

            if (e != null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
        #endregion

        #region =============  Helper classes  ===============
        internal class StartDragInfo
        {
            internal Point DragStart { get; private set; }
            internal ItemsControl DragSource { get; private set; }
            public void Init(ItemsControl dragSource, MouseEventArgs e)
            {
                DragSource = dragSource;
                DragStart = e.GetPosition(dragSource);
            }
            public void Clear()
            {
                DragSource = null;
                DragStart = new Point(-100, -100);
            }
        }

        internal class DragInfo
        {
            internal object LastDragLeaveObject;
            internal int? InsertIndex;
            internal int FirstItemOffset;
            public void Clear()
            {
                LastDragLeaveObject = null;
                InsertIndex = null;
            }
        }
        #endregion
    }
}
