using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiChild.xaml
    /// </summary>
    public partial class MwiChild
    {
        [Flags]
        public enum Buttons
        {
            None = 0,
            Close = 1,
            Minimize = 2,
            Maximize = 4,
            Detach = 8
        }

        private static int controlId = 0;
        internal int _controlId = controlId++;
        public MwiChild()
        {
            CmdDetach = new RelayCommand(ToggleDetach, _ => AllowDetach);
            CmdMinimize = new RelayCommand(ToggleMinimize, _ => AllowMinimize);
            CmdMaximize = new RelayCommand(ToggleMaximize, _ => AllowMaximize);
            SysCmdRestore = new RelayCommand(ToggleMaximize, _ => AllowMaximize && WindowState == WindowState.Maximized);
            SysCmdMaximize = new RelayCommand(ToggleMaximize, _ => AllowMaximize && WindowState != WindowState.Maximized);
            CmdClose = new RelayCommand(Close, _ => AllowClose);

            InitializeComponent();
            DataContext = this;

            if (Icon == null) Icon = (ImageSource) FindResource("DefaultIcon");
        }

        #region =============  Override methods  ====================
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AnimateShow();
        }

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
                    : new Size(ActualWidth - 0.01, ActualHeight - 0.01);

                if (IsWindowed)
                    _detachedPosition = new Point(0, 0);
                else
                    _attachedPosition = new Point(0, 0);
                ToggleMaximize(null);
            }
        }

        public void Activate(bool restoreMinimizedSize)
        {
            if (_isActivating || MwiContainer == null) return;
            _isActivating = true;

            base.Activate();

            if (!IsActive)
                MwiContainer.InvalidateLayout();

            if (!IsActive && MwiContainer != null)
            {
                MwiContainer.Children.Where(c => c != this && c.IsActive).ToList().ForEach(c => c.IsActive = false);

                if (MwiContainer.ActiveMwiChild != this)
                    MwiContainer.ActiveMwiChild = this;
            }

            if (!IsActive) IsActive = true;

            if (restoreMinimizedSize && (WindowState == WindowState.Minimized || (IsWindowed && ((Window)Parent).WindowState == WindowState.Minimized)))
                ToggleMinimize(null);
            if (Window.GetWindow(this) is Window wnd && !wnd.IsFocused)
                wnd.Focus();

            _isActivating = false;
            BringIntoView();
        }

        #endregion

        public async void Close(object obj)
        {
            var cmdCloseBinding = CommandBindings.OfType<CommandBinding>().FirstOrDefault(c => Equals(c.Command, ApplicationCommands.Close));
            if (cmdCloseBinding == null)
                await AnimateHide();
            else  // Dialog
                ((RoutedUICommand)cmdCloseBinding.Command).Execute(null, this);

            if (Content is IDisposable) ((IDisposable)Content).Dispose();
            if (!IsWindowed && WindowState == WindowState.Maximized)
            {
                BindingOperations.ClearBinding(this, WidthProperty);
                BindingOperations.ClearBinding(this, HeightProperty);
            }
            if (IsWindowed) ((Window)Parent).Close();
            MwiContainer?.Children.Remove(this);

            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void SystemMenuButton_OnChecked(object sender, RoutedEventArgs e) => Helpers.DropDownButtonHelper.OpenDropDownMenu(sender);

        private void MovingThumb_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            element.IsEnabled = false;
            ToggleMaximize(null);
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => element.IsEnabled = true)); // nexttick action to prevent MoveThumb_OnDragDelta event
        }

        #region ==============  Thumbnail  ===================
        public void RefreshThumbnail() => OnPropertiesChanged(nameof(Thumbnail), nameof(ThumbnailWidth), nameof(ThumbnailHeight));
        public ImageSource Thumbnail => CreateThumbnail();
        public double ThumbnailWidth => GetThumbnailSize().X;
        public double ThumbnailHeight => GetThumbnailSize().Y;

        private ImageSource CreateThumbnail()
        {
            // Execute before minimized, collapsed or mouse over on tab button
            if ((WindowState != WindowState.Minimized || _thumbnailCache == null) && Visibility == Visibility.Visible)
            {
                var bitmap = new RenderTargetBitmap(Convert.ToInt32(ActualWidth), Convert.ToInt32(ActualHeight), 96, 96, PixelFormats.Default);
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    var brush = new VisualBrush(this);
                    context.DrawRectangle(brush, null, new Rect(new Point(), new Size(ActualWidth, ActualHeight)));
                    context.Close();
                }

                bitmap.Render(drawingVisual);
                _thumbnailCache = bitmap;
            }
            return _thumbnailCache;
        }
        private Point GetThumbnailSize()
        {
            const double MAX_THUMBNAIL_SIZE = 180;
            var width = Thumbnail?.Width ?? 0;
            var height = Thumbnail?.Height ?? 0;
            var maxSize = Math.Max(width, height);
            var factor = maxSize > MAX_THUMBNAIL_SIZE ? MAX_THUMBNAIL_SIZE / maxSize : 1;
            return new Point(width * factor, height * factor);
        }
        #endregion

        #region =============  Properties  =================
        public event EventHandler Closed;
        public bool IsDialog => false;
        public MwiContainer MwiContainer { get; set; }
        public bool IsSelected => MwiContainer?.ActiveMwiChild == this;

        //============  Commands  =============
        public RelayCommand CmdDetach { get; }
        public RelayCommand CmdMinimize { get; }
        public RelayCommand CmdMaximize { get; }
        public RelayCommand SysCmdMaximize { get; }
        public RelayCommand SysCmdRestore { get; }
        public RelayCommand CmdClose { get; }
        //=========================
        public static readonly DependencyProperty AllowDetachProperty = DependencyProperty.Register(nameof(AllowDetach), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool AllowDetach
        {
            get => (bool)GetValue(AllowDetachProperty);
            set => SetValue(AllowDetachProperty, value);
        }
        //================================
        public static readonly DependencyProperty AllowMinimizeProperty = DependencyProperty.Register(nameof(AllowMinimize), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool AllowMinimize
        {
            get => (bool)GetValue(AllowMinimizeProperty);
            set => SetValue(AllowMinimizeProperty, value);
        }
        //================================
        public static readonly DependencyProperty AllowMaximizeProperty = DependencyProperty.Register(nameof(AllowMaximize), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool AllowMaximize
        {
            get => (bool)GetValue(AllowMaximizeProperty);
            set => SetValue(AllowMaximizeProperty, value);
        }
        //================================
        public static readonly DependencyProperty AllowCloseProperty = DependencyProperty.Register(nameof(AllowClose), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool AllowClose
        {
            get => (bool)GetValue(AllowCloseProperty);
            set => SetValue(AllowCloseProperty, value);
        }
        //================================
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(false, OnIsActiveValueChanged));
        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }
        private static void OnIsActiveValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            /*var mwiChild = (MwiChild)sender;
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
                mwiChild.RaiseEvent(new RoutedEventArgs(LostFocusEvent, mwiChild));*/
        }

        //================================
        public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register("WindowState", typeof(WindowState), typeof(MwiChild), new UIPropertyMetadata(WindowState.Normal, OnWindowStateValueChanged));
        public WindowState WindowState
        {
            get => (WindowState)GetValue(WindowStateProperty);
            set => SetValue(WindowStateProperty, value);
        }
        //================================
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(MwiChild));
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
        //==============================
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(MwiChild), new UIPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
        //================================
        public static readonly DependencyProperty LeftHeaderPanelProperty = DependencyProperty.Register("LeftHeaderPanel", typeof(FrameworkElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public FrameworkElement LeftHeaderPanel
        {
            get => (FrameworkElement)GetValue(LeftHeaderPanelProperty);
            set => SetValue(LeftHeaderPanelProperty, value);
        }
        //================================
        public static readonly DependencyProperty RightHeaderPanelProperty = DependencyProperty.Register("RightHeaderPanel", typeof(FrameworkElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public FrameworkElement RightHeaderPanel
        {
            get => (FrameworkElement)GetValue(RightHeaderPanelProperty);
            set => SetValue(RightHeaderPanelProperty, value);
        }
        //==============================
        public static readonly DependencyProperty StatusBarProperty = DependencyProperty.Register("StatusBar", typeof(FrameworkElement), typeof(MwiChild), new FrameworkPropertyMetadata(null));
        public FrameworkElement StatusBar
        {
            get => (FrameworkElement)GetValue(StatusBarProperty);
            set => SetValue(StatusBarProperty, value);
        }
        //==============================
        public static readonly DependencyProperty VisibleButtonsProperty = DependencyProperty.Register("VisibleButtons", typeof(Buttons), typeof(MwiChild), new FrameworkPropertyMetadata(null, OnVisibleButtonsChanged));
        public Buttons VisibleButtons
        {
            get => (Buttons)GetValue(VisibleButtonsProperty);
            set => SetValue(VisibleButtonsProperty, value);
        }
        private static void OnVisibleButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Todo:
        }
        #endregion
    }
}
