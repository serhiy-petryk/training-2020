using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using MyWpfMwi.Common;

namespace MyWpfMwi.Mwi
{
    [ContentProperty("Children")]
    public partial class MwiContainer : INotifyPropertyChanged
    {
        const int WINDOW_OFFSET_STEP = 25;
        public static int MwiUniqueCount = 1;

        public MwiContainer()
        {
            InitializeComponent();
            Children.CollectionChanged += Children_OnCollectionChanged;
            DataContext = this;
            CmdSetLayout = new RelayCommand(ExecuteWindowsMenuOption);
        }

        //==============================
        public static readonly DependencyProperty MwiContainerProperty = DependencyProperty.RegisterAttached("MwiContainer", typeof(MwiContainer), typeof(MwiContainer));
        public static void SetMwiContainer(DependencyObject element, MwiContainer value) => element?.SetValue(MwiContainerProperty, value); // NotNull propagation need to prevent VS designer error
        public static MwiContainer GetMwiContainer(DependencyObject element) => element?.GetValue(MwiContainerProperty) as MwiContainer; // NotNull propagation need to prevent VS designer error

        //==============================
        public RelayCommand CmdSetLayout { get; }
        public bool WindowShowLock = false; // lock for async window.Show()

        /// Offset for new window.
        private double _windowOffset;
        private MwiChild _activeMwiChild;

        internal IEnumerable<MwiChild> InternalWindows => Children.Where(w => !w.IsWindowed);
        internal double InnerHeight => ScrollViewer.ActualHeight;

        public ObservableCollection<MwiChild> Children { get; set; } = new ObservableCollection<MwiChild>();

        public MwiChild ActiveMwiChild
        {
            get => _activeMwiChild;
            set
            {
                if (WindowShowLock) // Async window.Show() is running for detaching window => new ActiveMwiChild is not valid
                    return;

                if (_activeMwiChild != value)
                {
                    _activeMwiChild = value;
                    foreach (var window in Children.Where(c => c != value && c.Focused))
                        window.Focused = false;

                    if (_activeMwiChild != null)
                    {
                        _activeMwiChild.Focused = true;
                        Panel.SetZIndex(_activeMwiChild, MwiUniqueCount++);
                        // ScrollIntoView
                        if (_activeMwiChild.WindowState == WindowState.Normal && !_activeMwiChild.IsWindowed)
                            _activeMwiChild.BringIntoView();
                    }
                }
                OnPropertyChanged(new[] {nameof(ActiveMwiChild), nameof(ScrollBarKind)});
                // Dispatcher.Invoke(DispatcherPriority.Render, Tips.EmptyDelegate); // Refresh UI (bug on Startup => active child doesn't highlight and ScrollBar is bad)
                InvalidateSize();
            }
        }

        private Point _canvasSize;
        public double CanvasWidth => _canvasSize.X;
        public double CanvasHeight => _canvasSize.Y;

        public ScrollBarVisibility ScrollBarKind =>
            ActiveMwiChild != null && ActiveMwiChild.WindowState == WindowState.Maximized
                ? ScrollBarVisibility.Disabled
                : ScrollBarVisibility.Auto;

        #region Container Events

        private void MwiContainer_OnLoaded(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this);
            if (wnd !=null) // need to check because an error in VS wpf designer
            {
                wnd.Activated += MwiContainer_OnActivated;
                wnd.Deactivated += MwiContainer_OnDeactivated;
            }
        }

        private void MwiContainer_OnActivated(object sender, EventArgs e)
        {
            if (ActiveMwiChild != null && !ActiveMwiChild.IsWindowed)
                ActiveMwiChild.Focused = true;
        }

        private void MwiContainer_OnDeactivated(object sender, EventArgs e)
        {
            foreach (var child in InternalWindows.Where(w => w.Focused))
                child.Focused = false;
        }

        private void MwiContainer_OnSizeChanged(object sender, SizeChangedEventArgs e) => InvalidateSize();

