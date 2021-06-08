using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Helpers;

namespace WpfSpLib.Controls
{
    /// <summary>
    /// Interaction logic for MwiBar.xaml
    /// </summary>
    public class MwiBar : TabControl, INotifyPropertyChanged
    {
        static MwiBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MwiBar), new FrameworkPropertyMetadata(typeof(MwiBar)));
        }

        public MwiBar()
        {
            Unloaded += OnUnloaded;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _doubleButtonGrid = GetTemplateChild("DoubleButtonGrid") as Grid;
            if (GetTemplateChild("PART_ScrollViewer") is ScrollViewer scrollViewer)
                scrollViewer.ScrollChanged += TabScrollViewer_OnScrollChanged;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            var model = SelectedItem;
            var item = ItemContainerGenerator.ContainerFromItem(model) as TabItem;
            var scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
            if (item != null && scrollViewer != null)
            {
                var point = item.TranslatePoint(new Point(), scrollViewer);
                scrollViewer.ScrollToHorizontalOffset(point.X + (item.ActualWidth / 2));
            }

            foreach (var a1 in e.RemovedItems)
                Dispatcher.InvokeAsync(() => AnimateTabButton((TabItem) ItemContainerGenerator.ContainerFromItem(a1)), DispatcherPriority.ContextIdle);

            foreach (var a1 in e.AddedItems)
                Dispatcher.InvokeAsync(() => AnimateTabButton((TabItem) ItemContainerGenerator.ContainerFromItem(a1)), DispatcherPriority.ContextIdle);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems)
                            TabItem_AttachEvents(ItemContainerGenerator.ContainerFromItem(item) as TabItem, false);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (var item in Items)
                            TabItem_AttachEvents(ItemContainerGenerator.ContainerFromItem(item) as TabItem, false);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        break;
                    default: throw new Exception("Please, check code");
                }
            }), DispatcherPriority.Render);
        }

        public bool CanScrollLeft => _scrollableWidth >= Tips.SCREEN_TOLERANCE && !Tips.AreEqual(_horizontalOffset, 0);
        public bool CanScrollRight => _scrollableWidth >= Tips.SCREEN_TOLERANCE && !Tips.AreEqual(_horizontalOffset + _viewportWidth, _extentWidth);
        public Visibility ScrollButtonVisibility =>
            _scrollableWidth < (Tips.SCREEN_TOLERANCE + (_doubleButtonGrid.IsVisible ? _doubleButtonGrid.ActualWidth + _doubleButtonGrid.Margin.Left + _doubleButtonGrid.Margin.Right : 0))
                ? Visibility.Collapsed : Visibility.Visible;

        private Grid _doubleButtonGrid;
        private double _scrollableWidth;
        private double _viewportWidth;
        private double _extentWidth;
        private double _horizontalOffset;

        private void TabScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var newScrollableWidth = ((ScrollViewer) sender).ScrollableWidth;
            var newViewportWidth = ((ScrollViewer) sender).ViewportWidth;
            var newExtentWidth = ((ScrollViewer) sender).ExtentWidth;
            var newHorizontalOffset = ((ScrollViewer) sender).HorizontalOffset;
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

        internal void UpdateTabItems()
        {
            foreach (var item in this.GetVisualChildren().OfType<TabItem>())
                AnimateTabButton(item);
        }

        #region ==============  Tab item  ==============

        private void TabItem_AttachEvents(TabItem item, bool onlyDetach)
        {
            if (item == null) return;
            var child = VisualTreeHelper.GetChildrenCount(item) > 0 ? VisualTreeHelper.GetChild(item, 0) as FrameworkElement : null;

            item.Unloaded -= OnTabItemUnloaded;
            item.PreviewMouseLeftButtonDown -= TabItem_OnPreviewMouseLeftButtonDown;
            item.Loaded -= OnTabItemLoaded;
            item.MouseEnter -= OnTabItemMouseEnterOrLeave;
            item.MouseLeave -= OnTabItemMouseEnterOrLeave;
            if (child != null)
                child.ToolTipOpening -= OnTabItemToolTipOpening;
            if (child != null && child.ToolTip is ToolTip childToolTip1)
                childToolTip1.Opened -= OnTabItemToolTipOnOpened;

            if (onlyDetach) return;

            item.Unloaded += OnTabItemUnloaded;
            item.PreviewMouseLeftButtonDown += TabItem_OnPreviewMouseLeftButtonDown;
            item.Loaded += OnTabItemLoaded;
            item.MouseEnter += OnTabItemMouseEnterOrLeave;
            item.MouseLeave += OnTabItemMouseEnterOrLeave;
            if (child != null)
                child.ToolTipOpening += OnTabItemToolTipOpening;
            if (child != null && child.ToolTip is ToolTip childToolTip)
                childToolTip.Opened += OnTabItemToolTipOnOpened;

            void OnTabItemUnloaded(object sender, RoutedEventArgs e) => ((TabItem)sender).AutomaticUnloading(OnTabItemUnloaded);
            void OnTabItemLoaded(object sender, RoutedEventArgs e) => ((TabItem)sender).BeginAnimation(OpacityProperty, new DoubleAnimation(0.0, 1.0, AnimationHelper.AnimationDuration));
            void OnTabItemMouseEnterOrLeave(object sender, MouseEventArgs e) => AnimateTabButton((TabItem)sender);
            void OnTabItemToolTipOpening(object sender, ToolTipEventArgs e) => ((MwiChild)((FrameworkElement)sender).DataContext)?.RefreshThumbnail();
            void OnTabItemToolTipOnOpened(object sender, RoutedEventArgs e)
            {
                var toolTip = (ToolTip)sender;
                var tabTextBlock = toolTip.PlacementTarget.GetVisualChildren().OfType<TextBlock>().First();
                toolTip.SetCurrentValueSmart(TagProperty, Tips.IsTextTrimmed(tabTextBlock) ? "1" : null);
            }
        }

        private void TabItem_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mwiChild = ((FrameworkElement) sender).DataContext as MwiChild;
            var element = (FrameworkElement) Mouse.DirectlyOver;
            while (element != null && element.Name != "DeleteTabButton")
                element = VisualTreeHelper.GetParent(element) as FrameworkElement;

            if (element != null) // delete button was pressed
                mwiChild?.CmdClose.Execute(null);
            else
                mwiChild?.Activate();

            e.Handled = true;
        }

        private void AnimateTabButton(TabItem tabItem)
        {
            if (tabItem == null || tabItem.IsElementDisposing()) return; // For VS designer

            LinearGradientBrush newBrush;
            if (tabItem.IsSelected)
                newBrush = TryFindResource("Mwi.BarItem.Selected.BackgroundBrush") as LinearGradientBrush;
            else if (tabItem.IsMouseOver)
                newBrush = TryFindResource("Mwi.BarItem.MouseOver.BackgroundBrush") as LinearGradientBrush;
            else
                newBrush = TryFindResource("Mwi.BarItem.BackgroundBrush") as LinearGradientBrush;

            if (newBrush != null)
                tabItem.SetCurrentValueSmart(BackgroundProperty, AnimationHelper.BeginLinearGradientBrushAnimation(newBrush, (LinearGradientBrush) tabItem.Background));
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

        public void OnUnloaded(object sender, RoutedEventArgs e) => this.AutomaticUnloading(OnUnloaded);
    }
}
