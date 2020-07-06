using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MyWpfMwi.Common;

namespace MyWpfMwi.Mwi
{
    public partial class MwiContainer
    {
        public enum WindowsMenuOptions
        {
            TilesHorizontally,
            TilesVertically,
            Horizontal,
            Vertical,
            Cascade,
            CollapseAllWindows,
            CloseAllWindows
        }

        private void OnWindowsMenuButtonCheckedChange(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            if (button.IsChecked == true)
            {
                Controls.ToggleButtonHelper.OpenMenu_OnCheck(sender);
                var cm = button.Resources.Values.OfType<ContextMenu>().First();

                // Remove old window tab items
                foreach (var a2 in cm.Items.Cast<FrameworkElement>().Where(a => a.DataContext is MwiChild).ToArray())
                    cm.Items.Remove(a2);

                // Add current window tab items
                var index = cm.Items.Cast<FrameworkElement>().TakeWhile(item => item.GetType() != typeof(Separator)).Count();
                foreach (var item in Children)
                {
                    cm.Items.Insert(++index, new MenuItem
                    {
                        Header = item.Title,
                        IsChecked = item.IsSelected,
                        Icon = new Image { Source = item.Icon },
                        DataContext = item,
                        Command = new RelayCommand((p) => item.Activate())
                    });
                }
            }
        }

        private void ExecuteWindowsMenuOption(object menuOption)
        {
            switch ((WindowsMenuOptions)menuOption)
            {
                case WindowsMenuOptions.CollapseAllWindows:
                    foreach (var window in InternalWindows.Where(w => w.WindowState != WindowState.Minimized).OrderBy(w => Panel.GetZIndex(w)).ToArray())
                        window.ToggleMinimize(null);
                    break;
                case WindowsMenuOptions.CloseAllWindows:
                    foreach (var window in InternalWindows.OrderBy(w => Panel.GetZIndex(w)).ToArray())
                        window.Close();
                    break;
                default:
                    ApplyWindowsLayout((WindowsMenuOptions)menuOption);
                    break;
            }
        }

        private void ApplyWindowsLayout(WindowsMenuOptions menuOption)
        {
            var windows = InternalWindows.Where(w => w.WindowState != WindowState.Minimized).ToList();
            if (windows.Count == 0) return;

            foreach (var window in windows.Where(w => w.WindowState != WindowState.Normal))
                window.WindowState = WindowState.Normal;

            var mi = GetType().GetMethod($"ApplyLayout_{menuOption}",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            mi.Invoke(this, new object[] { windows });

            // Update layout before resize animation
            var refreshScrollViewer = ScrollViewer.ScrollableWidth > Tips.SCREEN_TOLERANCE ||
                                      ScrollViewer.ScrollableHeight > Tips.SCREEN_TOLERANCE;
            if (refreshScrollViewer)
            {
                ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
            InvalidateSize();
        }

        private void ApplyLayout_TilesHorizontally(IList<MwiChild> windows)
        {
            var cols = (int)Math.Sqrt(windows.Count);
            var rows = windows.Count / cols;
            var rowCount = new List<int>(); // windows per row
            for (var i = 0; i < rows; i++)
            {
                if (windows.Count % rows > rows - i - 1)
                    rowCount.Add(cols + 1);
                else
                    rowCount.Add(cols);
            }

            var newHeight = InnerHeight / rows;
            var currentRow = 0;
            var currentColumn = 0;
            foreach (var window in windows)
            {
                SetSizeForChild(window, currentColumn * ActualWidth / rowCount[currentRow], currentRow * newHeight, ActualWidth / rowCount[currentRow], newHeight);

                currentColumn++;
                if (currentColumn == rowCount[currentRow])
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }
        }

        private void ApplyLayout_TilesVertically(IList<MwiChild> windows)
        {
            var rows = (int)Math.Sqrt(windows.Count);
            var cols = windows.Count / rows;
            var columnCount = new List<int>(); // windows per column
            for (var i = 0; i < cols; i++)
            {
                if (windows.Count % cols > cols - i - 1)
                    columnCount.Add(rows + 1);
                else
                    columnCount.Add(rows);
            }

            var newWidth = ActualWidth / cols;
            var currentRow = 0;
            var currentColumn = 0;
            foreach (var window in windows)
            {
                SetSizeForChild(window, currentColumn * newWidth, currentRow * InnerHeight / columnCount[currentColumn], newWidth, InnerHeight / columnCount[currentColumn]);

                currentRow++;
                if (currentRow == columnCount[currentColumn])
                {
                    currentRow = 0;
                    currentColumn++;
                }
            }
        }

        private void ApplyLayout_Horizontal(IList<MwiChild> windows)
        {
            var newHeight = InnerHeight / windows.Count;
            for (var i = 0; i < windows.Count; i++)
                SetSizeForChild(windows[i], 0, newHeight * i, ActualWidth, newHeight);
        }

        private void ApplyLayout_Vertical(IList<MwiChild> windows)
        {
            var newWidth = ActualWidth / windows.Count;
            for (var i = 0; i < windows.Count; i++)
                SetSizeForChild(windows[i], newWidth * i, 0, newWidth, InnerHeight);
        }

        private void ApplyLayout_Cascade(IList<MwiChild> windows)
        {
            var newWidth = ActualWidth * 0.58; // should be non-linear formula here
            var newHeight = InnerHeight * 0.67;
            var windowOffset = 0.0;
            foreach (var mwiChild in windows)
            {
                SetSizeForChild(mwiChild, windowOffset, windowOffset, newWidth, newHeight);

                windowOffset += WINDOW_OFFSET_STEP;
                if (windowOffset + mwiChild.Width > ActualWidth || windowOffset + mwiChild.Height > InnerHeight)
                    windowOffset = 0;

                Panel.SetZIndex(mwiChild, MwiUniqueCount++);
            }
            ActiveMwiChild = windows[windows.Count - 1];
        }

        private void SetSizeForChild(MwiChild mwiChild, double newLeft, double newTop, double newWidth, double newHeight)
        {
            var toRectangle = new Rect(newLeft, newTop, mwiChild.Resizable ? newWidth : mwiChild.ActualWidth, mwiChild.Resizable ? newHeight : mwiChild.ActualHeight);
            mwiChild.SetRectangleWithAnimation(toRectangle, () =>
            {
                if (ScrollViewer.HorizontalScrollBarVisibility != ScrollBarVisibility.Auto)
                {
                    ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                }
            });
        }

    }
}
