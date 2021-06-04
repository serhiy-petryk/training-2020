﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfInvestigate.Common;
using WpfInvestigate.Helpers;
using WpfInvestigate.Themes;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiChild.xaml
    /// </summary>
    public partial class MwiChild: ResizingControl, IColorThemeSupport
    {
        [Flags]
        public enum Buttons
        {
            Close = 1,
            Minimize = 2,
            Maximize = 4,
            Detach = 8,
            SelectTheme = 16
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
            CmdSelectTheme = new RelayCommand(o => this.SelectTheme(), _ => AllowSelectTheme);

            // DataContext = this;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            if (Icon == null) Icon = FindResource("Mwi.DefaultIcon") as ImageSource;
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

        private Window _activatedHost;
        protected override async void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            AddVisualParentChangedEvents();
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
        }

        public async void Close(object obj)
        {
            var cmdCloseBinding = CommandBindings.OfType<CommandBinding>().FirstOrDefault(c => Equals(c.Command, ApplicationCommands.Close));
            if (cmdCloseBinding == null)
                await AnimateHide();
            else  // Dialog
                ((RoutedUICommand)cmdCloseBinding.Command).Execute(null, this);

            // if (Content is IDisposable disposable) disposable.Dispose();
            if (!IsWindowed && WindowState == WindowState.Maximized)
            {
                BindingOperations.ClearBinding(this, WidthProperty);
                BindingOperations.ClearBinding(this, HeightProperty);
            }
            if (IsWindowed) ((Window)Parent).Close();
            MwiContainer?.Children?.Remove(this);

            Closed?.Invoke(this, EventArgs.Empty);
        }

        #region ==============  Thumbnail  ===================
        public ImageSource Thumbnail { get; private set; }
        public double ThumbnailWidth => GetThumbnailWidth();

        public void RefreshThumbnail()
        {
            // Execute before minimized, collapsed or mouse over on tab button
            if ((WindowState != WindowState.Minimized || Thumbnail == null) && Visibility == Visibility.Visible && IsLoaded)
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
        public MwiContainer MwiContainer; // !! Must be field, not property => important for clearing when unloaded

        //============  Buttons  ============
        public bool IsCloseButtonVisible => (VisibleButtons & Buttons.Close) == Buttons.Close;
        public bool IsMinimizeButtonVisible => (VisibleButtons & Buttons.Minimize) == Buttons.Minimize && Resizable && HostPanel != null;
        public bool IsMaximizeButtonVisible => (VisibleButtons & Buttons.Maximize) == Buttons.Maximize && Resizable && HostPanel != null;
        public bool IsDetachButtonVisible => (VisibleButtons & Buttons.Detach) == Buttons.Detach;
        public bool IsSelectThemeButtonVisible => (VisibleButtons & Buttons.SelectTheme) == Buttons.SelectTheme;

        //============  Commands  =============
        public RelayCommand CmdDetach { get; private set; }
        public RelayCommand CmdMinimize { get; private set; }
        public RelayCommand CmdMaximize { get; private set; }
        public RelayCommand SysCmdMaximize { get; private set; }
        public RelayCommand SysCmdRestore { get; private set; }
        public RelayCommand CmdClose { get; private set; }
        public RelayCommand CmdSelectTheme { get; private set; }
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
        public static readonly DependencyProperty AllowSelectThemeProperty = DependencyProperty.Register(nameof(AllowSelectTheme), typeof(bool), typeof(MwiChild), new FrameworkPropertyMetadata(true));
        public bool AllowSelectTheme
        {
            get => (bool)GetValue(AllowSelectThemeProperty);
            set => SetValue(AllowSelectThemeProperty, value);
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
            new FrameworkPropertyMetadata(Buttons.Close | Buttons.Minimize | Buttons.Maximize | Buttons.Detach  | Buttons.SelectTheme,
                (o, args) => ((MwiChild) o).UpdateUI()));
        public Buttons VisibleButtons
        {
            get => (Buttons)GetValue(VisibleButtonsProperty);
            set => SetValue(VisibleButtonsProperty, value);
        }
        //==============================
        #endregion

        #region ===============  IColorThemeSupport  =================
        public MwiThemeInfo ActualTheme => this.GetActualTheme();
        public Color ActualThemeColor => this.GetActualThemeColor();
        //=============
        public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register("Theme",
            typeof(MwiThemeInfo), typeof(MwiChild), new FrameworkPropertyMetadata(null, OnThemeChanged));

        public MwiThemeInfo Theme
        {
            get => (MwiThemeInfo)GetValue(ThemeProperty);
            set => SetValue(ThemeProperty, value);
        }
        private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MwiChild)d).UpdateColorTheme(false, true);
        }
        private static void FillResources(FrameworkElement fe, ResourceDictionary resources)
        {
            foreach (var rd in resources.MergedDictionaries)
                FillResources(fe, rd);
            foreach (var key in resources.Keys.OfType<string>().Where(key => !key.StartsWith("Mwi.Container") && !key.StartsWith("Mwi.Bar")))
                fe.Resources[key] = resources[key];
        }

        //==============================
        public static readonly DependencyProperty ThemeColorProperty = DependencyProperty.Register("ThemeColor",
            typeof(Color?), typeof(MwiChild), new FrameworkPropertyMetadata(null, OnThemeColorChanged));

        public Color? ThemeColor
        {
            get => (Color?)GetValue(ThemeColorProperty);
            set => SetValue(ThemeColorProperty, value);
        }
        private static void OnThemeColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MwiChild)d).UpdateColorTheme(true, true);
        }

        public void UpdateColorTheme(bool colorChanged, bool processChildren)
        {
            if (this.IsElementDisposing()) return;

            // Debug.Print($"MwiChild. UpdateColorTheme: {_controlId}");
            // Delay because no fill color for some icons
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!colorChanged && ActualTheme != null)
                {
                    foreach (var f1 in ActualTheme.GetResources())
                        FillResources(this, f1);
                }

                if (TryFindResource("Mwi.Child.BaseColorProxy") is BindingProxy colorProxy)
                    colorProxy.Value = ActualThemeColor;
            }), DispatcherPriority.Render);

            OnPropertiesChanged(nameof(ActualTheme), nameof(ActualThemeColor));
            if (processChildren)
                foreach (var element in this.GetVisualChildren().OfType<IColorThemeSupport>())
                    element.UpdateColorTheme(colorChanged, false);
        }
        #endregion

        private void UpdateUI() => OnPropertiesChanged(nameof(IsCloseButtonVisible), nameof(IsMaximizeButtonVisible),
            nameof(IsMinimizeButtonVisible), nameof(IsDetachButtonVisible), nameof(IsSelectThemeButtonVisible));
    }
}
