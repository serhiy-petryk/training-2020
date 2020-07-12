using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MyWpfMwi.Common;
using MyWpfMwi.Controls.DialogItems;
using MyWpfMwi.Examples;

namespace MyWpfMwi.Mwi
{
    /// <summary>
    /// Interaction logic for MwiChild.xaml
    /// </summary>
    [ContentProperty("Content")]
    public partial class MwiChild : INotifyPropertyChanged
    {
        public const double MIN_WIDTH = 250;
        public const double MIN_HEIGHT = 50;
        public const double DEFAULT_WIDTH = 450;
        public const double DEFAULT_HEIGHT = 300;
        private const double MAX_THUMBNAIL_SIZE = 180;

        #region Constructor

        static MwiChild()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MwiChild), new FrameworkPropertyMetadata(typeof(MwiChild)));
        }
        public MwiChild()
        {
            InitializeComponent();
            DataContext = this;
            CmdDetach = new RelayCommand(ToggleDetach, _ => AllowDetach);
            CmdMinimize = new RelayCommand(ToggleMinimize, _ => AllowMinimize);
            CmdMaximize = new RelayCommand(ToggleMaximize, _ => AllowMaximize);
            SysCmdRestore = new RelayCommand(ToggleMaximize, _ => AllowMaximize && WindowState == WindowState.Maximized);
            SysCmdMaximize = new RelayCommand(ToggleMaximize, _ => AllowMaximize && WindowState != WindowState.Maximized);
            CmdClose = new RelayCommand(DoClose, _ => AllowClose);
        }

        #endregion

        //============  Commands  =============
        public RelayCommand CmdDetach { get; }
        public RelayCommand CmdMinimize { get; }
        public RelayCommand CmdMaximize { get; }
        public RelayCommand SysCmdMaximize { get; }
        public RelayCommand SysCmdRestore { get; }
        public RelayCommand CmdClose { get; }

        //=====================================
        public readonly int Id = MwiContainer.MwiUniqueCount++;
        public bool IsWindowed => Parent is Window;
        public bool IsDialog => Parent is DialogItems;
        public MwiContainer Container => MwiContainer.GetMwiContainer(this);
        public bool IsSelected => Container?.ActiveMwiChild == this;

        public Window DetachedHost => IsWindowed ? (Window)Parent : null;
        public ImageSource Thumbnail => CreateThumbnail();
        public double ThumbnailWidth => GetThumbnailSize().X;
        public double ThumbnailHeight => GetThumbnailSize().Y;

        //  ===============  MwiChild State ===============
        private Size _lastNormalSize = new Size(DEFAULT_WIDTH, DEFAULT_HEIGHT);
        private Point _attachedPosition;
        private Point _detachedPosition;
        private WindowState? _beforeMinimizedState { get; set; } // Previous state of minimized window.
        private ImageSource _thumbnailCache;

        #region ============  Panels  =============
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(UIElement), typeof(MwiChild));
        public UIElement Content
        {
            get => (UIElement)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty CommandBarProperty = DependencyProperty.Register("CommandBar", typeof(UIElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public UIElement CommandBar
        {
            get => (UIElement)GetValue(CommandBarProperty);
            set => SetValue(CommandBarProperty, value);
        }

        public static readonly DependencyProperty StatusBarProperty = DependencyProperty.Register("StatusBar", typeof(UIElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public UIElement StatusBar
        {
            get => (UIElement)GetValue(StatusBarProperty);
            set => SetValue(StatusBarProperty, value);
        }

        public static readonly DependencyProperty LeftHeaderPanelProperty = DependencyProperty.Register("LeftHeaderPanel", typeof(FrameworkElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public FrameworkElement LeftHeaderPanel
        {
            get => (FrameworkElement)GetValue(LeftHeaderPanelProperty);
            set => SetValue(LeftHeaderPanelProperty, value);
        }

        public static readonly DependencyProperty RightHeaderPanelProperty = DependencyProperty.Register("RightHeaderPanel", typeof(FrameworkElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public FrameworkElement RightHeaderPanel
        {
            get => (FrameworkElement)GetValue(RightHeaderPanelProperty);
            set => SetValue(RightHeaderPanelProperty, value);
        }
        #endregion

        #region ============  Buttons  =============

        public static readonly DependencyProperty AllowDetachProperty = DependencyProperty.Register(nameof(AllowDetach), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool AllowDetach
        {
            get => (bool)GetValue(AllowDetachProperty);
            set => SetValue(AllowDetachProperty, value);
        }

        public static readonly DependencyProperty AllowMinimizeProperty = DependencyProperty.Register(nameof(AllowMinimize), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool AllowMinimize
        {
            get => (bool)GetValue(AllowMinimizeProperty);
            set => SetValue(AllowMinimizeProperty, value);
        }

        public static readonly DependencyProperty AllowMaximizeProperty = DependencyProperty.Register(nameof(AllowMaximize), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool AllowMaximize
        {
            get => (bool)GetValue(AllowMaximizeProperty);
            set => SetValue(AllowMaximizeProperty, value);
        }

        public static readonly DependencyProperty AllowCloseProperty = DependencyProperty.Register(nameof(AllowClose), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool AllowClose
        {
            get => (bool)GetValue(AllowCloseProperty);
            set => SetValue(AllowCloseProperty, value);
        }
        #endregion

        //================================
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MwiChild));
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(MwiChild));
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty ResizableProperty = DependencyProperty.Register(nameof(Resizable), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool Resizable
        {
            get => (bool)GetValue(ResizableProperty);
            set => SetValue(ResizableProperty, value);
        }

        //=============================================
        #region Dependency Properties

        /// <summary>
        /// Identifies the MwiChild.FocusedProperty dependency property.
        /// </summary>
        /// <returns>The identifier for the MwiChild.FocusedProperty property.</returns>
        public static readonly DependencyProperty FocusedProperty = DependencyProperty.Register("Focused", typeof(bool), typeof(MwiChild), new UIPropertyMetadata(false, OnFocusedValueChanged));

        /// <summary>
        /// Identifies the MwiChild.WindowStateProperty dependency property.
        /// </summary>
        /// <returns>The identifier for the MwiChild.WindowStateProperty property.</returns>
        public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register("WindowState", typeof(WindowState), typeof(MwiChild), new UIPropertyMetadata(WindowState.Normal, OnWindowStateValueChanged));

        /// <summary>
        /// Identifies the MwiChild.PositionProperty dependency property.
        /// </summary>
        /// <returns>The identifier for the MwiChild.PositionProperty property.</returns>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(Point), typeof(MwiChild), new UIPropertyMetadata(new Point(-1, -1), OnPositionValueChanged));

        #endregion

        #region Dependency Events

        /// <summary>
        /// Identifies the MwiChild.ClosingEvent routed event.
        /// </summary>
        /// <returns>The identifier for the MwiChild.ClosingEvent routed event.</returns>
        public static readonly RoutedEvent ClosingEvent = EventManager.RegisterRoutedEvent("Closing", RoutingStrategy.Bubble, typeof(ClosingEventArgs), typeof(MwiChild));

        /// <summary>
        /// Identifies the MwiChild.ClosedEvent routed event.
        /// </summary>
        /// <returns>The identifier for the MwiChild.ClosedEvent routed event.</returns>
        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventArgs), typeof(MwiChild));

        #endregion

        #region Property Accessors

        /// <summary>
        /// Gets or sets a value indicating whether the [window is focused].
        /// This is a dependency property.
        /// </summary>
        /// <value><c>true</c> if [window is focused]; otherwise, <c>false</c>.</value>
        public bool Focused
        {
            get { return (bool)GetValue(FocusedProperty); }
            set { SetValue(FocusedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the state of the window.
        /// This is a dependency property.
        /// </summary>
        /// <value>The state of the window.</value>
        public WindowState WindowState
        {
            get { return (WindowState)GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }

        /// <summary>
        /// Gets or sets position of top left corner of window.
        /// This is a dependency property.
        /// </summary>
        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// Force user not to use Margin property.
        /// </summary>
        private new Thickness Margin { set { } }

        #endregion

        #region Event Accessors

        public event RoutedEventHandler Closing
        {
            add { AddHandler(ClosingEvent, value); }
            remove { RemoveHandler(ClosingEvent, value); }
        }

        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        #endregion

        #region Control Events

        private void MwiChild_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Container != null)
                StatusBar = new StatusBarExample();

            if (Icon == null)
                Icon = (ImageSource)FindResource("DefaultIcon");

            AnimateShow(null);
        }

        /// <summary>
        /// Handles the GotFocus event of the MwiChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MwiChild_OnGotFocus(object sender, RoutedEventArgs e) => Focus();

        #endregion

        #region Control Overrides

        private void MoveThumb_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            element.IsEnabled = false;
            ToggleMaximize(null);
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => element.IsEnabled = true)); // nexttick action to prevent MoveThumb_OnDragDelta event
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseDown"/> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (Container != null)
                Focused = true;
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (IsWindowed && Icon != null && DetachedHost.Icon == null)
                DetachedHost.Icon = Icon;
            else if (IsWindowed && Icon == null && DetachedHost.Icon != null)
                Icon = DetachedHost.Icon;
            else if (IsWindowed && Icon == null && DetachedHost.Icon == null)
            {
                Icon = (ImageSource)FindResource("DefaultIcon");
                DetachedHost.Icon = (ImageSource)FindResource("DefaultIcon");
            }

            if (IsWindowed && !string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(DetachedHost.Title))
                DetachedHost.Title = Title;
            else if (IsWindowed && string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(DetachedHost.Title))
                Title = DetachedHost.Title;

            if (IsWindowed)
            {
                DetachedHost.KeyDown -= DetachedHost_KeyDown;
                DetachedHost.KeyDown += DetachedHost_KeyDown;
                DetachedHost.Closed += (sender, args) => DetachedHost.KeyDown -= DetachedHost_KeyDown;
            }
        }

        private void DetachedHost_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.F4) && IsWindowed) // Is Alt+F4 key pressed
            {
                CmdClose.Execute(null);
                e.Handled = true;
            }
        }

        private void SystemMenuButton_OnChecked(object sender, RoutedEventArgs e) =>
            Controls.ToggleButtonHelper.OpenMenu_OnCheck(sender);

        #endregion

        #region Top Button Events

        public void RestoreExternalWindowRect(Size? newSize = null)
        {
            var window = DetachedHost;
            if (window == null)
                return;

            if (newSize.HasValue)
                _lastNormalSize = newSize.Value;

            Position = _detachedPosition; // new Point(window.Left, window.Top);
            var maximizedWindowRectangle = Tips.GetMaximizedWindowRectangle();

            _detachedPosition = new Point(
                Math.Max(0, maximizedWindowRectangle.X + (maximizedWindowRectangle.Width - _lastNormalSize.Width) / 2),
                Math.Max(0, maximizedWindowRectangle.Y + (maximizedWindowRectangle.Height - _lastNormalSize.Height) / 2));

            if (WindowState == WindowState.Maximized)
            {
                window.Left = maximizedWindowRectangle.Left;
                window.Top = maximizedWindowRectangle.Top;
                Width = maximizedWindowRectangle.Width;
                Height = maximizedWindowRectangle.Height;
            }
            else
            {
                Width = _lastNormalSize.Width;
                Height = _lastNormalSize.Height;
                Position = _detachedPosition;
            }
        }

        public void ToggleDetach(object p)
        {
            if (WindowState == WindowState.Normal)
                SaveActualRectangle();

            if (IsWindowed)
            {
                AnimateHide(() =>
                {
                    var host = DetachedHost;
                    host.Close();
                    DetachedHost.Content = null;
                    Container.DetachChild(this, true);
                    Focused = true;

                    if (WindowState == WindowState.Maximized)
                    {
                        Position = new Point(0, 0);
                        Width = Container.ActualWidth;
                        Height = Container.ActualHeight;
                    }
                    else
                        Position = _attachedPosition;

                    OnWindowStateValueChanged(this, new DependencyPropertyChangedEventArgs(WindowStateProperty, this.WindowState, WindowState));
                    OnPropertyChanged(new[] { nameof(IsWindowed) });

                    var storyboard = new Storyboard();
                    storyboard.Children.Add(AnimationHelper.GetOpacityAnimation(this, 0, 1));
                    storyboard.Begin();
                });
            }
            else
            {
                var window = new Window
                {
                    Style = (Style)FindResource("HeadlessWindow"),
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Opacity = 0
                };

                AnimateHide(() =>
                {
                    Container.DetachChild(this, false);
                    window.Content = this;
                    RestoreExternalWindowRect();

                    window.Activated += (sender, args) =>
                    {
                        if (Container.ActiveMwiChild != this)
                            Container.ActiveMwiChild = this;
                        else
                            Focused = true;
                    };
                    window.Deactivated += (sender, args) =>
                    {
                        if (!Container.WindowShowLock)
                            Focused = false;
                    };

                    Container.WindowShowLock = true;
                    window.Show();
                    Container.WindowShowLock = false;

                    // Refresh ScrollBar scrolling
                    Container.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    Container.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                    OnWindowStateValueChanged(this,
                        new DependencyPropertyChangedEventArgs(WindowStateProperty, this.WindowState,
                            WindowState));
                    OnPropertyChanged(new[] { nameof(IsWindowed) });

                    Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                    {
                        var storyboard = new Storyboard();
                        storyboard.Children.Add(AnimationHelper.GetOpacityAnimation(window, 0, 1));
                        storyboard.Begin();
                    }));
                });
            }
        }

        /// <summary>
        /// Handles the Click event of the minimizeButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        public void ToggleMinimize(object p)
        {
            if (WindowState != WindowState.Minimized)
                CreateThumbnail();

            if (IsWindowed)
            {
                if (DetachedHost.WindowState == WindowState.Minimized)
                    DetachedHost.WindowState = _beforeMinimizedState ?? WindowState.Normal;
                else
                {
                    _beforeMinimizedState = DetachedHost.WindowState;
                    DetachedHost.WindowState = WindowState.Minimized;
                }
            }
            else if (WindowState == WindowState.Minimized)
            {
                if (Visibility != Visibility.Visible)
                    Visibility = Visibility.Visible;
                WindowState = _beforeMinimizedState ?? WindowState.Normal;
            }
            else
            {
                _beforeMinimizedState = WindowState;
                WindowState = WindowState.Minimized;
            }
        }

        /// <summary>
        /// Handles the Click event of the maximizeButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        // public void ToggleMaximize(object p) => AnimateToggleMaximize();
        public void ToggleMaximize(object p) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        /// <summary>
        /// Handles the Click event of the closeButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void DoClose(object p)
        {
            ClosingEventArgs eventArgs = new ClosingEventArgs(ClosingEvent);
            RaiseEvent(eventArgs);

            if (eventArgs.Cancel)
                return;

            if (IsDialog)
                ApplicationCommands.Close.Execute(this, (DialogItems)Parent);
            else
                Close();

            RaiseEvent(new RoutedEventArgs(ClosedEvent));
        }

        #endregion

        #region Move & Resize Thumb Events

        private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e) // Move & resize
        {
            if (!Focused)
                Focused = true;
            e.Handled = true;
        }

        private void MoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // SaveActualSize & SaveActualPosition doesn't work => ??? may be depend on animation
                _lastNormalSize = new Size(ActualWidth * 0.8, ActualHeight * 0.8);
                if (IsWindowed)
                    _detachedPosition = new Point(0, 0);
                else
                    _attachedPosition = new Point(0, 0);
                ToggleMaximize(null);
                return;
            }

            var newX = Position.X + e.HorizontalChange;
            var newY = Position.Y + e.VerticalChange;

            if (!IsWindowed)
            {
                // Check is mouse in outside of container
                var mousePosition = Mouse.GetPosition(Container);
                var childPosition = TranslatePoint(new Point(0, 0), Container);
                if (newX < (childPosition.X - mousePosition.X))
                    newX -= e.HorizontalChange; // is outside => restore old value
                if (newY < (childPosition.Y - mousePosition.Y))
                    newY -= e.VerticalChange; // is outside => restore old value
            }

            if (IsDialog)
            {
                var itemsPresenter = ((DialogItems)Parent).ItemsPresenter;
                var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
                if (itemsPresenter != null && container != null)
                {
                    if (newX + ActualWidth > container.ActualWidth)
                        newX = container.ActualWidth - ActualWidth;
                    if (newX < 0) newX = 0;

                    if (newY + ActualHeight > container.ActualHeight)
                        newY = container.ActualHeight - ActualHeight;
                    if (newY < 0) newY = 0;
                }
            }

            Position = new Point(newX, newY);
            Container?.InvalidateSize();

            if (!IsWindowed && !IsDialog)
            {
                // Smooth container scrolling
                var sv = Container.ScrollViewer;
                if (Math.Abs(e.HorizontalChange) > Tips.SCREEN_TOLERANCE && (newX + ActualWidth) > Container.ActualWidth)
                    sv.ScrollToHorizontalOffset(Math.Max(0, sv.HorizontalOffset + e.HorizontalChange * 0.25));
                if (Math.Abs(e.VerticalChange) > Tips.SCREEN_TOLERANCE && (newY + ActualHeight) > Container.InnerHeight)
                    sv.ScrollToVerticalOffset(Math.Max(0, sv.VerticalOffset + e.VerticalChange * 0.25));
            }
            e.Handled = true;
        }

        private void ResizeThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;

            if (thumb.HorizontalAlignment == HorizontalAlignment.Left)
                OnResizeLeft(e.HorizontalChange);
            else if (thumb.HorizontalAlignment == HorizontalAlignment.Right)
                OnResizeRight(e.HorizontalChange);

            if (thumb.VerticalAlignment == VerticalAlignment.Top)
                OnResizeTop(e.VerticalChange);
            else if (thumb.VerticalAlignment == VerticalAlignment.Bottom)
                OnResizeBottom(e.VerticalChange);

            e.Handled = true;
            if (sender != null && !IsWindowed)
                Container?.InvalidateSize();
        }

        private void OnResizeLeft(double horizontalChange)
        {
            var change = Math.Min(horizontalChange, ActualWidth - MinWidth);
            var oldX = Position.X;
            if (oldX + change < 0) change = -oldX;

            if (IsDialog)
            {
                var itemsPresenter = ((DialogItems) Parent).ItemsPresenter;
                if (itemsPresenter != null)
                {
                    if (itemsPresenter.Margin.Left + change < 0)
                        change = -itemsPresenter.Margin.Left;
                    if ((ActualWidth - change) > MaxWidth)
                        change = ActualWidth - MaxWidth;
                }
            }

            if (!Tips.AreEqual(0.0, change))
            {
                Width = ActualWidth - change;
                Position = new Point(oldX + change, Position.Y);
            }
        }
        private void OnResizeTop(double verticalChange)
        {
            var change = Math.Min(verticalChange, ActualHeight - MinHeight);
            var oldY = Position.Y;
            if (oldY + change < 0) change = -oldY;

            if (IsDialog)
            {
                var itemsPresenter = ((DialogItems)Parent).ItemsPresenter;
                if (itemsPresenter != null)
                {
                    if (itemsPresenter.Margin.Top + change < 0)
                        change = -itemsPresenter.Margin.Top;
                    if ((Height - change) > MaxHeight)
                        change = Height - MaxHeight;
                }
            }

            if (!Tips.AreEqual(0.0, change))
            {
                Height = ActualHeight - change;
                Position = new Point(Position.X, oldY + change);
            }
        }
        private void OnResizeRight(double horizontalChange)
        {
            var change = Math.Min(-horizontalChange, ActualWidth - MinWidth);
            if ((ActualWidth - change) > MaxWidth)
                change = ActualWidth - MaxWidth;

            if (IsDialog)
            {
                var itemsPresenter = ((DialogItems)Parent).ItemsPresenter;
                var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
                if (itemsPresenter != null && container != null && (itemsPresenter.Margin.Left + ActualWidth - change) > container.ActualWidth)
                    change = itemsPresenter.Margin.Left + ActualWidth - container.ActualWidth;
            }

            if (!Tips.AreEqual(0.0, change))
                Width = ActualWidth - change;
        }
        private void OnResizeBottom(double verticalChange)
        {
            var change = Math.Min(-verticalChange, ActualHeight - MinHeight);
            if ((ActualHeight - change) > MaxHeight)
                change = ActualHeight - MaxHeight;

            if (IsDialog)
            {
                var itemsPresenter = ((DialogItems)Parent).ItemsPresenter;
                var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
                if (itemsPresenter != null && container != null && (itemsPresenter.Margin.Top + ActualHeight - change) > container.ActualHeight)
                    change = itemsPresenter.Margin.Top + ActualHeight - container.ActualHeight;
            }

            if (!Tips.AreEqual(0.0, change))
                Height = ActualHeight - change;
        }

        #endregion

        #region Dependency Property Events


        /// <summary>
        /// Dependency property event once the windows state value has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnWindowStateValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            ((MwiChild)sender).WindowStateValueChanged((WindowState)e.NewValue, (WindowState)e.OldValue);
        private void WindowStateValueChanged(WindowState newWindowState, WindowState previousWindowState)
        {
            var isDetachEvent = previousWindowState == newWindowState;

            if (previousWindowState == WindowState.Normal && !isDetachEvent)
                SaveActualRectangle();

            if (IsWindowed)
            {
                if (newWindowState == WindowState.Normal)
                {
                    Width = _lastNormalSize.Width;
                    Height = _lastNormalSize.Height;
                    Position = new Point(_detachedPosition.X, _detachedPosition.Y);
                }

                if (newWindowState != WindowState.Minimized && DetachedHost.WindowState != WindowState.Normal)
                    DetachedHost.WindowState = WindowState.Normal;

                if (newWindowState == WindowState.Maximized)
                {
                    var maximizedWindowRectangle = Tips.GetMaximizedWindowRectangle();
                    Position = new Point(maximizedWindowRectangle.Left, maximizedWindowRectangle.Top);
                    Width = maximizedWindowRectangle.Width;
                    Height = maximizedWindowRectangle.Height;
                }
            }

            if (!IsWindowed && !isDetachEvent)
                AnimateWindowState(previousWindowState);

            if (!IsWindowed || isDetachEvent)
            {
                Container?.InvalidateSize();
                Container?.OnPropertyChanged(new[] { nameof(MwiContainer.ScrollBarKind) });
            }

            // Activate main window (in case of attach)
            if (IsWindowed && !DetachedHost.IsFocused)
                DetachedHost.Focus();
            else if (!IsWindowed && !Window.GetWindow(this).IsFocused)
                Window.GetWindow(this)?.Focus();
        }

        /// <summary>
        /// Dependency property event once the position value has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnPositionValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Point)e.NewValue == (Point)e.OldValue)
                return;

            var mwiChild = (MwiChild)sender;
            var newPosition = (Point)e.NewValue;

            if (mwiChild.IsWindowed)
            {
                var window = mwiChild.DetachedHost;
                var newTop = Math.Min(SystemParameters.PrimaryScreenHeight, newPosition.Y);
                if (!Tips.AreEqual(newTop, window.Top))
                    window.Top = newTop;
                var newLeft = Math.Min(SystemParameters.PrimaryScreenWidth, newPosition.X);
                if (!Tips.AreEqual(newLeft, window.Left))
                    window.Left = newLeft;
            }
            else if (mwiChild.Parent is Canvas)
            {
                Canvas.SetTop(mwiChild, newPosition.Y);
                Canvas.SetLeft(mwiChild, newPosition.X);
            }
            else if (mwiChild.IsDialog)
            {
                var dialogItems = (DialogItems)mwiChild.Parent;
                dialogItems.ItemsPresenter.Margin = new Thickness(newPosition.X, newPosition.Y, 0, 0);
            }
        }

        /// <summary>
        /// Dependency property event once the focused value has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnFocusedValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var mwiChild = (MwiChild)sender;
            var focused = (bool)e.NewValue;
            if (mwiChild.Container == null)
                return;

            if (focused)
            {
                if (mwiChild.WindowState == WindowState.Minimized || mwiChild.DetachedHost?.WindowState == WindowState.Minimized)
                {
                    // nothing to do (ToggleMinimize is doing in WindowsBar.TabItem_PreviewMouseLeftButtonDown): mwiChild.ToggleMinimize(null);
                }
                else if (mwiChild.IsWindowed)
                    mwiChild.DetachedHost?.Focus();
            }

            if (focused)
                mwiChild.RaiseEvent(new RoutedEventArgs(GotFocusEvent, mwiChild));
            else
                mwiChild.RaiseEvent(new RoutedEventArgs(LostFocusEvent, mwiChild));
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = (Button)sender;
            var position = Mouse.GetPosition(button);
            if (position.X > 0 && position.X < button.ActualWidth && position.Y > 0 && position.Y < button.ActualHeight)
            {
                ((Button)sender).Command.Execute(null);
                e.Handled = true;
            }
        }
        #endregion

        /// <summary>
        /// Manually closes the child window.
        /// </summary>
        public void Close()
        {
            AnimateHide(() =>
            {
                if (Content is IDisposable)
                    ((IDisposable)Content).Dispose();
                DetachedHost?.Close();
                Container?.Children.Remove(this);
            });
        }

        /// <summary>
        /// Set focus to the child window and brings into view.
        /// </summary>
        public new void Focus()
        {
            if (Container != null && Container.ActiveMwiChild != this)
                Container.ActiveMwiChild = this;
        }

        public void Activate()
        {
            Container.ActiveMwiChild = this;

            if (WindowState == WindowState.Minimized || DetachedHost?.WindowState == WindowState.Minimized)
            {
                ToggleMinimize(null);
                Focused = true;
            }
            else if (IsWindowed)
            {
                if (Focused)
                    Focused = false;
                Focused = true;
            }
        }


        private ImageSource CreateThumbnail()
        {
            if (WindowState != WindowState.Minimized || _thumbnailCache == null)
            {
                var bitmap = new RenderTargetBitmap((int)Math.Round(ActualWidth),
                    (int)Math.Round(ActualHeight), 96, 96, PixelFormats.Default);
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    var brush = new VisualBrush(this);
                    context.DrawRectangle(brush, null,
                        new Rect(new Point(), new Size(ActualWidth, ActualHeight)));
                    context.Close();
                }

                bitmap.Render(drawingVisual);
                _thumbnailCache = bitmap;
            }
            return _thumbnailCache;
        }

        private void SaveActualRectangle()
        {
            _lastNormalSize = new Size(ActualWidth, ActualHeight);
            SaveActualPosition();
        }
        private void SaveActualPosition()
        {
            if (IsWindowed)
                _detachedPosition = new Point(DetachedHost.Left, DetachedHost.Top);
            else
                _attachedPosition = new Point(Position.X, Position.Y);
        }

        public void RefreshThumbnail() =>
            OnPropertyChanged(new[] { nameof(Thumbnail), nameof(ThumbnailWidth), nameof(ThumbnailHeight) });
        private Point GetThumbnailSize()
        {
            var width = Thumbnail?.Width ?? 0;
            var height = Thumbnail?.Height ?? 0;
            var maxSize = Math.Max(width, height);
            var factor = maxSize > MAX_THUMBNAIL_SIZE ? MAX_THUMBNAIL_SIZE / maxSize : 1;
            return new Point(width * factor, height * factor);
        }

        public override string ToString() => Title;

        //=============  Setting =============
        public void SaveRectToSetting()
        {
            if (WindowState == WindowState.Normal)
                _lastNormalSize = new Size(ActualWidth, ActualHeight);
            Properties.Settings.Default.Width = _lastNormalSize.Width;
            Properties.Settings.Default.Height = _lastNormalSize.Height;
            Properties.Settings.Default.IsMaximized = WindowState == WindowState.Maximized;
            Properties.Settings.Default.Save();
        }
        public void RestoreRectFromSetting()
        {
            var newSize = new Size { Width = Properties.Settings.Default.Width, Height = Properties.Settings.Default.Height };
            RestoreExternalWindowRect(newSize);
            if (Properties.Settings.Default.IsMaximized)
            {
                WindowState = WindowState.Maximized;
                _lastNormalSize = newSize; // _lastNormalSize == 0 after Maximized just created window => need to restore savedSize
            }
        }

        //============================================================
        //===========  INotifyPropertyChanged  =======================

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}