using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfSpLib.Common;
using WpfSpLib.Controls;
using WpfSpLib.Themes;
using WpfSpLibDemo.Samples;

namespace WpfSpLibDemo.TestViews
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
            TestMwi = MwiContainer.Children.OfType<MwiChild>().FirstOrDefault(w => w.Title == "Window Using XAML");
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
                StatusBar = new MwiStatusBarSample(),
                CommandBar = new MwiCommandBarSample()
            // AllowDetach = false, AllowMinimize = false
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
            ((MwiChild)MwiContainer.Children[0]).Focus();
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
            var mwiContainer = ((MwiTests)Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive))?.MwiContainer;
            var adorner = new DialogAdorner(mwiContainer) {CloseOnClickBackground = true};

            var content = new MwiChild
            {
                Content = new ResizableSample(),
                LimitPositionToPanelBounds = true,
                VisibleButtons = MwiChild.Buttons.Close| MwiChild.Buttons.Maximize,
                Title="Dialog window"
            };
            adorner.ShowContentDialog(content);
        });

        public RelayCommand CmdShowMessage { get; } = new RelayCommand(o =>
        {
            var mwiContainer = ((MwiTests)Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive))?.MwiContainer;
            var result = DialogMessage.ShowDialog("Message text Message text Message text Message text Message text Message text",
                "Caption of Message block", DialogMessage.DialogMessageIcon.Success, new[] { "OK", "Cancel" }, true, mwiContainer);

            DialogMessage.ShowDialog($"You pressed '{result ?? "X" }' button", null, DialogMessage.DialogMessageIcon.Info, new[] { "OK" });
        });

        private void OnOpenDialogClick(object sender, RoutedEventArgs e)
        {
            var adorner = new DialogAdorner(MwiContainer) { CloseOnClickBackground = true };

            var content = new MwiChild
            {
                Content = new ResizableSample(),
                LimitPositionToPanelBounds = true,
                VisibleButtons = MwiChild.Buttons.Close | MwiChild.Buttons.Maximize,
                Title = "Dialog window"
            };
            adorner.ShowContentDialog(content);
        }

        private void AddDialog_OnClick(object sender, RoutedEventArgs e)
        {
            var content = new ResizableSample{HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top};
            var adorner = new DialogAdorner(MwiContainer) { CloseOnClickBackground = true };
            adorner.ShowContentDialog(content);
        }

        private void AddResizableDialog_OnClick(object sender, RoutedEventArgs e)
        {
            var content = new ResizableSample {Width = double.NaN, Height = double.NaN};
            var resizable = new ResizingControl{Content = content};
            var adorner = new DialogAdorner(MwiContainer);
            adorner.ShowContentDialog(resizable);
        }

        private void AddMwiDialog_OnClick(object sender, RoutedEventArgs e)
        {
            var content = new MwiChild
            {
                Content = new ResizableSample(),
                LimitPositionToPanelBounds = true,
                VisibleButtons = MwiChild.Buttons.Close | MwiChild.Buttons.Maximize,
                Title = "Dialog window"
            };
            var adorner = new DialogAdorner(MwiContainer) { CloseOnClickBackground = true };
            content.IsActive = true;
            adorner.ShowContentDialog(content);
        }

        private void OnTestClick(object sender, RoutedEventArgs e)
        {
            var a1 = sender as FrameworkElement;
            var aa1 = Tips.GetVisualParents(a1);
        }

        private void MwiTests_OnKeyDown(object sender, KeyEventArgs e)
        {
            var mwiContainer = MwiContainer;
            if (Keyboard.Modifiers == ModifierKeys.Control && Keyboard.IsKeyDown(Key.F4) && mwiContainer.ActiveMwiChild != null && !mwiContainer.ActiveMwiChild.IsWindowed) // Is Ctrl+F4 key pressed
            {
                mwiContainer.ActiveMwiChild.CmdClose.Execute(null);
                e.Handled = true;
            }
        }

        private void OnChangeColorClick(object sender, RoutedEventArgs e)
        {
            if (MwiContainer.ThemeColor != Colors.Green)
                MwiContainer.ThemeColor = Colors.Green;
            else
                MwiContainer.ThemeColor = Colors.Yellow;
        }

        private void OnChangeThemeClick(object sender, RoutedEventArgs e)
        {
            MwiContainer.Theme = MwiThemeInfo.GetNexThemeInfo(MwiContainer.ActualTheme);
            Debug.Print($"Theme: {MwiContainer.Theme.Name}");
        }
    }
}
