using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Movable.Common;

namespace WpfInvestigate.Samples
{
    public partial class SampleDialogMovable : UserControl
    {
        private static int Unique = 0;
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title",
            typeof(string), typeof(SampleDialogMovable), new PropertyMetadata((Unique++).ToString()));


        //========================
        private Panel HostPanel => VisualTreeHelper.GetParent(this) as Panel;

        public SampleDialogMovable()
        {
            InitializeComponent();
            DataContext = this;
        }

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

        private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            if (Focusable)
                Focus();
            // e.Handled = true;
        }

        private void GridMoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var grid = HostPanel as Grid;
            if (grid != null)
            {
                var newX = Margin.Left + e.HorizontalChange;
                //if (newX + ActualWidth > HostPanel.ActualWidth)
                  //  newX = HostPanel.ActualWidth - ActualWidth;
                //if (newX < 0) newX = 0;

                var newY = Margin.Top + e.VerticalChange;
                //if (newY + ActualHeight > HostPanel.ActualHeight)
                  //  newY = HostPanel.ActualHeight - ActualHeight;
                // if (newY < 0) newY = 0;

                Margin = new Thickness { Left = newX, Top = newY };
                e.Handled = true;
            }
        }

        private void CanvasMoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var canvas = HostPanel as Canvas;
            if (canvas != null)
            {
                // var newX = Canvas.GetLeft(this) + e.HorizontalChange; // GetLeft/Top may be NaN
                var p = TranslatePoint(new Point(0, 0), canvas);
                var newX = p.X + e.HorizontalChange;
                //if (newX + ActualWidth > HostPanel.ActualWidth)
                  //  newX = HostPanel.ActualWidth - ActualWidth;
                //if (newX < 0) newX = 0;

                // var newY = Canvas.GetTop(this) + e.VerticalChange;
                var newY = p.Y + e.VerticalChange;
                //if (newY + ActualHeight > HostPanel.ActualHeight)
                  //  newY = HostPanel.ActualHeight - ActualHeight;
                //if (newY < 0) newY = 0;

                Canvas.SetLeft(this, newX);
                Canvas.SetTop(this, newY);
                e.Handled = true;
            }
        }

        private void MoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var aa1 = Tips.GetVisualParents(this).OfType<FrameworkElement>().ToArray();
            var aa2 = Tips.GetVisualParents(this).OfType<FrameworkElement>().Select(a => new Size(a.ActualWidth, a.ActualHeight)).ToArray();

            /*var itemsPresenter = Tips.GetVisualParents(this).OfType<ItemsPresenter>().FirstOrDefault();
            var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
            if (itemsPresenter != null && container != null)
            {
                var newX = itemsPresenter.Margin.Left + e.HorizontalChange;
                if (newX + ActualWidth > container.ActualWidth)
                    newX = container.ActualWidth - ActualWidth;
                if (newX < 0) newX = 0;

                var newY = itemsPresenter.Margin.Top + e.VerticalChange;
                if (newY + ActualHeight > container.ActualHeight)
                    newY = container.ActualHeight - ActualHeight;
                if (newY < 0) newY = 0;

                itemsPresenter.Margin = new Thickness { Left = newX, Top = newY };
                e.Handled = true;
            }
            else */if (HostPanel is Grid)
                GridMoveThumb_OnDragDelta(sender, e);
            else if (HostPanel is Canvas)
                CanvasMoveThumb_OnDragDelta(sender, e);
        }

        private void XXMoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var aa1 = Tips.GetVisualParents(this).OfType<FrameworkElement>().ToArray();
            var aa2 = Tips.GetVisualParents(this).OfType<FrameworkElement>().Select(a => new Size(a.ActualWidth, a.ActualHeight)).ToArray();

            var itemsPresenter = Tips.GetVisualParents(this).OfType<ItemsPresenter>().FirstOrDefault();
            var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
            if (itemsPresenter != null && container != null)
            {
                var newX = itemsPresenter.Margin.Left + e.HorizontalChange;
                if (newX + ActualWidth > container.ActualWidth)
                    newX = container.ActualWidth - ActualWidth;
                if (newX < 0) newX = 0;

                var newY = itemsPresenter.Margin.Top + e.VerticalChange;
                if (newY + ActualHeight > container.ActualHeight)
                    newY = container.ActualHeight - ActualHeight;
                if (newY < 0) newY = 0;

                itemsPresenter.Margin = new Thickness { Left = newX, Top = newY };
            }
            e.Handled = true;
        }

        private void ResizeThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;
            var itemsPresenter = Tips.GetVisualParents(this).OfType<ItemsPresenter>().FirstOrDefault();

            if (thumb.HorizontalAlignment == HorizontalAlignment.Left)
                OnResizeLeft(e.HorizontalChange, itemsPresenter);
            else if (thumb.HorizontalAlignment == HorizontalAlignment.Right)
                OnResizeRight(e.HorizontalChange, itemsPresenter);

            if (thumb.VerticalAlignment == VerticalAlignment.Top)
                OnResizeTop(e.VerticalChange, itemsPresenter);
            else if (thumb.VerticalAlignment == VerticalAlignment.Bottom)
                OnResizeBottom(e.VerticalChange, itemsPresenter);

            e.Handled = true;
        }

        private void OnResizeLeft(double horizontalChange, FrameworkElement itemsPresenter)
        {
            if (itemsPresenter != null)
            {
                var change = Math.Min(horizontalChange, ActualWidth - MinWidth);
                if (itemsPresenter.Margin.Left + change < 0)
                    change = -itemsPresenter.Margin.Left;
                if ((ActualWidth - change) > MaxWidth)
                    change = ActualWidth - MaxWidth;

                if (!Tips.AreEqual(0.0, change))
                {
                    Width = ActualWidth - change;
                    itemsPresenter.Margin = new Thickness(itemsPresenter.Margin.Left + change, itemsPresenter.Margin.Top, 0, 0);
                }
            }
        }

        private void OnResizeTop(double verticalChange, FrameworkElement itemsPresenter)
        {
            if (itemsPresenter != null)
            {
                var change = Math.Min(verticalChange, ActualHeight - MinHeight);
                if (itemsPresenter.Margin.Top + change < 0)
                    change = -itemsPresenter.Margin.Top;
                if ((Height - change) > MaxHeight)
                    change = Height - MaxHeight;

                if (!Tips.AreEqual(0.0, change))
                {
                    Height = ActualHeight - change;
                    itemsPresenter.Margin = new Thickness(itemsPresenter.Margin.Left, itemsPresenter.Margin.Top + change, 0, 0);
                }
            }
        }
        private void OnResizeRight(double horizontalChange, FrameworkElement itemsPresenter)
        {
            var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
            var change = Math.Min(-horizontalChange, ActualWidth - MinWidth);
            
            if ((ActualWidth - change) > MaxWidth)
                change = ActualWidth - MaxWidth;
            if (container != null && (itemsPresenter.Margin.Left + ActualWidth - change) > container.ActualWidth)
                change = itemsPresenter.Margin.Left + ActualWidth - container.ActualWidth;

            if (!Tips.AreEqual(0.0, change))
                Width = ActualWidth - change;
        }
        private void OnResizeBottom(double verticalChange, FrameworkElement itemsPresenter)
        {
            var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
            var change = Math.Min(-verticalChange, ActualHeight - MinHeight);

            if ((ActualHeight - change) > MaxHeight)
                change = ActualHeight - MaxHeight;
            if (container != null && (itemsPresenter.Margin.Top + ActualHeight - change) > container.ActualHeight)
                change = itemsPresenter.Margin.Top + ActualHeight - container.ActualHeight;

            if (!Tips.AreEqual(0.0, change))
                Height = ActualHeight - change;
        }
    }
}
