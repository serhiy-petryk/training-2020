using System;
using System.Collections;
using System.Collections.Generic;
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
        public static readonly StartDragInfo StartDrag_Info = new StartDragInfo();
        public static readonly DragInfo Drag_Info = new DragInfo();

        #region ==============  Examples of event handlers  ==============
        public static void DragSource_OnPreviewMouseMove(object sender, MouseEventArgs e, string dragDropFormat = null)
        {
            if (_isDragging) return;
            if (!(sender is ItemsControl itemsControl) || e.LeftButton == MouseButtonState.Released ||
                GetSelectedItems(itemsControl, e).Count == 0)
            {
                StartDrag_Info.Clear();
                return;
            }

            var itemsHost = GetItemsHost(itemsControl);
            if (!itemsHost.IsMouseOverElement(e.GetPosition))
            {
                StartDrag_Info.Clear();
                return;
            }

            if (!Equals(itemsControl, StartDrag_Info.DragSource))
            {
                StartDrag_Info.Init(itemsControl, e);
                return;
            }

            var mousePosition = e.GetPosition(itemsControl);
            if (Math.Abs(mousePosition.X - StartDrag_Info.DragStart.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(mousePosition.Y - StartDrag_Info.DragStart.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (itemsControl is DataGrid dataGrid)
                    dataGrid.CommitEdit();

                var dataObject = new DataObject();
                dataObject.SetData(dragDropFormat ?? sender.GetType().Name, GetSelectedItems(itemsControl, e).ToArray());
                try
                {
                    _isDragging = true;
                    var result = DragDrop.DoDragDrop(itemsControl, dataObject, DragDropEffects.Copy);
                }
                finally
                {
                    _isDragging = false;
                    StartDrag_Info.Clear();
                    Drag_Info.Clear();
                    ResetDragDrop(null);
                }

                //adLayer.Remove(myAdornment);
                e.Handled = true;
            }
        }

        public static void DragSource_OnPreviewGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Copy)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Arrow);
            }
            else
                e.UseDefaultCursors = true;

            e.Handled = true;
        }

        public static void DropTarget_OnPreviewDragOver(object sender, DragEventArgs e, IEnumerable<string> formats = null)
        {
            Drag_Info.LastDragLeaveObject = null;

            formats = formats ?? new[] { sender.GetType().Name };
            object dragData = null;
            foreach (var format in formats)
            {
                dragData = e.Data.GetData(format);
                if (dragData != null)
                    break;
            }
            if (dragData == null)
            {
                ResetDragDrop(e);
                return;
            }

            var control = (ItemsControl)sender;
            DefineInsertIndex(control, e);
            if (!Drag_Info.InsertIndex.HasValue)
            {
                ResetDragDrop(e);
                return;
            }

            if (_dropTargetAdorner == null)
                _dropTargetAdorner = new DropTargetInsertionAdorner(control);
            _dropTargetAdorner.InvalidateVisual();

            if (_dragAdorner == null)
                _dragAdorner = new DragAdorner(Window.GetWindow(control).Content as UIElement, dragData);
            _dragAdorner.UpdateUI(e, control);

            CheckScroll(control, e);
        }

        public static void DropTarget_OnPreviewDragLeave(object sender, DragEventArgs e)
        {
            Drag_Info.LastDragLeaveObject = sender;
            ((FrameworkElement)sender).Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Equals(Drag_Info.LastDragLeaveObject, sender))
                    ResetDragDrop(e);
                Drag_Info.LastDragLeaveObject = null;
            }), DispatcherPriority.Normal);
        }

        public static void DropTarget_OnPreviewDrop(object sender, DragEventArgs e, string dragDropFormat = null)
        {
            if (!Drag_Info.InsertIndex.HasValue) return;
            var sourceData = e.Data.GetData(dragDropFormat ?? sender.GetType().Name) as Array;
            var control = sender as ItemsControl;

            var insertIndex = Drag_Info.InsertIndex.Value + Drag_Info.FirstItemOffset;
            var targetData = (IList)control.ItemsSource ?? control.Items;
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
                        ((TabControl)tabItem.Parent)?.Items.Remove(tabItem);
                    targetData.Insert(insertIndex++, o);
                }
            }

            Mouse.OverrideCursor = null;
            e.Handled = true;
        }
        #endregion

        #region =======  Public methods  ========
        public static Panel GetItemsHost(ItemsControl itemsControl)
        {
            if (_piItemsHost == null)
                _piItemsHost = typeof(ItemsControl).GetProperty("ItemsHost", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (Panel)_piItemsHost.GetValue(itemsControl);
        }
        #endregion

        private static DropTargetInsertionAdorner _dropTargetAdorner;
        private static DragAdorner _dragAdorner;
        private static bool _isDragging;

        #region =============  Private methods  ================
        private static List<object> GetSelectedItems(ItemsControl control, MouseEventArgs e)
        {
            if (control is MultiSelector multiSelector)
                return new List<object>(multiSelector.SelectedItems.OfType<object>());

            if (control is Selector selector)
            {
                if (selector.SelectedItem != null)
                    return new List<object> { selector.SelectedItem };
                return new List<object>();
            }

            if (control is TreeView treeView)
            {
                if (treeView.SelectedItem != null)
                    return new List<object> { treeView.SelectedItem };
                return new List<object>();
            }

            throw new Exception($"Trap! {control.GetType().Name} is not supported");
        }

        private static PropertyInfo _piItemsHost;
        private static PropertyInfo _piDataGridHeaderHost;
        private static FrameworkElement GetHeaderHost(ItemsControl itemsControl)
        {
            if (itemsControl is DataGrid)
            {
                if (_piDataGridHeaderHost == null)
                    _piDataGridHeaderHost = typeof(DataGrid).GetProperty("ColumnHeadersPresenter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return (DataGridColumnHeadersPresenter)_piDataGridHeaderHost.GetValue(itemsControl);
            }
            return null;
        }

        private static void CheckScroll(ItemsControl o, DragEventArgs e)
        {
            var scrollViewer = o.GetVisualChildren().OfType<ScrollViewer>().FirstOrDefault();
            if (scrollViewer != null)
            {
                const double scrollMargin = 25.0;
                var _scrollStep = 1.0;
                var position = e.GetPosition(scrollViewer);
                if (position.X >= scrollViewer.ActualWidth - scrollMargin && scrollViewer.HorizontalOffset <
                    scrollViewer.ExtentWidth - scrollViewer.ViewportWidth)
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + _scrollStep);
                else if (position.X < scrollMargin && scrollViewer.HorizontalOffset > 0)
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - _scrollStep);
                else if (position.Y >= scrollViewer.ActualHeight - scrollMargin && scrollViewer.VerticalOffset <
                         scrollViewer.ExtentHeight - scrollViewer.ViewportHeight)
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + _scrollStep);
                else if (position.Y < scrollMargin && scrollViewer.VerticalOffset > 0)
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - _scrollStep);
            }
        }

        internal static Orientation GetItemsPanelOrientation(ItemsControl itemsControl)
        {
            if (itemsControl is TabControl)
            {
                var tabControl = (TabControl)itemsControl;
                return tabControl.TabStripPlacement == Dock.Left || tabControl.TabStripPlacement == Dock.Right
                    ? Orientation.Vertical
                    : Orientation.Horizontal;
            }

            var panel = GetItemsHost(itemsControl);
            var orientationProperty = panel.GetType().GetProperty("Orientation", typeof(Orientation));

            if (orientationProperty != null)
                return (Orientation)orientationProperty.GetValue(panel, null);

            throw new Exception("Trap! Can't define item panel orientation");
        }

        private static void DefineInsertIndex(ItemsControl control, DragEventArgs e)
        {
            var orientation = GetItemsPanelOrientation(control);
            var panel = GetItemsHost(control);

            Drag_Info.FirstItemOffset = panel.Children.Count == 0 ? 0 : control.ItemContainerGenerator.IndexFromContainer(panel.Children[0]);
            Drag_Info.DragDropEffect = StartDrag_Info.DragSource == control ? DragDropEffects.Move : DragDropEffects.Copy;

            for (var i = 0; i < panel.Children.Count; i++)
            {
                var item = panel.Children[i] as FrameworkElement;
                if (item.IsMouseOverElement(e.GetPosition))
                {
                    var itemBounds = item.GetBoundsOfElement();
                    var mousePos = e.GetPosition(item);
                    if (orientation == Orientation.Vertical)
                        Drag_Info.InsertIndex = i + (mousePos.Y <= itemBounds.Bottom * 0.75 ? 0 : 1);
                    else
                        Drag_Info.InsertIndex = i + (mousePos.X <= itemBounds.Right * 0.75 ? 0 : 1);
                    return;
                }
            }

            var headerHost = GetHeaderHost(control);
            if (headerHost != null && headerHost.IsMouseOverElement(e.GetPosition))
            {
                Drag_Info.InsertIndex = 0;
                return;
            }

            Drag_Info.InsertIndex = panel.IsMouseOverElement(e.GetPosition) ? panel.Children.Count : (int?)null;
            if (!Drag_Info.InsertIndex.HasValue)
                Drag_Info.DragDropEffect = DragDropEffects.None;
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
        public class StartDragInfo
        {
            internal Point DragStart { get; private set; }
            public ItemsControl DragSource { get; private set; }
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

        public class DragInfo
        {
            public object LastDragLeaveObject;
            public int? InsertIndex;
            public int FirstItemOffset;
            public DragDropEffects DragDropEffect;
            public object GetHoveredItem(ItemsControl control)
            {
                if (!InsertIndex.HasValue) return null;
                var panel = GetItemsHost(control);
                if (InsertIndex.Value < panel.Children.Count)
                    return panel.Children[InsertIndex.Value];
                if (InsertIndex.Value == panel.Children.Count && panel.Children.Count > 0)
                    return panel.Children[panel.Children.Count - 1];
                return null;
            }

            public void Clear()
            {
                LastDragLeaveObject = null;
                InsertIndex = null;
            }
        }
        #endregion
    }
}
