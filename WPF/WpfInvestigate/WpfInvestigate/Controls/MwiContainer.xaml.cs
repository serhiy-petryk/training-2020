using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiContainer.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class MwiContainer: INotifyPropertyChanged
    {
        const int WINDOW_OFFSET_STEP = 25;

        public MwiContainer()
        {
            InitializeComponent();
            DataContext = this;
            Children.CollectionChanged += Children_OnCollectionChanged;
        }

        public void AddDialog(FrameworkElement content)
        {
            var adorner = new DialogAdorner(this) { CloseOnClickBackground = true };
            adorner.ShowContentDialog(content);
        }

        private void Children_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var mwiChild = Children[e.NewStartingIndex];
                    mwiChild.MwiContainer = this;

                    if (mwiChild.IsLoaded)
                        OnMwiChildLoaded(mwiChild, null);
                    else
                        mwiChild.Loaded += OnMwiChildLoaded;

                    MwiPanel.Children.Add(mwiChild);
                    mwiChild.Activate();
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var oldChild = (MwiChild)e.OldItems[0];
                    // ActiveMwiChild = null; // must be null because sometimes there is an error on WindowTabs.Remove (select window tab => press delete button on active MwiChild): Index was outside the bounds of the array
                    MwiPanel.Children.Remove(oldChild);
                    if (GetTopChild() is MwiChild newChild)
                        newChild.Activate(false);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    /*while (Children.Count > 0)
                        Children[0].Close();*/
                    break;
            }

            void OnMwiChildLoaded(object o, RoutedEventArgs args)
            {
                var mwiChild = (MwiChild)o;
                mwiChild.Loaded -= OnMwiChildLoaded;
                if (mwiChild.Position.X < 0 || mwiChild.Position.Y < 0)
                {
                    _windowOffset += WINDOW_OFFSET_STEP;
                    if ((_windowOffset + mwiChild.ActualWidth > MwiPanel.ActualWidth) || (_windowOffset + mwiChild.ActualHeight > MwiPanel.ActualHeight))
                        _windowOffset = 0;
                    mwiChild.Position = new Point(_windowOffset, _windowOffset);
                }
            }
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
        public ObservableCollection<MwiChild> Children { get; set; } = new ObservableCollection<MwiChild>();
        internal IEnumerable<MwiChild> InternalWindows => Children.Where(w => !w.IsWindowed);
        internal MwiChild GetTopChild() => Children.Any()
            ? Children.Aggregate((w1, w2) => Panel.GetZIndex(w1) > Panel.GetZIndex(w2) ? w1 : w2)
            : null;

        internal double InnerHeight => ScrollViewer.ActualHeight;

        /// Offset for new window.
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
