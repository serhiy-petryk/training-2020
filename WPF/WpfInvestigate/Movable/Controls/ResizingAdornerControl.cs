﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Movable.Common;

namespace Movable.Controls
{
    public class ResizingAdornerControl : UserControl
    {
        private static int Unique = 1;
        private FrameworkElement _adornedElement;
        private FrameworkElement Host => VisualTreeHelper.GetParent(_adornedElement) as FrameworkElement;
        public bool LimitPositionToPanelBounds { get; set; } = false;

        public ResizingAdornerControl(FrameworkElement adornedElement)
        {
            _adornedElement = adornedElement;
            DataContext = this;

            /*var currentWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (currentWindow != null)
                MaxWidth = Math.Max(400, currentWindow.ActualWidth / 2);*/
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            foreach (var thumb in Tips.GetVisualChildren(this).OfType<Thumb>())
            {
                thumb.DragStarted += Thumb_OnDragStarted;
                if (thumb.Name == "PART_HeaderMover")
                {
                    thumb.DragDelta += MoveThumb_OnDragDelta;
                    // thumb.PreviewMouseDoubleClick += ThumbOnPreviewMouseDoubleClick;
                }
                else
                    thumb.DragDelta += ResizeThumb_OnDragDelta;
            }

            var sv = Tips.GetVisualParents(Host).OfType<ScrollViewer>().FirstOrDefault();
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
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (_adornedElement.Focusable)
            {
                _adornedElement.Focus();
                Panel.SetZIndex(Host is Panel ? _adornedElement : Host, Unique++);
            }
        }
        #endregion

        #region ==========  Moving && resizing  =========
        private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            if (_adornedElement.Focusable)
                _adornedElement.Focus();
            e.Handled = true;
        }

        private void MoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var mousePosition = Mouse.GetPosition(Host);
            if (mousePosition.X < 0 || mousePosition.Y < 0) return;

            var oldPosition = _adornedElement.TranslatePoint(new Point(0, 0), Host);
            var newX = oldPosition.X + e.HorizontalChange;

            if (LimitPositionToPanelBounds)
            {
                if (newX + _adornedElement.ActualWidth > Host.ActualWidth)
                    newX = Host.ActualWidth - _adornedElement.ActualWidth;
                if (newX < 0) newX = 0;
            }

            var newY = oldPosition.Y + e.VerticalChange;
            if (LimitPositionToPanelBounds)
            {
                if (newY + _adornedElement.ActualHeight > Host.ActualHeight)
                    newY = Host.ActualHeight - _adornedElement.ActualHeight;
                if (newY < 0) newY = 0;
            }

            if (Host is Canvas)
            {
                if (!Tips.AreEqual(oldPosition.X, newX)) Canvas.SetLeft(_adornedElement, newX);
                if (!Tips.AreEqual(oldPosition.Y, newY)) Canvas.SetTop(_adornedElement, newY);
            }
            else
                _adornedElement.Margin = new Thickness { Left = newX, Top = newY };

