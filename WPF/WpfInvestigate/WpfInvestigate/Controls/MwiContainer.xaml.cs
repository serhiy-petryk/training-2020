using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
        public static int MwiUniqueCount = 1;

        public MwiContainer()
        {
            InitializeComponent();
            Children.CollectionChanged += Children_OnCollectionChanged;
        }

        private void Children_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var mwiChild = Children[e.NewStartingIndex];
                    mwiChild.MwiContainer = this;

                    if (mwiChild.ActualPosition.X < 0 || mwiChild.ActualPosition.Y < 0)
                        mwiChild.Position = new Point(_windowOffset, _windowOffset);
                    _windowOffset += WINDOW_OFFSET_STEP;
                    if ((_windowOffset + mwiChild.Width > ActualWidth) || (_windowOffset + mwiChild.Height > ActualHeight))
                        _windowOffset = 0;

                    MwiPanel.Children.Add(mwiChild);
                    mwiChild.Closed += OnMwiChildClosed;

                    ActiveMwiChild = mwiChild;

                    /*SetMwiContainer(mwiChild, this);

                    if (ActiveMwiChild?.WindowState == WindowState.Maximized && mwiChild.Resizable)
                        mwiChild.Loaded += (s, a) => mwiChild.WindowState = WindowState.Maximized;

                    if (mwiChild.ActualPosition.X < 0 || mwiChild.ActualPosition.Y < 0)
                        mwiChild.Position = new Point(_windowOffset, _windowOffset);
                    _windowOffset += WINDOW_OFFSET_STEP;
                    if ((_windowOffset + mwiChild.Width > ActualWidth) || (_windowOffset + mwiChild.Height > ActualHeight))
                        _windowOffset = 0;

                    MwiCanvas.Children.Add(mwiChild);
                    ActiveMwiChild = mwiChild;*/
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var oldChild = (MwiChild)e.OldItems[0];
                    // var oldActiveMwiChild = ActiveMwiChild == oldChild ? null : ActiveMwiChild;
                    // ActiveMwiChild = null; // must be null because sometimes there is an error on WindowTabs.Remove (select window tab => press delete button on active MwiChild): Index was outside the bounds of the array

                    MwiPanel.Children.Remove(oldChild);
                    // ActiveMwiChild = oldActiveMwiChild ?? GetTopChild();*/
                    break;

                case NotifyCollectionChangedAction.Reset:
                    /*while (Children.Count > 0)
                        Children[0].Close();*/
                    break;
            }

            void OnMwiChildClosed(object o, EventArgs args)
            {
                var mwiChild = (MwiChild)o;
                mwiChild.Closed -= OnMwiChildClosed;
                Children.Remove(mwiChild);
            }
        }

        #region =======  Properties  =========
        public ObservableCollection<MwiChild> Children { get; set; } = new ObservableCollection<MwiChild>();
        internal IEnumerable<MwiChild> InternalWindows => Children.Where(w => !w.IsWindowed);
        internal double InnerHeight => ScrollViewer.ActualHeight;
        internal bool WindowShowLock = false; // lock for async window.Show()

        /// Offset for new window.
        private double _windowOffset;
        private MwiChild _activeMwiChild;

        public MwiChild ActiveMwiChild
        {
            get => _activeMwiChild;
            set
            {
                Debug.Print($"Set ActiveMwiChild");
                if (WindowShowLock) // Async window.Show() is running for detaching window => new ActiveMwiChild is not valid
                    return;

                if (_activeMwiChild != value)
                {
                    _activeMwiChild = value;
                    foreach (var window in Children.Where(c => c != value && c.IsActive))
                        window.IsActive = false;

                    if (_activeMwiChild != null)
                    {
                        _activeMwiChild.IsActive = true;
                        // Panel.SetZIndex(_activeMwiChild, MwiUniqueCount++);
                        // ScrollIntoView
                        //if (_activeMwiChild.WindowState == WindowState.Normal && !_activeMwiChild.IsWindowed)
                          //  _activeMwiChild.BringIntoView();
                    }
                }
                OnPropertiesChanged(nameof(ActiveMwiChild), nameof(ScrollBarKind));
                // Dispatcher.Invoke(DispatcherPriority.Render, Tips.EmptyDelegate); // Refresh UI (bug on Startup => active child doesn't highlight and ScrollBar is bad)
                // InvalidateSize();
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
