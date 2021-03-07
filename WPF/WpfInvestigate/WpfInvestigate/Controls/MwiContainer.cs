using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiContainer.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class MwiContainer: ContentControl, INotifyPropertyChanged
    {
        const int WINDOW_OFFSET_STEP = 25;

        static MwiContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MwiContainer), new FrameworkPropertyMetadata(typeof(MwiContainer)));
            // KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(MwiContainer), new FrameworkPropertyMetadata(false));
            FocusableProperty.OverrideMetadata(typeof(MwiContainer), new FrameworkPropertyMetadata(false));
        }

        public MwiContainer()
        {
            DataContext = this;
            CmdSetLayout = new RelayCommand(ExecuteWindowsMenuOption, CanExecuteWindowsMenuOption);
        }

        private async void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var o in e.NewItems)
                    {
                        var mwiChild = o as MwiChild;
                        if (mwiChild == null)
                            throw new Exception($"All children of MwiContainer object have to be MwiChild type but it is '{o.GetType().Name}' type");

                        mwiChild.MwiContainer = this;
                        if (mwiChild.Parent is Grid parent)  // remove VS designer error: InvalidOperationException: Specified element is already the logical child of another element. Disconnect it first
                            parent.Children.Remove(mwiChild);
                        MwiPanel.Children.Add(mwiChild);
                        await mwiChild.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Normal).Task;
                        mwiChild.Activate();
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (MwiChild oldChild in e.OldItems)
                        MwiPanel.Children.Remove(oldChild);

                    GetTopChild(Children)?.Activate(false);
                    
                    break;

                default: throw new Exception("Please, check code");
            }
        }

        internal Point GetStartPositionForMwiChild(MwiChild mwiChild)
        {
            _windowOffset += WINDOW_OFFSET_STEP;
            if ((_windowOffset + mwiChild.ActualWidth > MwiPanel.ActualWidth) || (_windowOffset + mwiChild.ActualHeight > MwiPanel.ActualHeight))
                _windowOffset = 0;
            return new Point(_windowOffset, _windowOffset);
        }

        internal void InvalidateLayout()
        {
            var maximizedFlag = false;
            foreach (var mwiChild in InternalWindows.Where(w => w.WindowState != WindowState.Minimized).OrderByDescending(Panel.GetZIndex))
            {
                if (maximizedFlag)
                {
                    if (mwiChild.Visibility != Visibility.Hidden)
                    {
                        var temp = mwiChild.Thumbnail; // update thumbnail
                        mwiChild.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    if (mwiChild.Visibility != Visibility.Visible) mwiChild.Visibility = Visibility.Visible;
                    maximizedFlag = mwiChild.WindowState == WindowState.Maximized;
                }
            }
        }

        #region ============  Override  ====================
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ScrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            MwiPanel = GetTemplateChild("MwiPanel") as Grid;
            _leftPanelContainer = GetTemplateChild("LeftPanelContainer") as Grid;
            _leftPanelButton = GetTemplateChild("LeftPanelButton") as ToggleButton;
            if (_leftPanelButton != null)
            {
                _leftPanelButton.Checked += LeftPanelButton_OnCheckedChanged;
                _leftPanelButton.Unchecked += LeftPanelButton_OnCheckedChanged;
            }

            if (GetTemplateChild("LeftPanelDragThumb") is Thumb leftPanelDragThumb)
                leftPanelDragThumb.DragDelta += LeftPanel_OnDragDelta;

            if (GetTemplateChild("WindowsMenuButton") is ToggleButton windowsMenuButton)
            {
                windowsMenuButton.Checked += OnWindowsMenuButtonCheckedChange;
                windowsMenuButton.Unchecked += OnWindowsMenuButtonCheckedChange;
            }

            Children.CollectionChanged += OnChildrenCollectionChanged;
            OnChildrenCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Children));
            Unloaded += OnMwiContainerUnloaded;

            if (Window.GetWindow(this) is Window wnd) // need to check because an error in VS wpf designer
            {
                wnd.Activated += OnWindowActivated;
                wnd.Deactivated += OnWindowDeactivated;
            }

            void OnWindowActivated(object sender, EventArgs e)
            {
                if (ActiveMwiChild != null && !ActiveMwiChild.IsWindowed)
                    ActiveMwiChild.Activate();
            }
            void OnWindowDeactivated(object sender, EventArgs e)
            {
                if (ActiveMwiChild != null && !ActiveMwiChild.IsWindowed)
                    ActiveMwiChild.IsActive = false;
            }
            void OnMwiContainerUnloaded(object sender, RoutedEventArgs e)
            {
                Unloaded -= OnMwiContainerUnloaded;
                foreach (var mwiChild in Children.Cast<MwiChild>().Where(c=>c.IsWindowed))
                    ((Window) mwiChild.Parent).Close();
            }
        }

        #endregion

        #region =======  Properties  =========
        public ScrollViewer ScrollViewer;
        public Grid MwiPanel;
        public ObservableCollection<object> Children { get; set; } = new ObservableCollection<object>(); // if define as ObservableCollection<MwiChild>: there are vs designer errors in test forms "A value of type 'MwiChild' cannot be added to a collection or dictionary of type 'ObservableCollection`1'"
        internal IEnumerable<MwiChild> InternalWindows => Children.Cast<MwiChild>().Where(w => !w.IsWindowed);
        internal MwiChild GetTopChild(IEnumerable<object> items) => items.Cast<MwiChild>().OrderByDescending(Panel.GetZIndex).FirstOrDefault();

        // Offset for new window.
        private double _windowOffset = -WINDOW_OFFSET_STEP;

        private MwiChild _activeMwiChild;
        public MwiChild ActiveMwiChild
        {
            get => _activeMwiChild;
            set
            {
                if (!Equals(_activeMwiChild, value)) _activeMwiChild = value;
                OnPropertiesChanged(nameof(ActiveMwiChild), nameof(ScrollBarKind));
            }
        }

        public ScrollBarVisibility ScrollBarKind =>
            ActiveMwiChild != null && ActiveMwiChild.WindowState == WindowState.Maximized
                ? ScrollBarVisibility.Disabled
                : ScrollBarVisibility.Auto;

        //==============================
        public static readonly DependencyProperty MwiContainerProperty = DependencyProperty.RegisterAttached("MwiContainer", typeof(MwiContainer), typeof(MwiContainer));
        public static void SetMwiContainer(DependencyObject element, MwiContainer value) => element?.SetValue(MwiContainerProperty, value); // NotNull propagation need to prevent VS designer error
        public static MwiContainer GetMwiContainer(DependencyObject element) => element?.GetValue(MwiContainerProperty) as MwiContainer; // NotNull propagation need to prevent VS designer error

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