            e.Handled = true;
            var sv = Tips.GetVisualParents(Host).OfType<ScrollViewer>().FirstOrDefault();
            if (sv != null)
            {
                // Smooth scrolling
                if (Math.Abs(e.HorizontalChange) > Tips.SCREEN_TOLERANCE && (newX + _adornedElement.ActualWidth) > sv.ActualWidth)
                    sv.ScrollToHorizontalOffset(Math.Max(0, sv.HorizontalOffset + e.HorizontalChange * 0.5));
                if (Math.Abs(e.VerticalChange) > Tips.SCREEN_TOLERANCE && (newY + _adornedElement.ActualHeight) > sv.ActualHeight)
                    sv.ScrollToVerticalOffset(Math.Max(0, sv.VerticalOffset + e.VerticalChange * 0.5));
            }
        }

        private void ResizeThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var mousePosition = Mouse.GetPosition(Host);
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
            _adornedElement.BringIntoView();
        }

        private void OnResizeLeft(double horizontalChange)
        {
            var oldPosition = _adornedElement.TranslatePoint(new Point(0, 0), Host);
            var change = Math.Min(horizontalChange, _adornedElement.ActualWidth - _adornedElement.MinWidth);

            if (LimitPositionToPanelBounds)
            {
                if (oldPosition.X + change < 0)
                    change = -oldPosition.X;
                if ((_adornedElement.ActualWidth - change) > _adornedElement.MaxWidth)
                    change = _adornedElement.ActualWidth - _adornedElement.MaxWidth;
            }

            if (!Tips.AreEqual(0.0, change))
            {
                _adornedElement.Width = _adornedElement.ActualWidth - change;
                if (Host is Canvas)
                    Canvas.SetLeft(_adornedElement, oldPosition.X + change);
                else
                    _adornedElement.Margin = new Thickness(oldPosition.X + change, oldPosition.Y, 0, 0);
            }
        }
        private void OnResizeTop(double verticalChange)
        {
            var oldPosition = _adornedElement.TranslatePoint(new Point(0, 0), Host);
            var change = Math.Min(verticalChange, _adornedElement.ActualHeight - _adornedElement.MinHeight);

            if (LimitPositionToPanelBounds)
            {
                if (oldPosition.Y + change < 0)
                    change = -oldPosition.Y;
                if ((_adornedElement.Height - change) > _adornedElement.MaxHeight)
                    change = _adornedElement.Height - _adornedElement.MaxHeight;
            }

            if (!Tips.AreEqual(0.0, change))
            {
                _adornedElement.Height = _adornedElement.ActualHeight - change;
                if (Host is Canvas)
                    Canvas.SetTop(_adornedElement, oldPosition.Y + change);
                else
                    _adornedElement.Margin = new Thickness(oldPosition.X, oldPosition.Y + change, 0, 0);
            }
        }
        private void OnResizeRight(double horizontalChange)
        {
            var change = Math.Min(-horizontalChange, _adornedElement.ActualWidth - _adornedElement.MinWidth);

            if (LimitPositionToPanelBounds)
            {
                var oldPosition = _adornedElement.TranslatePoint(new Point(0, 0), Host);
                if ((_adornedElement.ActualWidth - change) > _adornedElement.MaxWidth)
                    change = _adornedElement.ActualWidth - _adornedElement.MaxWidth;
                if ((oldPosition.X + _adornedElement.ActualWidth - change) > Host.ActualWidth)
                    change = oldPosition.X + _adornedElement.ActualWidth - Host.ActualWidth;
            }

            if (!Tips.AreEqual(0.0, change))
                _adornedElement.Width = _adornedElement.ActualWidth - change;
        }
        private void OnResizeBottom(double verticalChange)
        {
            var change = Math.Min(-verticalChange, _adornedElement.ActualHeight - _adornedElement.MinHeight);

            if (LimitPositionToPanelBounds)
            {
                var oldPosition = _adornedElement.TranslatePoint(new Point(0, 0), Host);
                if ((_adornedElement.ActualHeight - change) > _adornedElement.MaxHeight)
                    change = _adornedElement.ActualHeight - _adornedElement.MaxHeight;
                if ((oldPosition.Y + _adornedElement.ActualHeight - change) > Host.ActualHeight)
                    change = oldPosition.Y + _adornedElement.ActualHeight - Host.ActualHeight;
            }

            if (!Tips.AreEqual(0.0, change))
                _adornedElement.Height = _adornedElement.ActualHeight - change;
        }
        #endregion

        /*#region ==========  Properties  ===============
        public static readonly DependencyProperty DragThumbProperty =
            DependencyProperty.Register("DragThumb", typeof(Thumb), typeof(ResizableControl), new PropertyMetadata(null, OnDragThumbChanged));

        public Thumb DragThumb
        {
            get => (Thumb)GetValue(DragThumbProperty);
            set => SetValue(DragThumbProperty, value);
        }

        private static void OnDragThumbChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ResizableControl control)
            {
                if (e.OldValue is Thumb oldThumb)
                    oldThumb.DragDelta -= control.MoveThumb_OnDragDelta;
                if (e.NewValue is Thumb newThumb)
                    newThumb.DragDelta += control.MoveThumb_OnDragDelta;
            }
        }

        #endregion*/

    }
}
