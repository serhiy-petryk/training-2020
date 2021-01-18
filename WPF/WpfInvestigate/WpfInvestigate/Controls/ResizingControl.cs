using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public class ResizingControl : ContentControl
    {
        public const string MovingThumbName = "MovingThumb";
        public bool LimitPositionToPanelBounds { get; set; } = false;

        private static int Unique = 1;
        private Panel HostPanel => VisualTreeHelper.GetParent(this) as Panel;

        public ResizingControl()
        {
            DataContext = this;
            /*var currentWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (currentWindow != null)
                MaxWidth = Math.Max(400, currentWindow.ActualWidth / 2);*/
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            var dpdWidth = DependencyPropertyDescriptor.FromProperty(WidthProperty, typeof(FrameworkElement));
            var dpdHeight = DependencyPropertyDescriptor.FromProperty(HeightProperty, typeof(FrameworkElement));

            if (oldContent is FrameworkElement m_oldContent)
            {
                var thumb = Tips.GetVisualChildren(m_oldContent).OfType<Thumb>().FirstOrDefault(e => e.Name == MovingThumbName);
                if (thumb != null)
                    thumb.DragDelta -= MoveThumb_OnDragDelta;

                dpdWidth.RemoveValueChanged(m_oldContent, OnWidthChanged);
                dpdHeight.RemoveValueChanged(m_oldContent, OnHeightChanged);
            }

            if (newContent is FrameworkElement m_newContent)
            {
                SetBinding(MinWidthProperty, new Binding("MinWidth") { Source = m_newContent });
                SetBinding(MaxWidthProperty, new Binding("MaxWidth") { Source = m_newContent });
                SetBinding(MinHeightProperty, new Binding("MinHeight") { Source = m_newContent });
                SetBinding(MaxHeightProperty, new Binding("MaxHeight") { Source = m_newContent });

                if (m_newContent.IsLoaded)
                    OnContentLoaded(m_newContent, null);
                else
                    m_newContent.Loaded += OnContentLoaded;

                dpdWidth.AddValueChanged(m_newContent, OnWidthChanged);
                dpdHeight.AddValueChanged(m_newContent, OnHeightChanged);
            }

            void OnWidthChanged(object sender, EventArgs e)
            {
                var content = (FrameworkElement)sender;
                var resizingControl = Tips.GetVisualParents(content).OfType<ResizingControl>().FirstOrDefault();
                Debug.Print($"OnWidthChanged: {content.Width}");
                if (!double.IsNaN(content.Width))
                {
                    resizingControl.Width = content.Width;
                    content.Dispatcher.Invoke(() =>
                    {
                        Debug.Print($"OnWidthChanged.Dispatcher: {content.Width}");
                        content.Width = double.NaN;
                    }, DispatcherPriority.Render);
                }
            }

            void OnHeightChanged(object sender, EventArgs e)
            {
                var content = (FrameworkElement)sender;
                var resizingControl = Tips.GetVisualParents(content).OfType<ResizingControl>().FirstOrDefault();
                Debug.Print($"OnHeightChanged: {content.Height}");
                if (!double.IsNaN(content.Height))
                {
                    resizingControl.Height = content.Height;
                    content.Dispatcher.Invoke(() =>
                    {
                        Debug.Print($"OnHeightChanged.Dispatcher: {content.Height}");
                        content.Height = double.NaN;
                    }, DispatcherPriority.Render);
                }
            }

            void OnContentLoaded(object sender, RoutedEventArgs args)
            {
                var content = (FrameworkElement) sender;
                content.Loaded -= OnContentLoaded;

                var resizingControl = Tips.GetVisualParents(content).OfType<ResizingControl>().FirstOrDefault();
                // resizingControl.Width = content.ActualWidth;
                Debug.Print($"OnContentLoaded");
                /*/ resizingControl.Height = content.ActualHeight;
                resizingControl.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    Debug.Print($"OnContentLoaded.Dispatcher");
                    // resizingControl.Width = double.NaN;
                    resizingControl.Height = double.NaN;
                    // content2.Width = double.NaN;
                }));*/

                var thumb = Tips.GetVisualChildren(content).OfType<Thumb>().FirstOrDefault(e => e.Name == MovingThumbName);
                if (thumb != null)
                    thumb.DragDelta += MoveThumb_OnDragDelta;

                /*if (!double.IsNaN(content2.Width))
                {
                    var b = new Binding("ActualWidth") { Source = this };
                    content2.SetBinding(Control.WidthProperty, b);
                }
                if (!double.IsNaN(content2.Height))
                {
                    var b = new Binding("ActualHeight") { Source = this };
                    content2.SetBinding(Control.HeightProperty, b);
                }*/
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            foreach (var thumb in Tips.GetVisualChildren(this).OfType<Thumb>())
                thumb.DragDelta += ResizeThumb_OnDragDelta;

            var sv = Tips.GetVisualParents(HostPanel).OfType<ScrollViewer>().FirstOrDefault();
            if (sv != null) sv.ScrollChanged += ScrollViewer_OnScrollChanged;
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var sv = (ScrollViewer)sender;
            // If unnecessary scrollbars are visible -> remove them
            if (sv.ScrollableWidth > 0 && sv.ExtentWidth <= sv.ActualWidth)
            {
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            if (sv.ScrollableHeight > 0 && sv.ExtentHeight <= sv.ActualHeight)
            {
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        #region ==========  Focus  ============
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            Panel.SetZIndex(this, Unique++);
        }

        /*protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (Content is FrameworkElement content)
            {
                if (!double.IsNaN(content.Width)) content.Width = double.NaN;
                if (!double.IsNaN(content.Height)) content.Height = double.NaN;
            }
        }*/

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (Focusable)
                Focus();
        }
        #endregion

        #region ==========  Moving && resizing  =========
        private void MoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var mousePosition = Mouse.GetPosition(HostPanel);
            if (mousePosition.X < 0 || mousePosition.Y < 0) return;

            var oldX = HostPanel is Canvas ? Canvas.GetLeft(this) : Margin.Left;
            var oldY = HostPanel is Canvas ? Canvas.GetTop(this) : Margin.Top;

            var newX = oldX + e.HorizontalChange;
            if (LimitPositionToPanelBounds)
            {
                if (newX + ActualWidth > HostPanel.ActualWidth)
                    newX = HostPanel.ActualWidth - ActualWidth;
                if (newX < 0) newX = 0;
            }

            var newY = oldY + e.VerticalChange;
            if (LimitPositionToPanelBounds)
            {
                if (newY + ActualHeight > HostPanel.ActualHeight)
                    newY = HostPanel.ActualHeight - ActualHeight;
                if (newY < 0) newY = 0;
            }

            if (HostPanel is Canvas)
            {
                if (!Tips.AreEqual(oldX, newX)) Canvas.SetLeft(this, newX);
                if (!Tips.AreEqual(oldY, newY)) Canvas.SetTop(this, newY);
            }
            else
                Margin = new Thickness(newX, newY, -1, -1);

            e.Handled = true;

            var sv = Tips.GetVisualParents(HostPanel).OfType<ScrollViewer>().FirstOrDefault();
            if (sv != null)
            {
                // Smooth scrolling
                if (Math.Abs(e.HorizontalChange) > Tips.SCREEN_TOLERANCE && (newX + ActualWidth) > sv.ActualWidth)
                    sv.ScrollToHorizontalOffset(Math.Max(0, sv.HorizontalOffset + e.HorizontalChange * 0.5));
                if (Math.Abs(e.VerticalChange) > Tips.SCREEN_TOLERANCE && (newY + ActualHeight) > sv.ActualHeight)
                    sv.ScrollToVerticalOffset(Math.Max(0, sv.VerticalOffset + e.VerticalChange * 0.5));
            }
        }

        private void ResizeThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var mousePosition = Mouse.GetPosition(HostPanel);
            if (mousePosition.X < 0 || mousePosition.Y < 0) return;

            var thumb = (Thumb)sender;
            if (thumb.HorizontalAlignment == HorizontalAlignment.Left)
                OnResizeLeft(e.HorizontalChange);
            else if (thumb.HorizontalAlignment == HorizontalAlignment.Right)
                OnResizeRight(e.HorizontalChange);

            if (thumb.VerticalAlignment == VerticalAlignment.Top)
                OnResizeTop(e.VerticalChange);
            else if (thumb.VerticalAlignment == VerticalAlignment.Bottom)
                OnResizeBottom(e.VerticalChange);

            e.Handled = true;
            BringIntoView();
        }

        private void OnResizeLeft(double horizontalChange)
        {
            var oldX = HostPanel is Canvas ? Canvas.GetLeft(this) : Margin.Left;
            var change = Math.Min(horizontalChange, ActualWidth - MinWidth);

            if (LimitPositionToPanelBounds)
            {
                if (oldX + change < 0) change = -oldX;
                if ((ActualWidth - change) > MaxWidth) change = ActualWidth - MaxWidth;
            }

            if (!Tips.AreEqual(0.0, change))
            {
                Width = ActualWidth - change;
                if (HostPanel is Canvas)
                    Canvas.SetLeft(this, oldX + change);
                else
                    Margin = new Thickness(oldX + change, Margin.Top, -1, -1);
            }
        }
        private void OnResizeTop(double verticalChange)
        {
            var oldY = HostPanel is Canvas ? Canvas.GetTop(this) : Margin.Top;
            var change = Math.Min(verticalChange, ActualHeight - MinHeight);
            
            if (LimitPositionToPanelBounds)
            {
                if (oldY + change < 0) change = -oldY;
                if ((Height - change) > MaxHeight) change = Height - MaxHeight;
            }

            if (!Tips.AreEqual(0.0, change))
            {
                Height = ActualHeight - change;
                if (HostPanel is Canvas)
                    Canvas.SetTop(this, oldY + change);
                else
                    Margin = new Thickness(Margin.Left, oldY + change, -1, -1);
            }
        }
        private void OnResizeRight(double horizontalChange)
        {
            var change = Math.Min(-horizontalChange, ActualWidth - MinWidth);

            if (LimitPositionToPanelBounds)
            {
                var oldX = HostPanel is Canvas ? Canvas.GetLeft(this) : Margin.Left;
                if ((ActualWidth - change) > MaxWidth)
                    change = ActualWidth - MaxWidth;
                if ((oldX + ActualWidth - change) > HostPanel.ActualWidth)
                    change = oldX + ActualWidth - HostPanel.ActualWidth;
            }

            if (!Tips.AreEqual(0.0, change))
                Width = ActualWidth - change;
        }
        private void OnResizeBottom(double verticalChange)
        {
            var change = Math.Min(-verticalChange, ActualHeight - MinHeight);

            if (LimitPositionToPanelBounds)
            {
                var oldY = HostPanel is Canvas ? Canvas.GetTop(this) : Margin.Top;
                if ((ActualHeight - change) > MaxHeight)
                    change = ActualHeight - MaxHeight;
                if ((oldY + ActualHeight - change) > HostPanel.ActualHeight)
                    change = oldY + ActualHeight - HostPanel.ActualHeight;
            }

            if (!Tips.AreEqual(0.0, change))
                Height = ActualHeight - change;
        }
        #endregion

        #region ==========  Properties  ===============
        public static readonly DependencyProperty MovingThumbProperty =
            DependencyProperty.Register("MovingThumb", typeof(Thumb), typeof(ResizingControl), new PropertyMetadata(null, OnMovingThumbChanged));
        public Thumb MovingThumb
        {
            get => (Thumb)GetValue(MovingThumbProperty);
            set => SetValue(MovingThumbProperty, value);
        }

        private static void OnMovingThumbChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ResizingControl control)
            {
                if (e.OldValue is Thumb oldThumb)
                    oldThumb.DragDelta -= control.MoveThumb_OnDragDelta;
                if (e.NewValue is Thumb newThumb) 
                    newThumb.DragDelta += control.MoveThumb_OnDragDelta;
            }
        }
        //=========================
        public static readonly DependencyProperty EdgeThicknessProperty =
            DependencyProperty.Register("EdgeThickness", typeof(Thickness), typeof(ResizingControl), new PropertyMetadata(new Thickness(6)));

        public Thickness EdgeThickness
        {
            get => (Thickness)GetValue(EdgeThicknessProperty);
            set => SetValue(EdgeThicknessProperty, value);
        }
        #endregion

    }
}
