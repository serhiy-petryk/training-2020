using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MyWpfMwi.Common;
using MyWpfMwi.Controls.DialogItems;
using MyWpfMwi.Examples;
using MyWpfMwi.Mwi;
using MyWpfMwi.ViewModels;

namespace MyWpfMwi
{
    /// <summary>
    /// Interaction logic for MwiStartup.xaml
    /// </summary>
    public partial class MwiStartup
    {
        public RelayCommand CmdScaleSliderReset { get; private set; }

        public static readonly DependencyProperty ScaleSliderProperty = DependencyProperty.Register(nameof(ScaleSlider), typeof(Slider), typeof(MwiStartup), new UIPropertyMetadata(null));
        public Slider ScaleSlider
        {
            get => (Slider)GetValue(ScaleSliderProperty);
            set => SetValue(ScaleSliderProperty, value);
        }

        public MwiStartup()
        {
            InitializeComponent();
            DataContext = this;
            TopControl.RestoreRectFromSetting();
            TopControl.CommandBar = new CommandBarExample();
        }

        private void MwiStartup_OnLoaded(object sender, RoutedEventArgs e)
        {
            AppViewModel.Instance.ContainerControl = Tips.GetVisualChildren(this).OfType<MwiContainer>().First(s => s.Uid == "Container");
            ScaleSlider = Tips.GetVisualChildren(this).OfType<Slider>().First(s => s.Uid == "ScaleSlider");
            Window1 = AppViewModel.Instance.ContainerControl?.Children.FirstOrDefault(w => w.Title == "Window Using XAML");
            CmdScaleSliderReset = new RelayCommand(p => ScaleSlider.Value = 1.0);
        }

        private void MwiStartup_OnUnloaded(object sender, RoutedEventArgs e) => TopControl.SaveRectToSetting();

        private void MwiStartup_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && Keyboard.IsKeyDown(Key.F4) &&
                AppViewModel.Instance.ContainerControl.ActiveMwiChild != null &&
                !AppViewModel.Instance.ContainerControl.ActiveMwiChild.IsWindowed) // Is Ctrl+F4 key pressed
            {
                AppViewModel.Instance.ContainerControl.ActiveMwiChild.CmdClose.Execute(null);
                e.Handled = true;
            }
        }

        private void MwiStartup_Active_OnChanged(object sender, EventArgs e) => TopControl.Focused = IsActive;

        //============  Test window  =============
        private static MwiChild Window1;
        public RelayCommand CmdDisableDetach { get; } = new RelayCommand(o => Window1.AllowDetach = false);
        public RelayCommand CmdEnableDetach { get; } = new RelayCommand(o => Window1.AllowDetach = true);
        public RelayCommand CmdDisableMinimize { get; } = new RelayCommand(o => Window1.AllowMinimize = false);
        public RelayCommand CmdEnableMinimize { get; } = new RelayCommand(o => Window1.AllowMinimize = true);
        public RelayCommand CmdDisableMaximize { get; } = new RelayCommand(o => Window1.AllowMaximize = false);
        public RelayCommand CmdEnableMaximize { get; } = new RelayCommand(o => Window1.AllowMaximize = true);
        public RelayCommand CmdDisableClose { get; } = new RelayCommand(o => Window1.AllowClose = false);
        public RelayCommand CmdEnableClose { get; } = new RelayCommand(o => Window1.AllowClose = true);
        public RelayCommand CmdHideIcon { get; } = new RelayCommand(o =>
        {
            if (Window1.Icon != null)
            {
                Window1.Tag = Window1.Icon;
                Window1.Icon = null;
            }
        });
        public RelayCommand CmdShowIcon { get; } = new RelayCommand(o =>
        {
            if (Window1.Tag is ImageSource)
                Window1.Icon = (ImageSource)Window1.Tag;
        });

        public RelayCommand CmdChangeTitle { get; } = new RelayCommand(o => Window1.Title = "New " + Window1.Title);

        public RelayCommand CmdOpenDialog { get; } = new RelayCommand(o =>
        {
            var container = AppViewModel.Instance.ContainerControl;
            var dialog = new MwiChild {AllowMaximize = false, AllowMinimize = false, AllowDetach = false};
            dialog.Content = new TextBlock { Text = "AAAAAAAAAAAAAAAAAA", Background = new SolidColorBrush(Colors.Green) };
            dialog.Title = "Dialog";
            if (container.ActiveMwiChild.IsWindowed)
                DialogItems.Show(container.ActiveMwiChild, dialog, GetAfterCreationCallbackForDialog(dialog, true));
            else
                DialogItems.Show(container, dialog, GetAfterCreationCallbackForDialog(dialog, true));
        });

        private static Action<DialogItems> GetAfterCreationCallbackForDialog(FrameworkElement content, bool closeOnClickBackground)
        {
            return dialogItems =>
            {
                content.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    dialogItems.CloseOnClickBackground = closeOnClickBackground;

                    // set absolute positioning for moving
                    if (dialogItems.ItemsHostPanel != null)
                    {
                        dialogItems.ItemsHostPanel.HorizontalAlignment = HorizontalAlignment.Left;
                        dialogItems.ItemsHostPanel.VerticalAlignment = VerticalAlignment.Top;
                    }

                    // clear moving area margin
                    if (VisualTreeHelper.GetParent(content) is ContentPresenter contentPresenter)
                        contentPresenter.Margin = new Thickness(0);

                    // center content position
                    var mwiChild = (MwiChild)dialogItems.Items[0];
                    mwiChild.Position = new Point(Math.Max(0, (dialogItems.ItemsPresenter.ActualWidth - content.ActualWidth) / 2),
                        Math.Max(0, (dialogItems.ItemsPresenter.ActualHeight - content.ActualHeight) / 2));
                }));
            };
        }

    }
}
