using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
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
            CmdSetLayout = new RelayCommand(ExecuteWindowsMenuOption, CanExecutePredicate);
        }

        private bool CanExecutePredicate(object obj)
        {
            Debug.Print($"CanExecutePredicate: {obj}");
            // throw new NotImplementedException();
            return false;
        }

        public void AddDialog(FrameworkElement content)
        {
            var adorner = new DialogAdorner(this) { CloseOnClickBackground = true };
            if (content is MwiChild mwiChild)
                mwiChild.IsActive = true;
            adorner.ShowContentDialog(content);
        }

        private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (MwiChild mwiChild in e.NewItems)
                    {
                        mwiChild.MwiContainer = this;
                        MwiPanel.Children.Add(mwiChild);
                    }
                    
                    if (e.NewItems.Count > 0)
                        ((MwiChild) e.NewItems[e.NewItems.Count - 1]).Activate();
                    
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (MwiChild oldChild in e.OldItems)
                        MwiPanel.Children.Remove(oldChild);

                    GetTopChild(Children)?.Activate(false);
                    
                    break;
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
                    if (mwiChild.Visibility != Visibility.Collapsed)
                    {
                        var temp = mwiChild.Thumbnail; // update thumbnail
                        mwiChild.Visibility = Visibility.Collapsed;
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
            if (GetTemplateChild("WindowsMenuButton") is ToggleButton windowsMenuButton)
            {
                windowsMenuButton.Checked += OnWindowsMenuButtonCheckedChange;
                windowsMenuButton.Unchecked += OnWindowsMenuButtonCheckedChange;
            }

            Children.CollectionChanged += OnChildrenCollectionChanged;
            OnChildrenCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Children));

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
                foreach (var child in InternalWindows.Where(w => w.IsActive))
                    child.IsActive = false;
            }
        }
        #endregion

        #region =======  Properties  =========
        public ScrollViewer ScrollViewer;
        public Grid MwiPanel;
        public ObservableCollection<MwiChild> Children { get; set; } = new ObservableCollection<MwiChild>();
        internal IEnumerable<MwiChild> InternalWindows => Children.Where(w => !w.IsWindowed);
        internal MwiChild GetTopChild(IEnumerable<MwiChild> items) => items.OrderByDescending(Panel.GetZIndex).FirstOrDefault();

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

        public RelayCommand CmdSetLayout { get; }
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
