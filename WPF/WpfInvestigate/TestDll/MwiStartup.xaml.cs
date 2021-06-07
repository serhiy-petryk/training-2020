using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Controls;
using WpfSpLib.Helpers;
using WpfSpLib.Themes;
using WpfSpLibDemo.Samples;

namespace TestDll
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
            CmdScaleSliderReset = new RelayCommand(p => ScaleSlider.Value = 1.0);

            // TopControl.RestoreRectFromSetting();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (TopControl.Template.FindName("ContentBorder", TopControl) is FrameworkElement topContentControl)
                {
                    if (!(topContentControl.LayoutTransform is ScaleTransform))
                        topContentControl.LayoutTransform = new ScaleTransform();
                    var transform = (ScaleTransform)topContentControl.LayoutTransform;
                    BindingOperations.SetBinding(transform, ScaleTransform.ScaleXProperty, new Binding("Value") { Source = ScaleSlider });
                    BindingOperations.SetBinding(transform, ScaleTransform.ScaleYProperty, new Binding("Value") { Source = ScaleSlider });
                }
            }), DispatcherPriority.Normal);
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

        private MwiChild TestMwi;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TestMwi = MwiContainer.Children?.OfType<MwiChild>().FirstOrDefault(w => w.Title == "Window Using XAML");

            if (TestMwi != null && TestMwi.Content is FrameworkElement fe)
                fe.DataContext = new WpfSpLibDemo.MwiStartup.TestViewModel(TestMwi);
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

        //============  Test window  =============

        #region ========  Subclass TestViewModel  ===========
        public class TestViewModel : DependencyObject, IDisposable
        {
            private MwiChild _owner;
            public RelayCommand CmdDisableDetach { get; private set; }
            public RelayCommand CmdEnableDetach { get; private set; }
            public RelayCommand CmdDisableMinimize { get; private set; }
            public RelayCommand CmdEnableMinimize { get; private set; }
            public RelayCommand CmdDisableMaximize { get; private set; }
            public RelayCommand CmdEnableMaximize { get; private set; }
            public RelayCommand CmdDisableClose { get; private set; }
            public RelayCommand CmdEnableClose { get; private set; }
            public RelayCommand CmdHideIcon { get; private set; }
            public RelayCommand CmdShowIcon { get; private set; }
            public RelayCommand CmdChangeTitle { get; private set; }
            public RelayCommand CmdOpenDialog { get; private set; }
            public RelayCommand CmdShowMessage { get; private set; }
            public TestViewModel(MwiChild owner)
            {
                _owner = owner;

                CmdDisableDetach = new RelayCommand(o => _owner.AllowDetach = false);
                CmdEnableDetach = new RelayCommand(o => _owner.AllowDetach = true);
                CmdDisableMinimize = new RelayCommand(o => _owner.AllowMinimize = false);
                CmdEnableMinimize = new RelayCommand(o => _owner.AllowMinimize = true);
                CmdDisableMaximize = new RelayCommand(o => _owner.AllowMaximize = false);
                CmdEnableMaximize = new RelayCommand(o => _owner.AllowMaximize = true);
                CmdDisableClose = new RelayCommand(o => _owner.AllowClose = false);
                CmdEnableClose = new RelayCommand(o => _owner.AllowClose = true);

                CmdHideIcon = new RelayCommand(o =>
                {
                    if (_owner.Icon != null)
                    {
                        _owner.Tag = _owner.Icon;
                        _owner.Icon = null;
                    }
                });

                CmdShowIcon = new RelayCommand(o =>
                {
                    if (_owner.Tag is ImageSource tag)
                        _owner.Icon = tag;
                });

                CmdChangeTitle = new RelayCommand(o => _owner.Title = "New " + _owner.Title);

                CmdOpenDialog = new RelayCommand(o =>
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
                    var adorner = new DialogAdorner(_owner.DialogHost) { CloseOnClickBackground = true };
                    adorner.ShowContentDialog(content);
                });

                CmdShowMessage = new RelayCommand(o =>
                {
                    var result = DialogMessage.ShowDialog(
                        "Message text Message text Message text Message text Message text Message text",
                        "Caption of Message block", DialogMessage.DialogMessageIcon.Success, new[] { "OK", "Cancel" }, true,
                        _owner.DialogHost);

                    DialogMessage.ShowDialog($"You pressed '{result ?? "X"}' button", null,
                        DialogMessage.DialogMessageIcon.Info, new[] { "OK" }, true, _owner.DialogHost);
                });
            }

            public void Dispose()
            {
                _owner = null;
                UnloadingHelper.ClearElement(this);
                EventHelper.RemoveWpfEventHandlers(this);
                // UnloadingHelper.UnloadElement(this);
            }
        }
        #endregion

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var a1 in TestMwi.MwiContainer.Children.OfType<MwiChild>())
            {
                Debug.Print($"Child: {a1.ActualWidth}, {a1.ActualHeight}");
            }
        }

        public void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.IsElementDisposing())
            {
                if (TestMwi != null && TestMwi.Content is FrameworkElement fe && fe.DataContext is WpfSpLibDemo.MwiStartup.TestViewModel vm)
                    vm.Dispose();
            }

            if (this.AutomaticUnloading(OnUnloaded))
            {
                if (TestMwi != null)
                {
                    TestMwi.DataContext = null;
                    TestMwi = null;
                }

                Icon = null;
                TopControl = null;
                ScaleSlider = null;
                MwiContainer = null;
            }
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TopControl.Theme = MwiThemeInfo.GetNexThemeInfo(TopControl.ActualTheme);
        }
    }
}
