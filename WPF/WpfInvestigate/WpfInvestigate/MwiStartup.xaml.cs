using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;
using WpfInvestigate.Helpers;
using WpfInvestigate.Samples;
using WpfInvestigate.ViewModels;

namespace WpfInvestigate
{
    /// <summary>
    /// Interaction logic for MwiStartup.xaml
    /// </summary>
    public partial class MwiStartup
    {
        public RelayCommand CmdScaleSliderReset { get; private set; }

        public MwiStartup()
        {
            InitializeComponent();
            DataContext = this;
            CmdScaleSliderReset = new RelayCommand(p => ScaleSlider.Value = 1.0);

            // TopControl.RestoreRectFromSetting();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (TopControl.Template.FindName("ContentBorder", TopControl) is FrameworkElement topContentControl)
                    topContentControl.LayoutTransform = FindResource("Mwi.ScaleTransform") as ScaleTransform;
            }), DispatcherPriority.Normal);

            /*MwiAppViewModel.Instance.PropertyChanged += OnMwiAppViewModelInstancePropertyChanged;
            OnMwiAppViewModelInstancePropertyChanged(null, new PropertyChangedEventArgs(nameof(MwiAppViewModel.CurrentTheme)));

            void OnMwiAppViewModelInstancePropertyChanged(object sender, PropertyChangedEventArgs args)
            {
                if (args is PropertyChangedEventArgs e && e.PropertyName == nameof(MwiAppViewModel.CurrentTheme))
                    TestChild.BorderThickness = new Thickness(Equals(MwiAppViewModel.Instance.CurrentTheme.Id, "Windows7") ? 0 : 6);
            }*/
        }

        private void MwiStartup_OnKeyDown(object sender, KeyEventArgs e)
        {
            var mwiContainer = MwiContainer;
            if (Keyboard.Modifiers == ModifierKeys.Control && Keyboard.IsKeyDown(Key.F4) && mwiContainer.ActiveMwiChild != null && !mwiContainer.ActiveMwiChild.IsWindowed) // Is Ctrl+F4 key pressed
            {
                mwiContainer.ActiveMwiChild.CmdClose.Execute(null);
                e.Handled = true;
            }
        }

        // =============  Specific properties && methods  ============

        private static MwiChild TestMwi;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TestMwi = MwiContainer.Children?.OfType<MwiChild>().FirstOrDefault(w => w.Title == "Window Using XAML");
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
                Content = new ResizableSample {Width = double.NaN, Height = double.NaN},
                Width = 300,
                Height = 200,
                Position = new Point(300, 80)
            });
        }

        private void Test_OnClick(object sender, RoutedEventArgs e)
        {
            // foreach (var c in MwiContainer.Children)
            // c.AllowDetach = !c.AllowDetach;
            ((MwiChild) MwiContainer.Children[0]).Focus();
            var a1 = Keyboard.FocusedElement;
        }

        private void OpenWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var wnd = new Window();
            wnd.Show();
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var wnd = Window.GetWindow((DependencyObject) sender);
            var a1 = wnd.ActualWidth;
            var a2 = wnd.ActualHeight;
        }

        //============  Test window  =============
        public RelayCommand CmdDisableDetach { get; private set; } = new RelayCommand(o => TestMwi.AllowDetach = false);
        public RelayCommand CmdEnableDetach { get; private set; } = new RelayCommand(o => TestMwi.AllowDetach = true);
        public RelayCommand CmdDisableMinimize { get; private set; } = new RelayCommand(o => TestMwi.AllowMinimize = false);
        public RelayCommand CmdEnableMinimize { get; private set; } = new RelayCommand(o => TestMwi.AllowMinimize = true);
        public RelayCommand CmdDisableMaximize { get; private set; } = new RelayCommand(o => TestMwi.AllowMaximize = false);
        public RelayCommand CmdEnableMaximize { get; private set; } = new RelayCommand(o => TestMwi.AllowMaximize = true);
        public RelayCommand CmdDisableClose { get; private set; } = new RelayCommand(o => TestMwi.AllowClose = false);
        public RelayCommand CmdEnableClose { get; private set; } = new RelayCommand(o => TestMwi.AllowClose = true);

        public RelayCommand CmdHideIcon { get; private set; } = new RelayCommand(o =>
        {
            if (TestMwi.Icon != null)
            {
                TestMwi.Tag = TestMwi.Icon;
                TestMwi.Icon = null;
            }
        });

        public RelayCommand CmdShowIcon { get; private set; } = new RelayCommand(o =>
        {
            if (TestMwi.Tag is ImageSource tag)
                TestMwi.Icon = tag;
        });

        public RelayCommand CmdChangeTitle { get; private set; } = new RelayCommand(o => TestMwi.Title = "New " + TestMwi.Title);

        public RelayCommand CmdOpenDialog { get; private set; } = new RelayCommand(o =>
        {
            var content = new MwiChild
            {
                Content = new MwiExampleControl(),
                LimitPositionToPanelBounds = true,
                VisibleButtons = MwiChild.Buttons.Close | MwiChild.Buttons.Maximize,
                Title = "Dialog window",
                Width = 300,
                Height = 200,
                IsActive = true
            };
            var adorner = new DialogAdorner(MwiAppViewModel.Instance.DialogHost) {CloseOnClickBackground = true};
            adorner.ShowContentDialog(content);
        });

        public RelayCommand CmdShowMessage { get; private set; } = new RelayCommand(o =>
        {
            var result = DialogMessage.ShowDialog(
                "Message text Message text Message text Message text Message text Message text",
                "Caption of Message block", DialogMessage.DialogMessageIcon.Success, new[] {"OK", "Cancel"}, true,
                MwiAppViewModel.Instance.DialogHost);

            DialogMessage.ShowDialog($"You pressed '{result ?? "X"}' button", null,
                DialogMessage.DialogMessageIcon.Info, new[] {"OK"}, true, MwiAppViewModel.Instance.DialogHost);
        });

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var a1 in TestMwi.MwiContainer.Children.OfType<MwiChild>())
            {
                Debug.Print($"Child: {a1._controlId}, {a1.ActualWidth}, {a1.ActualHeight}");
            }
        }

        public void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.AutomaticUnloading(OnUnloaded))
            {
                TestMwi = null;
                Icon = null;
            }
        }
    }
}
