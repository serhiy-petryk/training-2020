using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;
using WpfInvestigate.Samples;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for MwiTests.xaml
    /// </summary>
    public partial class MwiTests : Window
    {
        public MwiTests()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TestMwi = MwiContainer.Children.FirstOrDefault(w => w.Title == "Window Using XAML");
        }

        private int cnt = 0;
        private void AddChild_OnClick(object sender, RoutedEventArgs e)
        {
            MwiContainer.Children.Add(new MwiChild
            {
                Title = "Window Using Code",
                Content = $"New MwiChild: {cnt++}",
                Width = 300,
                Height = 200,
                StatusBar = new MwiStatusBarSample()
            });
        }

        private void AddChild2_OnClick(object sender, RoutedEventArgs e)
        {
            MwiContainer.Children.Add(new MwiChild
            {
                Title = "Window Using Code",
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 300,
                Height = 200,
                Position = new Point(300, 80)
            });
        }

        private void Test_OnClick(object sender, RoutedEventArgs e)
        {
            // foreach (var c in MwiContainer.Children)
            // c.AllowDetach = !c.AllowDetach;
            MwiContainer.Children[0].Focus();
            var a1 = Keyboard.FocusedElement;
        }

        private void OpenWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var wnd = new Window();
            wnd.Show();
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var wnd = Window.GetWindow((DependencyObject)sender);
            var a1 = wnd.ActualWidth;
            var a2 = wnd.ActualHeight;
        }

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
        }

        //============  Test window  =============
        private static MwiChild TestMwi;
        public RelayCommand CmdDisableDetach { get; } = new RelayCommand(o => TestMwi.AllowDetach = false);
        public RelayCommand CmdEnableDetach { get; } = new RelayCommand(o => TestMwi.AllowDetach = true);
        public RelayCommand CmdDisableMinimize { get; } = new RelayCommand(o => TestMwi.AllowMinimize = false);
        public RelayCommand CmdEnableMinimize { get; } = new RelayCommand(o => TestMwi.AllowMinimize = true);
        public RelayCommand CmdDisableMaximize { get; } = new RelayCommand(o => TestMwi.AllowMaximize = false);
        public RelayCommand CmdEnableMaximize { get; } = new RelayCommand(o => TestMwi.AllowMaximize = true);
        public RelayCommand CmdDisableClose { get; } = new RelayCommand(o => TestMwi.AllowClose = false);
        public RelayCommand CmdEnableClose { get; } = new RelayCommand(o => TestMwi.AllowClose = true);
        public RelayCommand CmdHideIcon { get; } = new RelayCommand(o =>
        {
            if (TestMwi.Icon != null)
            {
                TestMwi.Tag = TestMwi.Icon;
                TestMwi.Icon = null;
            }
        });
        public RelayCommand CmdShowIcon { get; } = new RelayCommand(o =>
        {
            if (TestMwi.Tag is ImageSource tag)
                TestMwi.Icon = tag;
        });

        public RelayCommand CmdChangeTitle { get; } = new RelayCommand(o => TestMwi.Title = "New " + TestMwi.Title);

        public RelayCommand CmdOpenDialog { get; } = new RelayCommand(o =>
        {
            // Tips.ShowMwiChildDialog(new TextBlock { Text = "Test dialog window", Background = new SolidColorBrush(Colors.Green) }, "Dialog");
        });

        public RelayCommand CmdShowMessage { get; } = new RelayCommand(o =>
        {
            /*var aa = MessageBlock.Show("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Warning, new[] { "OK", "Cancel" });
            if (aa != null)
                MessageBlock.Show($"You pressed '{aa}' button", null, MessageBlock.MessageBlockIcon.Information);*/
        });

    }
}