        private void MwiContainer_OnUnloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            // for (var i = Children.Count - 1; i >= 0; i--) // don't use while Count>0 because closing is with delay (animation)
               // Children[i].Close();
        }

        private void Children_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var mwiChild = Children[e.NewStartingIndex];
                    SetMwiContainer(mwiChild, this);

                    if (ActiveMwiChild?.WindowState == WindowState.Maximized && mwiChild.Resizable)
                        mwiChild.Loaded += (s, a) => mwiChild.WindowState = WindowState.Maximized;

                    if (mwiChild.Position.X < 0 || mwiChild.Position.Y < 0)
                        mwiChild.Position = new Point(_windowOffset, _windowOffset);
                    _windowOffset += WINDOW_OFFSET_STEP;
                    if ((_windowOffset + mwiChild.Width > ActualWidth) || (_windowOffset + mwiChild.Height > ActualHeight))
                        _windowOffset = 0;

                    MwiCanvas.Children.Add(mwiChild);
                    ActiveMwiChild = mwiChild;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var oldChild = (MwiChild)e.OldItems[0];
                    var oldActiveMwiChild = ActiveMwiChild == oldChild ? null : ActiveMwiChild;
                    ActiveMwiChild = null; // must be null because sometimes there is an error on WindowTabs.Remove (select window tab => press delete button on active MwiChild): Index was outside the bounds of the array

                    MwiCanvas.Children.Remove(oldChild);
                    ActiveMwiChild = oldActiveMwiChild ?? GetTopChild();
                    break;

                case NotifyCollectionChangedAction.Reset:
                    while (Children.Count > 0)
                        Children[0].Close();
                    break;
            }
        }

        #endregion

        internal void DetachChild(MwiChild child, bool reverse)
        {
            if (!reverse)
                MwiCanvas.Children.Remove(child);
            else
                MwiCanvas.Children.Add(child);
        }

        /// <summary>
        /// Gets MwiChild with maximum ZIndex.
        /// </summary>
        internal MwiChild GetTopChild() => InternalWindows.Any()
            ? InternalWindows.Aggregate((w1, w2) => Panel.GetZIndex(w1) > Panel.GetZIndex(w2) ? w1 : w2)
            : null;

        internal MwiChild GetBottomChild() => InternalWindows.Any()
            ? InternalWindows.Aggregate((w1, w2) => Panel.GetZIndex(w1) > Panel.GetZIndex(w2) ? w2 : w1)
            : null;

        internal void InvalidateSize()
        {
            double maxWidth = 0, maxHeight=0;
            // підрахунок іде тільки для WindowState=Normal до першого максимального вікна
            foreach (var mwiChild in InternalWindows.Where(w => w.WindowState != WindowState.Minimized).OrderByDescending(Panel.GetZIndex))
            {
                if (mwiChild.WindowState == WindowState.Normal)
                {
                    maxWidth = Math.Max(maxWidth, mwiChild.Position.X + mwiChild.Width);
                    maxHeight = Math.Max(maxHeight, mwiChild.Position.Y + mwiChild.Height);
                }
                else
                    break;
            }

            foreach (var mwiChild in InternalWindows.Where(w => w.WindowState == WindowState.Maximized))
            {
                var newWidth = Math.Max(ActualWidth, maxWidth);
                var newHeight = Math.Max(InnerHeight, maxHeight);
                if (!Tips.AreEqual(mwiChild.Width, newWidth))
                    mwiChild.Width = Math.Max(ActualWidth, maxWidth);
                if (!Tips.AreEqual(mwiChild.Height, newHeight))
                    mwiChild.Height = Math.Max(InnerHeight, maxHeight);
            }

            if (!Tips.AreEqual(_canvasSize.X, maxWidth) || !Tips.AreEqual(_canvasSize.Y, maxHeight))
            {
                _canvasSize = new Point(maxWidth, maxHeight);
                OnPropertyChanged(new[] {nameof(CanvasWidth), nameof(CanvasHeight)});
            }
        }

        //============================================================
        //===========  INotifyPropertyChanged  =======================

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}