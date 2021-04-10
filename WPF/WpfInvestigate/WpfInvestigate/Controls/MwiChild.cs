using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfInvestigate.Common;
using WpfInvestigate.Themes;
using WpfInvestigate.ViewModels;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiChild.xaml
    /// </summary>
    public partial class MwiChild: ResizingControl
    {
        [Flags]
        public enum Buttons
        {
            Close = 1,
            Minimize = 2,
            Maximize = 4,
            Detach = 8
        }

        private static int controlId = 0;
        internal int _controlId = controlId++;
        static MwiChild()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MwiChild), new FrameworkPropertyMetadata(typeof(ResizingControl)));
            FocusableProperty.OverrideMetadata(typeof(MwiChild), new FrameworkPropertyMetadata(true));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(MwiChild), new FrameworkPropertyMetadata(true));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(MwiChild), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(MwiChild), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(MwiChild), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));
        }

        public MwiChild()
        {
            CmdDetach = new RelayCommand(ToggleDetach, _ => AllowDetach);
            CmdMinimize = new RelayCommand(ToggleMinimize, _ => AllowMinimize);
            CmdMaximize = new RelayCommand(ToggleMaximize, _ => AllowMaximize);
            SysCmdRestore = new RelayCommand(ToggleMaximize, _ => AllowMaximize && WindowState == WindowState.Maximized);
            SysCmdMaximize = new RelayCommand(ToggleMaximize, _ => AllowMaximize && WindowState != WindowState.Maximized);
            CmdClose = new RelayCommand(Close, _ => AllowClose);

            DataContext = this;
            Loaded += OnMwiChildLoaded;
            var dpd = DependencyPropertyDescriptor.FromProperty(BackgroundProperty, typeof(MwiChild));
            dpd.AddValueChanged(this, (sender, args) => OnPropertiesChanged(nameof(BaseColor)));

            MwiAppViewModel.Instance.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(MwiAppViewModel.AppColor))
                {
                    if (TryFindResource("Mwi.Common.BaseColorProxy") is BindingProxy colorProxy)
                        colorProxy.Value = BaseColor;
                    OnPropertiesChanged(nameof(BaseColor));
                }
            };

            if (Icon == null) Icon = FindResource("Mwi.DefaultIcon") as ImageSource;

            /* can't reproduce (2021-03-12):
             MwiAppViewModel.Instance.PropertyChanged += async (sender, args) =>
            {
                if (ActualWidth > 0 && args is PropertyChangedEventArgs e && e.PropertyName == nameof(MwiAppViewModel.CurrentTheme))
                {
                    // fixed bug (?): при зміні theme в MwiChild зявляється справа полоса
                    var oldWidth = ActualWidth;
                    Width = ActualWidth + 1;
                    await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render).Task;
                    Width = oldWidth;
                }
            };*/

            void OnMwiChildLoaded(object sender, RoutedEventArgs e)
            {
                Loaded -= OnMwiChildLoaded;
                if (MwiContainer != null && (Position.X < 0 || Position.Y < 0))
                    Position = MwiContainer.GetStartPositionForMwiChild(this);
                AnimateShow();
            }
        }

        #region =============  Override methods  ====================
        private static bool _isActivating = false;
        public override void Activate() => Activate(true);

        protected override void MoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            TransitionFromMaximizedToNormalWindowState(true);
            base.MoveThumb_OnDragDelta(sender, e);
        }

        protected override void ResizeThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            TransitionFromMaximizedToNormalWindowState(false);
            base.ResizeThumb_OnDragDelta(sender, e);
        }
        private void TransitionFromMaximizedToNormalWindowState(bool isMoving)
        {
            if (WindowState == WindowState.Maximized)
            {
                // SaveActualSize & SaveActualPosition doesn't work => ??? may be depend on animation
                _lastNormalSize = isMoving
                    ? new Size(Math.Round(ActualWidth * 0.9), Math.Round(ActualHeight * 0.9))
                    : new Size(ActualWidth, ActualHeight);

                if (IsWindowed)
                    _detachedPosition = new Point(0, 0);
                else
                    _attachedPosition = new Point(0, 0);
                ToggleMaximize(null);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("MovingThumb") is Thumb movingThumb)
            {
                MovingThumb = movingThumb;
                movingThumb.MouseDoubleClick += (sender, args) =>
                {
                    var element = (FrameworkElement)sender;
                    element.IsEnabled = false;
                    ToggleMaximize(null);
                    Dispatcher.InvokeAsync(new Action(() => element.IsEnabled = true), DispatcherPriority.Render); // nexttick action to prevent MoveThumb_OnDragDelta event
                };
            }
        }

        private Window _activatedHost;
        protected override async void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (_activatedHost != null)
            {
                _activatedHost.Activated -= OnWindowActivated;
                _activatedHost.Deactivated -= OnWindowDeactivated;
                _activatedHost = null;
                return;
            }

            if (!IsWindowed) return;

            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Normal).Task;
            _activatedHost = Parent as Window;
            _activatedHost.Icon = (GetTemplateChild("IconImage") as Image)?.Source;

            if (!string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(_activatedHost.Title))
                _activatedHost.Title = Title;
            else if (string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(_activatedHost.Title))
                Title = _activatedHost.Title;

            OnHostClosed(_activatedHost, null);
            _activatedHost.KeyDown += OnHostKeyDown;
            _activatedHost.Closed += OnHostClosed;
            _activatedHost.Activated += OnWindowActivated;
            _activatedHost.Deactivated += OnWindowDeactivated;

            void OnHostKeyDown(object sender, KeyEventArgs e)
            {
                if (Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.F4)) // Is Alt+F4 key pressed
                {
                    CmdClose.Execute(null);
                    e.Handled = true;
                }
            }
            void OnHostClosed(object sender, EventArgs e)
            {
                if (sender is Window wnd)
                {
                    wnd.Closed -= OnHostClosed;
                    wnd.KeyDown -= OnHostKeyDown;
                    wnd.Activated -= OnWindowActivated;
                    wnd.Deactivated -= OnWindowDeactivated;
                }
            }
            void OnWindowActivated(object sender, EventArgs e) => Activate();
            void OnWindowDeactivated(object sender, EventArgs e) => IsActive = false;
        }
        #endregion

        public void Activate(bool restoreMinimizedSize)
        {
            if (MwiContainer == null && IsWindowed) // MwiStartup
                IsActive = true;

            if (_isActivating || MwiContainer == null) return;

            _isActivating = true;

            base.Activate();

            if (!IsActive)
                MwiContainer.InvalidateLayout();

            if (!IsActive && MwiContainer != null)
            {
                MwiContainer.Children.Cast<MwiChild>().Where(c => !Equals(c, this) && c.IsActive).ToList().ForEach(c => c.IsActive = false);

                if (MwiContainer.ActiveMwiChild != this)
                    MwiContainer.ActiveMwiChild = this;
            }

            if (!IsActive) IsActive = true;

            if (restoreMinimizedSize && (WindowState == WindowState.Minimized || (IsWindowed && ((Window)Parent).WindowState == WindowState.Minimized)))
                ToggleMinimize(null);

            if (IsWindowed && !((Window)Parent).IsKeyboardFocusWithin)
                ((Window)Parent).Focus();

            _isActivating = false;

            // BringIntoView(); // mouse click on element is not working in case of active BringIntoView -> need to delay BringIntoView
            // todo: delay for ResizingControl.Activate, cancel original RequestBringIntoView, make own BringIntoView (see Obsolete.AnimationHelper.ScrollViewerAnimator method)
            /*var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(300)};
            timer.Tick += OnDispatcherTimerTick;
            timer.Start();

            void OnDispatcherTimerTick(object sender, EventArgs e)
            {
                ((DispatcherTimer)sender).Stop();
                BringIntoView();
            }*/
        }

        public async void Close(object obj)
        {
            var cmdCloseBinding = CommandBindings.OfType<CommandBinding>().FirstOrDefault(c => Equals(c.Command, ApplicationCommands.Close));
            if (cmdCloseBinding == null)
                await AnimateHide();
            else  // Dialog
                ((RoutedUICommand)cmdCloseBinding.Command).Execute(null, this);

            if (Content is IDisposable disposable) disposable.Dispose();
            if (!IsWindowed && WindowState == WindowState.Maximized)
            {
                BindingOperations.ClearBinding(this, WidthProperty);
                BindingOperations.ClearBinding(this, HeightProperty);
            }
            if (IsWindowed) ((Window)Parent).Close();
            MwiContainer?.Children.Remove(this);

            Closed?.Invoke(this, EventArgs.Empty);
        }

        #region ==============  Thumbnail  ===================
        public ImageSource Thumbnail { get; private set; }
        public double ThumbnailWidth => GetThumbnailWidth();

        public void RefreshThumbnail()
        {
            // Execute before minimized, collapsed or mouse over on tab button
            if ((WindowState != WindowState.Minimized || Thumbnail == null) && Visibility == Visibility.Visible)
            {
                var bitmap = new RenderTargetBitmap(Convert.ToInt32(ActualWidth), Convert.ToInt32(ActualHeight), 96, 96, PixelFormats.Pbgra32);
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    var brush = new VisualBrush(this);
                    context.DrawRectangle(brush, null, new Rect(new Point(), new Size(ActualWidth, ActualHeight)));
                    context.Close();
                }

                bitmap.Render(drawingVisual);
                Thumbnail = bitmap;
                OnPropertiesChanged(nameof(Thumbnail), nameof(ThumbnailWidth));
            }
        }
        private double GetThumbnailWidth()
        {
            const double MAX_THUMBNAIL_SIZE = 180;
            var width = Thumbnail?.Width ?? 0;
            var height = Thumbnail?.Height ?? 0;
            var maxSize = Math.Max(width, height);
            var factor = maxSize > MAX_THUMBNAIL_SIZE ? MAX_THUMBNAIL_SIZE / maxSize : 1;
            return width * factor;
        }
        #endregion

        #region =============  Properties  =================
        public event EventHandler Closed;
        public MwiContainer MwiContainer { get; set; }
        public Color BaseColor
        {
            get
            {
                if (Theme?.Id == "Windows7") return MwiThemeInfo.Wnd7BaseColor;
                var backColor = Tips.GetColorFromBrush(Background);
                return backColor == Colors.Transparent ? MwiAppViewModel.Instance.AppColor : backColor;
            }
        }

        //============  Buttons  ============
        public bool IsCloseButtonVisible => (VisibleButtons & Buttons.Close) == Buttons.Close;
        public bool IsMinimizeButtonVisible => (VisibleButtons & Buttons.Minimize) == Buttons.Minimize && Resizable;
        public bool IsMaximizeButtonVisible => (VisibleButtons & Buttons.Maximize) == Buttons.Maximize && Resizable;
        public bool IsDetachButtonVisible => (VisibleButtons & Buttons.Detach) == Buttons.Detach;

        //============  Commands  =============
        public RelayCommand CmdDetach { get; }
        public RelayCommand CmdMinimize { get; }
        public RelayCommand CmdMaximize { get; }
        public RelayCommand SysCmdMaximize { get; }
        public RelayCommand SysCmdRestore { get; }
        public RelayCommand CmdClose { get; }
        //=========================
        public static readonly DependencyProperty AllowDetachProperty = DependencyProperty.Register(nameof(AllowDetach), typeof(bool), typeof(MwiChild), new FrameworkPropertyMetadata(true));
        public bool AllowDetach
        {
            get => (bool)GetValue(AllowDetachProperty);
            set => SetValue(AllowDetachProperty, value);
        }
        //================================
        public static readonly DependencyProperty AllowMinimizeProperty = DependencyProperty.Register(nameof(AllowMinimize), typeof(bool), typeof(MwiChild), new FrameworkPropertyMetadata(true));
        public bool AllowMinimize
        {
            get => (bool)GetValue(AllowMinimizeProperty);
            set => SetValue(AllowMinimizeProperty, value);
        }
        //================================
        public static readonly DependencyProperty AllowMaximizeProperty = DependencyProperty.Register(nameof(AllowMaximize), typeof(bool), typeof(MwiChild), new FrameworkPropertyMetadata(true));
        public bool AllowMaximize
        {
            get => (bool)GetValue(AllowMaximizeProperty);
            set => SetValue(AllowMaximizeProperty, value);
        }
        //================================
        public static readonly DependencyProperty AllowCloseProperty = DependencyProperty.Register(nameof(AllowClose), typeof(bool), typeof(MwiChild), new FrameworkPropertyMetadata(true));
        public bool AllowClose
        {
            get => (bool)GetValue(AllowCloseProperty);
            set => SetValue(AllowCloseProperty, value);
        }
        //================================
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(MwiChild), new FrameworkPropertyMetadata(false));
        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }
        //================================
        public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register("WindowState", typeof(WindowState), typeof(MwiChild), new FrameworkPropertyMetadata(WindowState.Normal, OnWindowStateValueChanged));
        public WindowState WindowState
        {
            get => (WindowState)GetValue(WindowStateProperty);
            set => SetValue(WindowStateProperty, value);
        }
        //================================
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        //================================
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MwiChild));
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        //================================
        public static readonly DependencyProperty LeftHeaderBarProperty = DependencyProperty.Register("LeftHeaderBar", typeof(FrameworkElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public FrameworkElement LeftHeaderBar
        {
            get => (FrameworkElement)GetValue(LeftHeaderBarProperty);
            set => SetValue(LeftHeaderBarProperty, value);
        }
        //================================
        public static readonly DependencyProperty RightHeaderBarProperty = DependencyProperty.Register("RightHeaderBar", typeof(FrameworkElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public FrameworkElement RightHeaderBar
        {
            get => (FrameworkElement)GetValue(RightHeaderBarProperty);
            set => SetValue(RightHeaderBarProperty, value);
        }
        //==============================
        public static readonly DependencyProperty StatusBarProperty = DependencyProperty.Register("StatusBar", typeof(FrameworkElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public FrameworkElement StatusBar
        {
            get => (FrameworkElement)GetValue(StatusBarProperty);
            set => SetValue(StatusBarProperty, value);
        }
        //==============================
        public static readonly DependencyProperty CommandBarProperty = DependencyProperty.Register("CommandBar", typeof(FrameworkElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public FrameworkElement CommandBar
        {
            get => (FrameworkElement)GetValue(CommandBarProperty);
            set => SetValue(CommandBarProperty, value);
        }
        //==============================
        public static readonly DependencyProperty VisibleButtonsProperty = DependencyProperty.Register("VisibleButtons",
            typeof(Buttons), typeof(MwiChild),
            new FrameworkPropertyMetadata(Buttons.Close | Buttons.Minimize | Buttons.Maximize | Buttons.Detach,
                (o, args) => ((MwiChild) o).UpdateUI()));
        public Buttons VisibleButtons
        {
            get => (Buttons)GetValue(VisibleButtonsProperty);
            set => SetValue(VisibleButtonsProperty, value);
        }
        //==============================
        public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register("Theme",
            typeof(MwiThemeInfo), typeof(MwiChild), new FrameworkPropertyMetadata(null, OnThemeChanged));

        public MwiThemeInfo Theme
        {
            get => (MwiThemeInfo)GetValue(ThemeProperty);
            set => SetValue(ThemeProperty, value);
        }
        private static async void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mwiChild = (MwiChild)d;
            if (e.NewValue is MwiThemeInfo themeInfo)
            {
                // Delay because no fill color for some icons
                await mwiChild.Dispatcher.BeginInvoke(new Action(() => { }), DispatcherPriority.Normal);

                foreach (var f1 in themeInfo.GetResources())
                    FillResources(mwiChild, f1);

                if (mwiChild.TryFindResource("Mwi.Common.BaseColorProxy") is BindingProxy colorProxy)
                    colorProxy.Value = mwiChild.BaseColor;
            }
        }
        private static void FillResources(FrameworkElement fe, ResourceDictionary resources)
        {
            foreach (var rd in resources.MergedDictionaries)
                FillResources(fe, rd);
            foreach (var key in resources.Keys.OfType<string>().Where(key => !key.StartsWith("Mwi.Container") && !key.StartsWith("Mwi.Bar")))
                fe.Resources[key] = resources[key];
        }

        #endregion

        private void UpdateUI() => OnPropertiesChanged(nameof(IsCloseButtonVisible), nameof(IsMaximizeButtonVisible),
            nameof(IsMinimizeButtonVisible), nameof(IsDetachButtonVisible));
    }
}
