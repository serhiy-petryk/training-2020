using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfInvestigate.Common;

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

        private void ToggleMinimize(object obj)
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
        private ImageSource CreateThumbnail()
        {
            if (WindowState != WindowState.Minimized || _thumbnailCache == null)
            {
                var bitmap = new RenderTargetBitmap((int)Math.Round(ActualWidth), (int)Math.Round(ActualHeight),
                    96, 96, PixelFormats.Default);
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    var brush = new VisualBrush(this);
                    context.DrawRectangle(brush, null,
                        new Rect(new Point(), new Size(ActualWidth, ActualHeight)));
                    context.Close();
                }

                bitmap.Render(drawingVisual);
                _thumbnailCache = bitmap;
            }
            return _thumbnailCache;
        }

        private async void ToggleDetach(object obj)
        {
            if (WindowState == WindowState.Normal)
                SaveActualRectangle();

            if (IsWindowed)
            {
                await AnimateHide();
                var host = (Window)Parent;
                host.Close();
                ((Window)Parent).Content = null;
                MwiContainer.MwiPanel.Children.Add(this);
                // Focused = true;

                if (WindowState == WindowState.Maximized)
                {
                    Position = new Point(0, 0);
                    Width = MwiContainer.ActualWidth;
                    Height = MwiContainer.ActualHeight;
                }
                else
                    Position = _attachedPosition;

                OnWindowStateValueChanged(this, new DependencyPropertyChangedEventArgs(WindowStateProperty, this.WindowState, WindowState));
                OnPropertiesChanged(nameof(IsWindowed));

                await this.BeginAnimationAsync(OpacityProperty, 0.0, 1.0);
            }
            else
            {
                    var window = new Window
                    {
                        Style = (Style)FindResource("HeadlessWindow"),
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Background = Brushes.LightBlue,
                        Opacity = 1
                    };

                    await AnimateHide();
                    Margin = new Thickness(0,0,-1,-1);
                    MwiContainer.MwiPanel.Children.Remove(this);
                    window.Content = this;
                    RestoreExternalWindowRect();

                    /*window.Activated += (sender, args) =>
                    {
                        if (MwiContainer.ActiveMwiChild != this)
                            MwiContainer.ActiveMwiChild = this;
                        else
                            Focused = true;
                    };
                    window.Deactivated += (sender, args) =>
                    {
                        if (!MwiContainer.WindowShowLock)
                            Focused = false;
                    };*/

                    MwiContainer.WindowShowLock = true;
                    window.Show();
                    MwiContainer.WindowShowLock = false;

                    // Refresh ScrollBar scrolling
                    MwiContainer.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    MwiContainer.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                    OnWindowStateValueChanged(this,
                        new DependencyPropertyChangedEventArgs(WindowStateProperty, this.WindowState,
                            WindowState));
                    OnPropertiesChanged(nameof(IsWindowed));

                    await this.BeginAnimationAsync(OpacityProperty, 0.0, 1.0);
            }
        }

        public void RestoreExternalWindowRect(Size? newSize = null)
        {
            var window = (Window)Parent;
            if (window == null)
                return;

            if (newSize.HasValue)
                _lastNormalSize = newSize.Value;

            Position = _detachedPosition; // new Point(window.Left, window.Top);
            var maximizedWindowRectangle = Tips.GetMaximizedWindowRectangle();

            _detachedPosition = new Point(
                Math.Max(0, maximizedWindowRectangle.X + (maximizedWindowRectangle.Width - _lastNormalSize.Width) / 2),
                Math.Max(0, maximizedWindowRectangle.Y + (maximizedWindowRectangle.Height - _lastNormalSize.Height) / 2));

            if (WindowState == WindowState.Maximized)
            {
                window.Left = maximizedWindowRectangle.Left;
                window.Top = maximizedWindowRectangle.Top;
                Width = maximizedWindowRectangle.Width;
                Height = maximizedWindowRectangle.Height;
            }
            else
            {
                Width = _lastNormalSize.Width;
                Height = _lastNormalSize.Height;
                Position = _detachedPosition;
            }
        }

        #region ==============  OnWindowStateValueChanged  =================
        private static void OnWindowStateValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            ((MwiChild)sender).WindowStateValueChanged((WindowState)e.NewValue, (WindowState)e.OldValue);
        private async void WindowStateValueChanged(WindowState newWindowState, WindowState previousWindowState)
        {
            var isDetachEvent = previousWindowState == newWindowState;

            if (previousWindowState == WindowState.Normal && !isDetachEvent)
                SaveActualRectangle();

            if (IsWindowed)
            {
                if (newWindowState == WindowState.Normal)
                {
                    Width = _lastNormalSize.Width;
                    Height = _lastNormalSize.Height;
                    Position = new Point(_detachedPosition.X, _detachedPosition.Y);
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

            /* todo: container if (!IsWindowed || isDetachEvent)
            {
                Container?.InvalidateSize();
                Container?.OnPropertiesChanged(nameof(MwiContainer.ScrollBarKind));
            }*/

            // Activate main window (in case of attach)
            if (IsWindowed && !((Window)Parent).IsFocused)
                ((Window)Parent).Focus();
            else if (!IsWindowed && !Window.GetWindow(this).IsFocused)
                Window.GetWindow(this)?.Focus();
        }
        private void SaveActualRectangle()
        {
            _lastNormalSize = new Size(ActualWidth, ActualHeight);
            SaveActualPosition();
        }
        private void SaveActualPosition()
        {
            if (IsWindowed)
                _detachedPosition = new Point(((Window)Parent).Left, ((Window)Parent).Top);
            else
                _attachedPosition = new Point(Position.X, Position.Y);
        }
        #endregion
    }
}
