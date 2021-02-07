using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiBar.xaml
    /// </summary>
    public partial class MwiBar : INotifyPropertyChanged
    {
        static MwiBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MwiBar), new FrameworkPropertyMetadata(typeof(MwiBar)));
        }

        public MwiBar()
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
        private void TabScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
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

        private void WindowsBar_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var model = SelectedItem;
            var item = ItemContainerGenerator.ContainerFromItem(model) as TabItem;
            var scrollViewer = GetTemplateChild("PART_ScrollBar") as ScrollViewer;
            if (item != null && scrollViewer != null)
            {
                var point = item.TranslatePoint(new Point(), scrollViewer);
                scrollViewer.ScrollToHorizontalOffset(point.X + (item.ActualWidth / 2));
            }

            foreach (var a1 in e.RemovedItems)
                Dispatcher.InvokeAsync(() => AnimateTabButton((TabItem)ItemContainerGenerator.ContainerFromItem(a1)), DispatcherPriority.ContextIdle);

            foreach (var a1 in e.AddedItems)
                Dispatcher.InvokeAsync(() => AnimateTabButton((TabItem)ItemContainerGenerator.ContainerFromItem(a1)), DispatcherPriority.ContextIdle);
        }

        #region ==============  Tab item  ==============
        private async void TabItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ((TabItem)sender).BeginAnimationAsync(OpacityProperty, 0.0, 1.0);
        }

        private void TabItem_OnMouseEnterOrLeave(object sender, MouseEventArgs e)
        {
            AnimateTabButton((TabItem)sender);
        }

        private void TabItem_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var resizingControl = ((FrameworkElement)sender).DataContext as ResizingControl;
            var element = (FrameworkElement)Mouse.DirectlyOver;
            while (element != null && element.Name != "DeleteTabButton")
                element = VisualTreeHelper.GetParent(element) as FrameworkElement;

            if (element != null) // delete button was pressed
                //mwiChild.CmdClose.Execute(null);
            {
                if (resizingControl is MwiChild mwiChild)
                    mwiChild.CmdClose.Execute(null);
                else if (resizingControl is MwiItem mwiItem)
                    mwiItem.CmdClose.Execute(null);
                else if (ItemsSource is IList list)
                    list.Remove(((FrameworkElement)sender).DataContext);
            }
            else
                resizingControl?.Activate();

            e.Handled = true;
        }

        private void TabItem_OnToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is MwiChild mwiChild)
                mwiChild.RefreshThumbnail();
        }

        private void TabToolTip_OnOpened(object sender, RoutedEventArgs e)
        {
            var toolTip = (ToolTip)sender;
            var tabTextBlock = Tips.GetVisualChildren(toolTip.PlacementTarget).OfType<TextBlock>().First();
            toolTip.Tag = Tips.IsTextTrimmed(tabTextBlock) ? "1" : null;
        }

        private void AnimateTabButton(TabItem tabItem)
        {
            if (tabItem == null) // For VS designer
                return;

            LinearGradientBrush newBrush;
            // var dc = tabItem.DataContext as MwiChild;
            // if (dc.IsSelected)
            if (tabItem.IsSelected)
                newBrush = (LinearGradientBrush)FindResource("Mwi.WindowTab.Selected.BackgroundBrush");
            else if (tabItem.IsMouseOver)
                newBrush = (LinearGradientBrush)FindResource("Mwi.WindowTab.MouseOver.BackgroundBrush");
            else
                newBrush = (LinearGradientBrush)FindResource("Mwi.WindowTab.BackgroundBrush");

            tabItem.Background = AnimationHelper.BeginLinearGradientBrushAnimation(newBrush, (LinearGradientBrush)tabItem.Background);
        }
        #endregion

        #region ===========  INotifyPropertyChanged  ==============
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
