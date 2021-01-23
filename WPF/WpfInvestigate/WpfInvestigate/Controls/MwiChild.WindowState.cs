using System;
using System.Windows;
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
                var bitmap = new RenderTargetBitmap((int)Math.Round(ActualWidth),
                    (int)Math.Round(ActualHeight), 96, 96, PixelFormats.Default);
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

        private void ToggleDetach(object obj)
        {
            DialogMessage.ShowDialog("need ToDo!");
        }

        #region ==============  OnWindowStateValueChanged  =================
        private static void OnWindowStateValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
    ((MwiChild)sender).WindowStateValueChanged((WindowState)e.NewValue, (WindowState)e.OldValue);
        private void WindowStateValueChanged(WindowState newWindowState, WindowState previousWindowState)
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
                AnimateWindowState(previousWindowState);

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
