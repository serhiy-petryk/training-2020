using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiChild.xaml
    /// </summary>
    public partial class MwiChild
    {
        public bool IsDialog => false;
        public MwiChild()
        {
            InitializeComponent();
            DataContext = this;

            if (Icon == null)
                Icon = (ImageSource)FindResource("DefaultIcon");

            CmdDetach = new RelayCommand(ToggleDetach, _ => AllowDetach);
            CmdMinimize = new RelayCommand(ToggleMinimize, _ => AllowMinimize);
            CmdMaximize = new RelayCommand(ToggleMaximize, _ => AllowMaximize);
            SysCmdRestore = new RelayCommand(ToggleMaximize, _ => AllowMaximize && WindowState == WindowState.Maximized);
            SysCmdMaximize = new RelayCommand(ToggleMaximize, _ => AllowMaximize && WindowState != WindowState.Maximized);
            CmdClose = new RelayCommand(DoClose, _ => AllowClose);
        }

        private void DoClose(object obj)
        {
            DialogMessage.ShowDialog("need ToDo!");
        }

        private void ToggleMaximize(object obj)
        {
            DialogMessage.ShowDialog("need ToDo!");
        }

        private void ToggleMinimize(object obj)
        {
            DialogMessage.ShowDialog("need ToDo!");
        }

        private void ToggleDetach(object obj)
        {
            DialogMessage.ShowDialog("need ToDo!");
        }

        private void SystemMenuButton_OnChecked(object sender, RoutedEventArgs e) => Helpers.DropDownButtonHelper.OpenDropDownMenu(sender);

        private void MovingThumb_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DialogMessage.ShowDialog("need ToDo!");
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DialogMessage.ShowDialog("need ToDo!");
        }

        #region =============  Properties  =================
        //============  Commands  =============
        public RelayCommand CmdDetach { get; }
        public RelayCommand CmdMinimize { get; }
        public RelayCommand CmdMaximize { get; }
        public RelayCommand SysCmdMaximize { get; }
        public RelayCommand SysCmdRestore { get; }
        public RelayCommand CmdClose { get; }

        //============  Commands  =============
        public static readonly DependencyProperty AllowDetachProperty = DependencyProperty.Register(nameof(AllowDetach), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool AllowDetach
        {
            get => (bool)GetValue(AllowDetachProperty);
            set => SetValue(AllowDetachProperty, value);
        }
        public static readonly DependencyProperty AllowMinimizeProperty = DependencyProperty.Register(nameof(AllowMinimize), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        //================================
        public bool AllowMinimize
        {
            get => (bool)GetValue(AllowMinimizeProperty);
            set => SetValue(AllowMinimizeProperty, value);
        }

        public static readonly DependencyProperty AllowMaximizeProperty = DependencyProperty.Register(nameof(AllowMaximize), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        //================================
        public bool AllowMaximize
        {
            get => (bool)GetValue(AllowMaximizeProperty);
            set => SetValue(AllowMaximizeProperty, value);
        }

        public static readonly DependencyProperty AllowCloseProperty = DependencyProperty.Register(nameof(AllowClose), typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        //================================
        public bool AllowClose
        {
            get => (bool)GetValue(AllowCloseProperty);
            set => SetValue(AllowCloseProperty, value);
        }
        //================================
        public static readonly DependencyProperty ResizableProperty = DependencyProperty.Register(nameof(Resizable),
            typeof(bool), typeof(MwiChild), new UIPropertyMetadata(true));
        public bool Resizable
        {
            get => (bool)GetValue(ResizableProperty);
            set => SetValue(ResizableProperty, value);
        }
        //================================
        public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register("WindowState", typeof(WindowState), typeof(MwiChild), new UIPropertyMetadata(WindowState.Normal, OnWindowStateValueChanged));

        public WindowState WindowState
        {
            get { return (WindowState)GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }
        private static void OnWindowStateValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogMessage.ShowDialog("need ToDo!");
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
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(MwiChild), new UIPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        #endregion

    }
}
