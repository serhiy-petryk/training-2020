using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using LightyTest.Service;

namespace LightySample
{
    public partial class SampleDialogMovable : UserControl
    {
        public SampleDialogMovable()
        {
            InitializeComponent();
        }

        private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            if (!Focusable)
                Focus();
            e.Handled = true;
        }

        private void MoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var panel = Tips.GetVisualParents(this).OfType<ItemsPresenter>().FirstOrDefault();
            if (panel != null)
            {
                var oldMargin = panel.Margin;
                panel.Margin = new Thickness { Left = Math.Max(0, oldMargin.Left + e.HorizontalChange), Top = Math.Max(0, oldMargin.Top + e.VerticalChange) };
            }
            e.Handled = true;
        }

        private void ResizeThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
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
        }

        private void OnResizeLeft(double horizontalChange)
        {
            var panel = Tips.GetVisualParents(this).OfType<ItemsPresenter>().FirstOrDefault();
            if (panel != null)
            {
                var change = Math.Min(horizontalChange, ActualWidth - MinWidth);
                if (panel.Margin.Left + change < 0)
                    change = -panel.Margin.Left;
                if ((Width - change) > MaxWidth)
                    change = Width - MaxWidth;

                if (!Tips.AreEqual(0.0, change))
                {
                    Width -= change;
                    panel.Margin = new Thickness(panel.Margin.Left + change, panel.Margin.Top, 0, 0);
                }
            }
        }

        private void OnResizeTop(double verticalChange)
        {
            var panel = Tips.GetVisualParents(this).OfType<ItemsPresenter>().FirstOrDefault();
            if (panel != null)
            {
                var change = Math.Min(verticalChange, ActualHeight - MinHeight);
                if (panel.Margin.Top + change < 0)
                    change = -panel.Margin.Top;
                if ((Height - change) > MaxHeight)
                    change = Height - MaxHeight;

                if (!Tips.AreEqual(0.0, change))
                {
                    Height -= change;
                    panel.Margin = new Thickness(panel.Margin.Left, panel.Margin.Top + change, 0, 0);
                }
            }
        }
        private void OnResizeRight(double horizontalChange)
        {
            var change = Math.Min(-horizontalChange, ActualWidth - MinWidth);
            if (Width - change > MaxWidth)
                change = -(MaxWidth - Width);
            if (!Tips.AreEqual(0.0, change))
                Width -= change;
        }
        private void OnResizeBottom(double verticalChange)
        {
            var change = Math.Min(-verticalChange, ActualHeight - MinHeight);
            if (Height - change > MaxHeight)
                change = -(MaxHeight - Height); 
            if (!Tips.AreEqual(0.0, change))
                Height -= change;
        }
    }
}
