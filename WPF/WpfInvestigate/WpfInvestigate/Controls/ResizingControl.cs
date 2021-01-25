using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfInvestigate.Common;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.Controls
{
    public class ResizingControl : ContentControl
    {
        static ResizingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizingControl), new FrameworkPropertyMetadata(typeof(ResizingControl)));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(ResizingControl), new FrameworkPropertyMetadata(false));
            FocusableProperty.OverrideMetadata(typeof(ResizingControl), new FrameworkPropertyMetadata(true));
        }

        public ResizingControl()
        {
            DataContext = this;
        }

        public const string MovingThumbName = "MovingThumb";
        public bool LimitPositionToPanelBounds { get; set; } = false;
        public bool IsWindowed => Parent is Window;

        private static int Unique = 1;
        private Grid HostPanel => VisualTreeHelper.GetParent(this) as Grid;

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

                BindingOperations.ClearBinding(this, MinWidthProperty);
                BindingOperations.ClearBinding(this, MaxWidthProperty);
                BindingOperations.ClearBinding(this, MinHeightProperty);
                BindingOperations.ClearBinding(this, MaxHeightProperty);

                dpdWidth.RemoveValueChanged(m_oldContent, OnWidthChanged);
                dpdHeight.RemoveValueChanged(m_oldContent, OnHeightChanged);
            }

            if (newContent is FrameworkElement m_newContent)
            {
                SetBinding(MinWidthProperty, new Binding("MinWidth") { Source = m_newContent });
                SetBinding(MaxWidthProperty, new Binding("MaxWidth") { Source = m_newContent });
                SetBinding(MinHeightProperty, new Binding("MinHeight") { Source = m_newContent });
                SetBinding(MaxHeightProperty, new Binding("MaxHeight") { Source = m_newContent });

                ControlHelper.SetFocus(m_newContent);

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
                if (!double.IsNaN(content.Width))
                {
                    var resizingControl = Tips.GetVisualParents(content).OfType<ResizingControl>().FirstOrDefault();
                    resizingControl.Width = content.Width;
                    content.Dispatcher.Invoke(() => content.Width = double.NaN, DispatcherPriority.Render);
                }
            }

            void OnHeightChanged(object sender, EventArgs e)
            {
                var content = (FrameworkElement)sender;
                if (!double.IsNaN(content.Height))
                {
                    var resizingControl = Tips.GetVisualParents(content).OfType<ResizingControl>().FirstOrDefault();
                    resizingControl.Height = content.Height;
                    content.Dispatcher.Invoke(() => content.Height = double.NaN, DispatcherPriority.Render);
                }
            }

            void OnContentLoaded(object sender, RoutedEventArgs args)
            {
                var content = (FrameworkElement)sender;
                content.Loaded -= OnContentLoaded;
                var thumb = Tips.GetVisualChildren(content).OfType<Thumb>().FirstOrDefault(e => e.Name == MovingThumbName);
                if (thumb != null)
                    thumb.DragDelta += MoveThumb_OnDragDelta;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            foreach (var thumb in Tips.GetVisualChildren(this).OfType<Thumb>())
                thumb.DragDelta += ResizeThumb_OnDragDelta;

            if (Tips.GetVisualParents(HostPanel).OfType<ScrollViewer>().FirstOrDefault() is ScrollViewer sv && !Equals(sv.Resources["State"], "Activated"))
            {
                sv.Resources["State"] = "Activated";
                sv.ScrollChanged += ScrollViewer_OnScrollChanged;
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (Parent is Window wnd && Position == new Point(-1, -1) && !wnd.IsLoaded)
                wnd.LocationChanged += OnWindowLocationChanged;

            void OnWindowLocationChanged(object sender, EventArgs e)
            {
                wnd.LocationChanged -= OnWindowLocationChanged;
                Position = new Point(wnd.Left, wnd.Top);
            }
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

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            // If thumb of focused control pressed => don't change focus & zindex
            if (Tips.GetElementsUnderMouseClick(this, e).OfType<FrameworkElement>().Any(c => c.TemplatedParent is Thumb) &&
                Tips.GetVisualParents(Keyboard.FocusedElement as DependencyObject).Any(c => c == this))
                return;

            if (Focusable) Focus();
        }
        #endregion

        #region ==========  Moving && resizing  =========
        private void MoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var mousePosition = IsWindowed ? PointToScreen(Mouse.GetPosition(this)) : Mouse.GetPosition(HostPanel);
            if (mousePosition.X < 0 || mousePosition.Y < 0) return;

            var oldPosition = Position;
            var newX = oldPosition.X + e.HorizontalChange;
            if (LimitPositionToPanelBounds)
            {
                if (newX + ActualWidth > HostPanel.ActualWidth)
                    newX = HostPanel.ActualWidth - ActualWidth;
                if (newX < 0) newX = 0;
            }

            var newY = oldPosition.Y + e.VerticalChange;
            if (LimitPositionToPanelBounds)
            {
                if (newY + ActualHeight > HostPanel.ActualHeight)
                    newY = HostPanel.ActualHeight - ActualHeight;
                if (newY < 0) newY = 0;
            }

            Position = new Point(newX, newY);
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
            var mousePosition = IsWindowed ? PointToScreen(Mouse.GetPosition(this)) : Mouse.GetPosition(HostPanel);
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
            var oldPosition = Position;
            var change = Math.Min(horizontalChange, ActualWidth - MinWidth);

            if (LimitPositionToPanelBounds)
            {
                if (oldPosition.X + change < 0) change = -oldPosition.X;
                if ((ActualWidth - change) > MaxWidth) change = ActualWidth - MaxWidth;
            }

            if (!Tips.AreEqual(0.0, change))
            {
                Width = ActualWidth - change;
                Position = new Point(oldPosition.X + change, oldPosition.Y);
            }
        }
        private void OnResizeTop(double verticalChange)
        {
            var oldPosition = Position;
            var change = Math.Min(verticalChange, ActualHeight - MinHeight);

            if (LimitPositionToPanelBounds)
            {
                if (oldPosition.Y + change < 0) change = -oldPosition.Y;
                if ((Height - change) > MaxHeight) change = Height - MaxHeight;
            }

            if (!Tips.AreEqual(0.0, change))
            {
                Height = ActualHeight - change;
                Position = new Point(oldPosition.X, oldPosition.Y + change);
            }
        }
        private void OnResizeRight(double horizontalChange)
        {
            var change = Math.Min(-horizontalChange, ActualWidth - MinWidth);
            if (LimitPositionToPanelBounds)
            {
                var oldX = Position.X;
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
                var oldY = Position.Y;
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
        public static readonly DependencyProperty MovingThumbProperty = DependencyProperty.Register("MovingThumb",
            typeof(Thumb), typeof(ResizingControl), new PropertyMetadata(null, OnMovingThumbValueChanged));
        public Thumb MovingThumb
        {
            get => (Thumb)GetValue(MovingThumbProperty);
            set => SetValue(MovingThumbProperty, value);
        }

        private static void OnMovingThumbValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
        public static readonly DependencyProperty EdgeThicknessProperty = DependencyProperty.Register("EdgeThickness",
            typeof(Thickness), typeof(ResizingControl), new PropertyMetadata(new Thickness(6)));

        public Thickness EdgeThickness
        {
            get => (Thickness)GetValue(EdgeThicknessProperty);
            set => SetValue(EdgeThicknessProperty, value);
        }
        //================================
        public static readonly DependencyProperty ResizableProperty = DependencyProperty.Register(nameof(Resizable),
            typeof(bool), typeof(ResizingControl), new UIPropertyMetadata(true));
        public bool Resizable
        {
            get => (bool)GetValue(ResizableProperty);
            set => SetValue(ResizableProperty, value);
        }
        //=========================
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(Point), typeof(ResizingControl), new UIPropertyMetadata(new Point(-1, -1), OnPositionValueChanged));
        public Point Position
        {
            get => (Point)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }
        private static void OnPositionValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((Point)e.NewValue == (Point)e.OldValue) return;

            var resizingControl = (ResizingControl)d;
            var newPosition = (Point)e.NewValue;
            if (resizingControl.Parent is Window window)
            {
                var newTop = Math.Min(SystemParameters.PrimaryScreenHeight, newPosition.Y);
                if (!Tips.AreEqual(newTop, window.Top))
                    window.Top = newTop;
                var newLeft = Math.Min(SystemParameters.PrimaryScreenWidth, newPosition.X);
                if (!Tips.AreEqual(newLeft, window.Left))
                    window.Left = newLeft;
            }
            else
                resizingControl.Margin = new Thickness(newPosition.X, newPosition.Y, -1, -1);
        }
        #endregion
    }
}
