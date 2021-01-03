using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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
            var currentWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (currentWindow != null)
                MaxWidth = Math.Max(400, currentWindow.ActualWidth / 2);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (oldContent is FrameworkElement)
            {
                var thumb = Tips.GetVisualChildren((FrameworkElement)oldContent).OfType<Thumb>().FirstOrDefault(e => e.Name == MovingThumbName);
                if (thumb != null)
                    thumb.DragDelta -= MoveThumb_OnDragDelta;
            }

            if (newContent is FrameworkElement content)
            {
                if (content.IsLoaded)
                    NewContent_OnLoaded(content, null);
                else
                    content.Loaded += NewContent_OnLoaded;
            }

            void NewContent_OnLoaded(object sender, RoutedEventArgs args)
            {
                content.Loaded -= NewContent_OnLoaded;
                var thumb = Tips.GetVisualChildren(content).OfType<Thumb>().FirstOrDefault(e => e.Name == MovingThumbName);
                if (thumb != null)
                    thumb.DragDelta += MoveThumb_OnDragDelta;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            foreach (var thumb in Tips.GetVisualChildren(this).OfType<Thumb>())
            {
                thumb.DragStarted += Thumb_OnDragStarted;
                thumb.DragDelta += ResizeThumb_OnDragDelta;
            }

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

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (Focusable)
                Focus();
        }
        #endregion

        #region ==========  Moving && resizing  =========
        private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            if (Focusable)
                Focus();
            e.Handled = true;
        }

        private void MoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var mousePosition = Mouse.GetPosition(HostPanel);
            if (mousePosition.X < 0 || mousePosition.Y < 0) return;

            var oldPosition = TranslatePoint(new Point(0, 0), HostPanel);
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

            if (HostPanel is Grid)
                Margin = new Thickness { Left = newX, Top = newY };
            else
            {
                if (!Tips.AreEqual(oldPosition.X, newX)) Canvas.SetLeft(this, newX);
                if (!Tips.AreEqual(oldPosition.Y, newY)) Canvas.SetTop(this, newY);
            }

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
            var oldPosition = TranslatePoint(new Point(0, 0), HostPanel);
            var change = Math.Min(horizontalChange, ActualWidth - MinWidth);

            if (LimitPositionToPanelBounds)
            {
                if (oldPosition.X + change < 0)
                    change = -oldPosition.X;
                if ((ActualWidth - change) > MaxWidth)
                    change = ActualWidth - MaxWidth;
            }

            if (!Tips.AreEqual(0.0, change))
            {
                Width = ActualWidth - change;
                if (HostPanel is Grid)
                    Margin = new Thickness(oldPosition.X + change, oldPosition.Y, 0, 0);
                else
                    Canvas.SetLeft(this, oldPosition.X + change);
            }
        }
        private void OnResizeTop(double verticalChange)
        {
            var oldPosition = TranslatePoint(new Point(0, 0), HostPanel);
            var change = Math.Min(verticalChange, ActualHeight - MinHeight);
            
            if (LimitPositionToPanelBounds)
            {
                if (oldPosition.Y + change < 0)
                    change = -oldPosition.Y;
                if ((Height - change) > MaxHeight)
                    change = Height - MaxHeight;
            }

            if (!Tips.AreEqual(0.0, change))
            {
                Height = ActualHeight - change;
                if (HostPanel is Grid)
                    Margin = new Thickness(oldPosition.X, oldPosition.Y + change, 0, 0);
                else
                    Canvas.SetTop(this, oldPosition.Y + change);
            }
        }
        private void OnResizeRight(double horizontalChange)
        {
            var change = Math.Min(-horizontalChange, ActualWidth - MinWidth);

            if (LimitPositionToPanelBounds)
            {
                var oldPosition = TranslatePoint(new Point(0, 0), HostPanel);
                if ((ActualWidth - change) > MaxWidth)
                    change = ActualWidth - MaxWidth;
                if ((oldPosition.X + ActualWidth - change) > HostPanel.ActualWidth)
                    change = oldPosition.X + ActualWidth - HostPanel.ActualWidth;
            }

            if (!Tips.AreEqual(0.0, change))
                Width = ActualWidth - change;
        }
        private void OnResizeBottom(double verticalChange)
        {
            var change = Math.Min(-verticalChange, ActualHeight - MinHeight);

            if (LimitPositionToPanelBounds)
            {
                var oldPosition = TranslatePoint(new Point(0, 0), HostPanel);
                if ((ActualHeight - change) > MaxHeight)
                    change = ActualHeight - MaxHeight;
                if ((oldPosition.Y + ActualHeight - change) > HostPanel.ActualHeight)
                    change = oldPosition.Y + ActualHeight - HostPanel.ActualHeight;
            }

            if (!Tips.AreEqual(0.0, change))
                Height = ActualHeight - change;
        }
        #endregion

        #region ==========  Properties  ===============
        public static readonly DependencyProperty DragThumbProperty =
            DependencyProperty.Register("DragThumb", typeof(Thumb), typeof(ResizingControl), new PropertyMetadata(null, OnDragThumbChanged));
        public Thumb DragThumb
        {
            get => (Thumb)GetValue(DragThumbProperty);
            set => SetValue(DragThumbProperty, value);
        }

        private static void OnDragThumbChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
            DependencyProperty.Register("EdgeThickness", typeof(Thickness), typeof(ResizingControl), new PropertyMetadata(new Thickness(6), OnEdgeThicknessChanged));

        public Thickness EdgeThickness
        {
            get => (Thickness)GetValue(EdgeThicknessProperty);
            set => SetValue(EdgeThicknessProperty, value);
        }
        private static void OnEdgeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // throw new NotImplementedException();
        }

        #endregion

    }
}
