using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MyWpfMwi.Common;

namespace MyWpfMwi.Mwi
{
    /// <summary>
    /// Interaction logic for WindowsBar.xaml
    /// </summary>
    public partial class WindowsBar : INotifyPropertyChanged
    {
        static WindowsBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowsBar), new FrameworkPropertyMetadata(typeof(WindowsBar)));
        }

        public WindowsBar()
        {
            InitializeComponent();
        }

        public bool CanScrollLeft => _scrollableWidth >= Tips.SCREEN_TOLERANCE && !Tips.AreEqual(_horizontalOffset, 0);
        public bool CanScrollRight => _scrollableWidth >= Tips.SCREEN_TOLERANCE && !Tips.AreEqual(_horizontalOffset + _viewportWidth, _extentWidth);
        public Visibility ScrollButtonVisibility => _scrollableWidth < Tips.SCREEN_TOLERANCE ? Visibility.Collapsed : Visibility.Visible;

        private double _scrollableWidth;
        private double _viewportWidth;
        private double _extentWidth;
        private double _horizontalOffset;
        private void TabScrollViewer_OnScrollChanged(object sender, EventArgs e)
        {
            var newScrollableWidth = ((ScrollViewer)sender).ScrollableWidth;
            var newViewportWidth = ((ScrollViewer)sender).ViewportWidth;
            var newExtentWidth = ((ScrollViewer)sender).ExtentWidth;
            var newHorizontalOffset = ((ScrollViewer)sender).HorizontalOffset;
            if (!Tips.AreEqual(_scrollableWidth, newScrollableWidth) || !Tips.AreEqual(_viewportWidth, newViewportWidth) || !Tips.AreEqual(_extentWidth, newExtentWidth) || !Tips.AreEqual(_horizontalOffset, newHorizontalOffset))
            {
                var oldCanScrollLeft = CanScrollLeft;
                var oldCanScrollRight = CanScrollRight;
                _scrollableWidth = newScrollableWidth;
                _viewportWidth = newViewportWidth;
                _extentWidth = newExtentWidth;
                _horizontalOffset = newHorizontalOffset;
                if (oldCanScrollLeft != CanScrollLeft || oldCanScrollRight != CanScrollRight)
                    OnPropertiesChanged(nameof(CanScrollLeft), nameof(CanScrollRight), nameof(ScrollButtonVisibility));
            }
        }

        private void TabItem_OnMouseEnterOrLeave(object sender, MouseEventArgs e)
        {
            AnimateTabButton((TabItem)sender);
        }

        private void WindowsBar_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var model = base.SelectedItem;
            var item = ItemContainerGenerator.ContainerFromItem(model) as TabItem;
            var scrollViewer = GetTemplateChild("PART_ScrollBar") as ScrollViewer;
            if (item != null && scrollViewer != null)
            {
                var point = item.TranslatePoint(new Point(), scrollViewer);
                scrollViewer.ScrollToHorizontalOffset(point.X + (item.ActualWidth / 2));
            }

            foreach (var a1 in e.RemovedItems)
                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
                    new Action(() => AnimateTabButton((TabItem)ItemContainerGenerator.ContainerFromItem(a1))));

            foreach (var a1 in e.AddedItems)
                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
                    new Action(() => AnimateTabButton((TabItem)ItemContainerGenerator.ContainerFromItem(a1))));
        }

        private void TabItem_OnToolTipOpening(object sender, ToolTipEventArgs e)
        {
            ((MwiChild)((FrameworkElement)sender).DataContext).RefreshThumbnail();
        }

        private void TabItem_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mwiChild = ((FrameworkElement)sender).DataContext as MwiChild;
            var element = (FrameworkElement)Mouse.DirectlyOver;
            while (element != null && element.Name != "DeleteTabButton")
                element = VisualTreeHelper.GetParent(element) as FrameworkElement;

            if (element != null) // delete button was pressed
                mwiChild.CmdClose.Execute(null);
            else
                mwiChild.Activate();

            e.Handled = true;
        }

        private void TabItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var storyboard = new Storyboard();
            storyboard.Children.Add(AnimationHelper.GetOpacityAnimation((TabItem)sender, 0, 1, FillBehavior.Stop));
            storyboard.Begin();
        }

        private void TabToolTip_OnOpened(object sender, RoutedEventArgs e)
        {
            var toolTip = (ToolTip)sender;
            var tabTextBlock = Tips.GetVisualChildren(toolTip.PlacementTarget).First(s=> s is TextBlock) as TextBlock;
            toolTip.Tag = Tips.IsTextTrimmed(tabTextBlock) ? "1" : null;
        }

        private void AnimateTabButton(TabItem tabItem)
        {
            if (tabItem == null) // For VS designer
                return;

            LinearGradientBrush newBrush;
            var dc = tabItem.DataContext as MwiChild;
            if (dc.IsSelected)
                newBrush = (LinearGradientBrush)FindResource("Mwi.WindowTab.Selected.BackgroundBrush");
            else if (tabItem.IsMouseOver)
                newBrush = (LinearGradientBrush)FindResource("Mwi.WindowTab.MouseOver.BackgroundBrush");
            else
                newBrush = (LinearGradientBrush)FindResource("Mwi.WindowTab.BackgroundBrush");

            tabItem.Background = AnimationHelper.RunLinearGradientBrushAnimation(newBrush, (LinearGradientBrush)tabItem.Background);
        }

        //============================================================
        //===========  INotifyPropertyChanged  =======================
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
