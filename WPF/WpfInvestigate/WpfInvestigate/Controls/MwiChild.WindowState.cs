using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfInvestigate.Common;
using WpfInvestigate.ViewModels;

namespace WpfInvestigate.Controls
{
    public partial class MwiChild
    {
        //  ===============  MwiChild State ===============
        private Size _lastNormalSize;
        private Point _attachedPosition;
        private Point _detachedPosition;
        private WindowState? _beforeMinimizedState { get; set; } // Previous state of minimized window.
        private ImageSource _thumbnailCache;

        //==========================
        private void ToggleMaximize(object p) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        internal void ToggleMinimize(object obj)
        {
            if (WindowState != WindowState.Minimized)
                CreateThumbnail();

            if (IsWindowed)
            {
                if (((Window)Parent).WindowState == WindowState.Minimized)
                    ((Window)Parent).WindowState = _beforeMinimizedState ?? WindowState.Normal;
                else
                {
                    _beforeMinimizedState = ((Window)Parent).WindowState;
                    ((Window)Parent).WindowState = WindowState.Minimized;
                }
            }
            else if (WindowState == WindowState.Minimized)
            {
                if (Visibility != Visibility.Visible)
                    Visibility = Visibility.Visible;
                WindowState = _beforeMinimizedState ?? WindowState.Normal;
            }
            else
            {
                _beforeMinimizedState = WindowState;
                WindowState = WindowState.Minimized;
            }
        }

        private async void ToggleDetach(object obj)
        {
            if (MwiContainer == null) return;

            if (WindowState == WindowState.Normal) SaveActualRectangle();

            var contentHeight = GetTemplateChild("ContentBorder") is Border contentBorder1 ? contentBorder1.ActualHeight : ActualHeight;
            var headerSize = GetTemplateChild("ContentBorder") is Border contentBorder ? new Size(ActualWidth - contentBorder.ActualWidth,  ActualHeight - contentBorder.ActualHeight) : new Size(0,0);
            if (IsWindowed)
            {
                await AnimateHide();
                var wnd = (Window)Parent;
                wnd.Close();
                wnd.Content = null;
                if (WindowState == WindowState.Normal)
                {
                    // Width = ActualWidth / MwiAppViewModel.Instance.ScaleValue;
                    // Height = ActualHeight - contentHeight * MwiAppViewModel.Instance.ScaleValue + contentHeight;
                    Width = ActualWidth - (ActualWidth - headerSize.Width) * MwiAppViewModel.Instance.ScaleValue + ActualWidth - headerSize.Width;
                    Height = ActualHeight - (ActualHeight - headerSize.Height) * MwiAppViewModel.Instance.ScaleValue + ActualHeight - headerSize.Height;
                }

                MwiContainer.MwiPanel.Children.Add(this);

                Activate();
                Position = WindowState == WindowState.Maximized ? new Point(0, 0) : _attachedPosition;
            }
            else
            {
                await AnimateHide();

                Margin = new Thickness(0, 0, -1, -1);
                MwiContainer.MwiPanel.Children.Remove(this);

                var wnd = new Window { Style = (Style)FindResource("HeadlessWindow"), Content = this };
                var lastSize = WindowState == WindowState.Normal ? new Size(ActualWidth, ActualHeight) : _lastNormalSize;
                var wndWidth = headerSize.Width + (lastSize.Width - headerSize.Width) * MwiAppViewModel.Instance.ScaleValue;
                var wndHeight = headerSize.Height + (lastSize.Height - headerSize.Height) * MwiAppViewModel.Instance.ScaleValue;
                RestoreExternalWindowRect(new Size(wndWidth, wndHeight));

                wnd.Show();
                Activate();

                // Refresh ScrollBar scrolling
                MwiContainer.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                MwiContainer.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                Position = new Point(-1, -1); // Reset position (need for maximized window or when positions before and after detach are equal)
            }

            OnWindowStateValueChanged(this, new DependencyPropertyChangedEventArgs(WindowStateProperty, WindowState, WindowState));
            OnPropertiesChanged(nameof(IsWindowed));
            BeginAnimation(OpacityProperty, new DoubleAnimation(0.0, 1.0, AnimationHelper.AnimationDuration));
        }

        private void RestoreExternalWindowRect(Size? newSize = null)
        {
            if (!IsWindowed) return;

            if (newSize.HasValue) // Restore from Settings
                _lastNormalSize = newSize.Value;

            var maximizedWindowRectangle = Tips.GetMaximizedWindowRectangle();
            _detachedPosition = new Point(
                Math.Max(0, maximizedWindowRectangle.X + (maximizedWindowRectangle.Width - _lastNormalSize.Width * MwiAppViewModel.Instance.ScaleValue) / 2),
                Math.Max(0, maximizedWindowRectangle.Y + (maximizedWindowRectangle.Height - _lastNormalSize.Height * MwiAppViewModel.Instance.ScaleValue) / 2));
        }

        #region ==============  OnWindowStateValueChanged  =================
        private static void OnWindowStateValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            ((MwiChild)sender).WindowStateValueChanged((WindowState)e.NewValue, (WindowState)e.OldValue);
        private async void WindowStateValueChanged(WindowState newWindowState, WindowState previousWindowState)
        {
            if (previousWindowState == WindowState.Maximized)
            {
                BindingOperations.ClearBinding(this, WidthProperty);
                BindingOperations.ClearBinding(this, HeightProperty);
            }

            var isDetachEvent = previousWindowState == newWindowState;

            if (previousWindowState == WindowState.Normal && !isDetachEvent)
                SaveActualRectangle();

            if (IsWindowed)
            {
                if (newWindowState == WindowState.Normal)
                {
                    Width = _lastNormalSize.Width;
                    Height = _lastNormalSize.Height;
                    Position = _detachedPosition;
                }

                if (newWindowState != WindowState.Minimized && ((Window)Parent).WindowState != WindowState.Normal)
                    ((Window)Parent).WindowState = WindowState.Normal;

                if (newWindowState == WindowState.Maximized)
                {
                    var maximizedWindowRectangle = Tips.GetMaximizedWindowRectangle();
                    Position = new Point(maximizedWindowRectangle.Left, maximizedWindowRectangle.Top);
                    Width = maximizedWindowRectangle.Width;
                    Height = maximizedWindowRectangle.Height;
                }
            }

            if (!IsWindowed && !isDetachEvent)
                await AnimateWindowState(previousWindowState);

            if (!IsWindowed && newWindowState == WindowState.Maximized)
            {
                SetBinding(WidthProperty, new Binding("ActualWidth") { Source = HostPanel });
                SetBinding(HeightProperty, new Binding("ActualHeight") { Source = HostPanel });
            }

            MwiContainer?.InvalidateLayout();
        }

        private void SaveActualRectangle()
        {
            _lastNormalSize = new Size(ActualWidth, ActualHeight);
            SaveActualPosition();
        }
        private void SaveActualPosition()
        {
            if (IsWindowed)
                _detachedPosition = ActualPosition;
            else
                _attachedPosition = ActualPosition;
        }
        #endregion
    }
}
