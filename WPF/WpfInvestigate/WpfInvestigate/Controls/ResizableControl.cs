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
    public class ResizableControl : ContentControl
    {
        private static int Unique = 0;
        private Panel HostPanel => VisualTreeHelper.GetParent(this) as Panel;

        public bool LimitPositionToPanelBounds { get; set; } = false;

        public ResizableControl()
        {
            DataContext = this;
            var currentWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (currentWindow != null)
                MaxWidth = Math.Max(400, currentWindow.ActualWidth / 2);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            foreach (var thumb in Tips.GetVisualChildren(this).OfType<Thumb>())
            {
                thumb.DragStarted += Thumb_OnDragStarted;
                if (thumb.Name == "PART_HeaderMover")
                    thumb.DragDelta += MoveThumb_OnDragDelta;
                else
                    thumb.DragDelta += ResizeThumb_OnDragDelta;
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
            BringIntoView();
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

    }
}
