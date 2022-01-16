using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

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
            internal DragEventArgs _currentEventArgs;
            public DropInfo(DragEventArgs args)
            {
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
            var control = (ItemsControl) sender;
            _dropInfo = new DropInfo(e);

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
                    ResetDragDrop(e);
                    return;
                }
            }

            if (_dropTargetAdorner == null)
                _dropTargetAdorner = new DropTargetInsertionAdorner(control);
            _dropTargetAdorner.InvalidateVisual();

            if (_dragAdorner == null)
                CreateDragAdorner(control, e);
            UpdateDragAdorner(e);

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
        // public static void DropTarget_OnPreviewDragLeave(object sender, DragEventArgs e) => ResetDragDrop(e);

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
                        return i + (mousePos.Y <= itemBounds.Bottom / 2 ? 0 : 1);
                    return i + (mousePos.X <= itemBounds.Right / 2 ? 0 : 1);
                }
            }
            return panel.IsMouseOverElement(e.GetPosition) ? panel.Children.Count : 0;
        }

        private static void ResetDragDrop(DragEventArgs e)
        {
            if (_dropTargetAdorner != null)
            {
                _dropTargetAdorner.Detatch();
                _dropTargetAdorner = null;
            }
            if (_dragAdorner != null)
            {
                _dragAdorner.Detatch();
                _dragAdorner = null;
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        //============================
        private static DragAdorner _dragAdorner;
        private static Point _adornerPos;
        private static Size _adornerSize;
        private static void CreateDragAdorner(UIElement adornedElement, DragEventArgs e)
        {
            var template = Application.Current.Resources["DragAdorner"] as DataTemplate;
            var sourceData = e.Data.GetData("Source") as Array;
            //            if (((IEnumerable)sourceData.Length).Cast<object>().Count() <= 10)
            // {
            var itemsControl = new ItemsControl();
            itemsControl.ItemsSource = sourceData;
            itemsControl.ItemTemplate = template;
            // itemsControl.ItemTemplateSelector = templateSelector;

            // The ItemsControl doesn't display unless we create a border to contain it.
            // Not quite sure why this is...
            var border = new Border();
            border.Child = itemsControl;
            var adornment = border;
            // }

            var parentWindow = Window.GetWindow(_dragInfo.DragSource);
            var rootElement = parentWindow != null ? parentWindow.Content as UIElement : null;
            if (rootElement == null && Application.Current != null && Application.Current.MainWindow != null)
            {
                rootElement = (UIElement)Application.Current.MainWindow.Content;
            }
            //                i don't want the fu... windows forms reference
            //                if (rootElement == null) {
            //                    var elementHost = m_DragInfo.VisualSource.GetVisualAncestor<ElementHost>();
            //                    rootElement = elementHost != null ? elementHost.Child : null;
            //                }
            /*if (rootElement == null)
            {
                rootElement = m_DragInfo.VisualSource.GetVisualAncestor<UserControl>();
            }*/

            // _dragAdorner = new DragAdorner(rootElement, adornment);
            _dragAdorner = new DragAdorner(adornedElement, adornment);
            var k = 0;
            /*var template = GetDragAdornerTemplate(m_DragInfo.VisualSource);
            var templateSelector = GetDragAdornerTemplateSelector(m_DragInfo.VisualSource);

            UIElement adornment = null;

            var useDefaultDragAdorner = GetUseDefaultDragAdorner(m_DragInfo.VisualSource);

            if (template == null && templateSelector == null && useDefaultDragAdorner)
            {
                template = new DataTemplate();

                var factory = new FrameworkElementFactory(typeof(Image));

                var bs = CaptureScreen(m_DragInfo.VisualSourceItem, m_DragInfo.VisualSourceFlowDirection);
                factory.SetValue(Image.SourceProperty, bs);
                factory.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                factory.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
                if (m_DragInfo.VisualSourceItem is FrameworkElement)
                {
                    factory.SetValue(FrameworkElement.WidthProperty, ((FrameworkElement)m_DragInfo.VisualSourceItem).ActualWidth);
                    factory.SetValue(FrameworkElement.HeightProperty, ((FrameworkElement)m_DragInfo.VisualSourceItem).ActualHeight);
                    factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top);
                }
                template.VisualTree = factory;
            }

            if (template != null || templateSelector != null)
            {
                if (m_DragInfo.Data is IEnumerable && !(m_DragInfo.Data is string))
                {
                    if (((IEnumerable)m_DragInfo.Data).Cast<object>().Count() <= 10)
                    {
                        var itemsControl = new ItemsControl();
                        itemsControl.ItemsSource = (IEnumerable)m_DragInfo.Data;
                        itemsControl.ItemTemplate = template;
                        itemsControl.ItemTemplateSelector = templateSelector;

                        // The ItemsControl doesn't display unless we create a border to contain it.
                        // Not quite sure why this is...
                        var border = new Border();
                        border.Child = itemsControl;
                        adornment = border;
                    }
                }
                else
                {
                    var contentPresenter = new ContentPresenter();
                    contentPresenter.Content = m_DragInfo.Data;
                    contentPresenter.ContentTemplate = template;
                    contentPresenter.ContentTemplateSelector = templateSelector;
                    adornment = contentPresenter;
                }
            }

            if (adornment != null)
            {
                if (useDefaultDragAdorner)
                {
                    adornment.Opacity = GetDefaultDragAdornerOpacity(m_DragInfo.VisualSource);
                }

                var parentWindow = m_DragInfo.VisualSource.GetVisualAncestor<Window>();
                var rootElement = parentWindow != null ? parentWindow.Content as UIElement : null;
                if (rootElement == null && Application.Current != null && Application.Current.MainWindow != null)
                {
                    rootElement = (UIElement)Application.Current.MainWindow.Content;
                }
                //                i don't want the fu... windows forms reference
                //                if (rootElement == null) {
                //                    var elementHost = m_DragInfo.VisualSource.GetVisualAncestor<ElementHost>();
                //                    rootElement = elementHost != null ? elementHost.Child : null;
                //                }
                if (rootElement == null)
                {
                    rootElement = m_DragInfo.VisualSource.GetVisualAncestor<UserControl>();
                }

                DragAdorner = new DragAdorner(rootElement, adornment);
            }*/
        }

        private static void UpdateDragAdorner(DragEventArgs e)
        {
            if (_dragAdorner != null)
            {
                var tempAdornerPos = e.GetPosition(_dragAdorner.AdornedElement);
                if (tempAdornerPos.X > 0 && tempAdornerPos.Y > 0)
                    _adornerPos = tempAdornerPos;

                // Fixed the flickering adorner - Size changes to zero 'randomly'...?
                /*if (_dragAdorner.RenderSize.Width > 0 && _dragAdorner.RenderSize.Height > 0)
                {
                    _adornerSize = _dragAdorner.RenderSize;
                }*/

                if (_dragInfo != null)
                {
                    // move the adorner
                    var offsetX = 5;//_adornerSize.Width * -GetDragMouseAnchorPoint(_dragInfo.DragSource).X;
                    var offsetY = - _dragAdorner.ActualHeight + 5.0;//_adornerSize.Height * -GetDragMouseAnchorPoint(_dragInfo.DragSource).Y;
                    _adornerPos.Offset(offsetX, offsetY);
                    var maxAdornerPosX = _dragAdorner.AdornedElement.RenderSize.Width;
                    var adornerPosRightX = _adornerPos.X + _adornerSize.Width;
                    if (adornerPosRightX > maxAdornerPosX)
                        _adornerPos.Offset(-adornerPosRightX + maxAdornerPosX, 0);
                }

                _dragAdorner.MousePosition = _adornerPos;
                _dragAdorner.InvalidateVisual();
            }
        }
    }
}
