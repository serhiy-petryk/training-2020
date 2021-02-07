using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiItems.xaml
    /// </summary>
    public class MwiItems: ItemsControl, INotifyPropertyChanged
    {
        static MwiItems()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MwiItems), new FrameworkPropertyMetadata(typeof(MwiItems)));
        }

        public MwiItems()
        {
            DataContext = this;
            ((INotifyCollectionChanged)Items).CollectionChanged += OnItemsChanged; 
        }

        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Print($"OnItemsChanged. {e.Action}");
        }

        internal void InvalidateLayout()
        {
            var maximizedFlag = false;
            foreach (var mwiItem in InternalWindows.Where(w => w.WindowState != WindowState.Minimized).OrderByDescending(Panel.GetZIndex))
            {
                if (maximizedFlag)
                {
                    if (mwiItem.Visibility != Visibility.Collapsed)
                    {
                        var temp = mwiItem.Thumbnail; // update thumbnail
                        mwiItem.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (mwiItem.Visibility != Visibility.Visible) mwiItem.Visibility = Visibility.Visible;
                    maximizedFlag = mwiItem.WindowState == WindowState.Maximized;
                }
            }
        }

        const int WINDOW_OFFSET_STEP = 25;
        private double _windowOffset = -WINDOW_OFFSET_STEP;
        internal Point GetStartPositionForItem(FrameworkElement mwiItem)
        {
            _windowOffset += WINDOW_OFFSET_STEP;
            if ((_windowOffset + mwiItem.ActualWidth > MwiPanel.ActualWidth) || (_windowOffset + mwiItem.ActualHeight > MwiPanel.ActualHeight))
                _windowOffset = 0;
            return new Point(_windowOffset, _windowOffset);
        }

        #region ==========  Override  ============
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ScrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            ScrollViewer?.Dispatcher.InvokeAsync(() =>
            {
                if (ScrollViewer.Content is FrameworkElement itemsPresenter && VisualTreeHelper.GetChildrenCount(itemsPresenter) > 0)
                    MwiPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Grid;
            }, DispatcherPriority.Send);
        }
        #endregion

        #region =======  Properties  =========
        public ScrollViewer ScrollViewer;
        public Grid MwiPanel;
        internal IEnumerable<MwiItem> InternalWindows => Items.OfType<MwiItem>().Where(w => !w.IsWindowed);

        private MwiItem _activeMwiItem;
        public MwiItem ActiveMwiItem
        {
            get => _activeMwiItem;
            set
            {
                if (!Equals(_activeMwiItem, value)) _activeMwiItem = value;
                OnPropertiesChanged(nameof(ActiveMwiItem), nameof(ScrollBarKind));
            }
        }

        public ScrollBarVisibility ScrollBarKind =>
            ActiveMwiItem != null && ActiveMwiItem.WindowState == WindowState.Maximized
                ? ScrollBarVisibility.Disabled
                : ScrollBarVisibility.Auto;

        #endregion

        #region =================  INotifyPropertyChanged  ==================
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
