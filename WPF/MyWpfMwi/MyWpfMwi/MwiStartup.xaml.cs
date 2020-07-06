using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MyWpfMwi.Common;
using MyWpfMwi.Controls.Dialog;
using MyWpfMwi.Examples;
using MyWpfMwi.Mwi;

namespace MyWpfMwi
{
    /// <summary>
    /// Interaction logic for MwiStartup.xaml
    /// </summary>
    public partial class MwiStartup
    {
        private MwiContainer MwiContainer { get; set; }
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
            ScaleSlider = Tips.GetVisualChildren(this).OfType<Slider>().First(s => s.Uid == "ScaleSlider");
            MwiContainer = Tips.GetVisualChildren(this).OfType<MwiContainer>().First(s => s.Uid == "Container");
            Cont = MwiContainer;
            Window1 = MwiContainer?.Children.FirstOrDefault(w => w.Title == "Window Using XAML");
            CmdScaleSliderReset = new RelayCommand(p => ScaleSlider.Value = 1.0);
        }

        private void MwiStartup_OnUnloaded(object sender, RoutedEventArgs e) => TopControl.SaveRectToSetting();

        private void MwiStartup_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && Keyboard.IsKeyDown(Key.F4) && MwiContainer.ActiveMwiChild != null && !MwiContainer.ActiveMwiChild.IsWindowed) // Is Ctrl+F4 key pressed
            {
                MwiContainer.ActiveMwiChild.CmdClose.Execute(null);
                e.Handled = true;
            }
        }

        private void MwiStartup_Active_OnChanged(object sender, EventArgs e) => TopControl.Focused = IsActive;

        //============  Test window  =============
        private static MwiChild Window1;
        private static MwiContainer Cont;
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
            var a1 = new MwiChild();
            a1.Content = new TextBlock {Text = "AAAAAAAAAAAAAAAAAA", Background = new SolidColorBrush(Colors.Red)};
            // Cont.Children.Add(a1);
            a1.Title = "AAAAA";
            LightBox.ShowDialog(Cont, a1);
            /*var wnd = Window.GetWindow(Cont);
            Cont.Opacity = 0.5;
            Cont.IsHitTestVisible = false;
            p.Child = a1;
            p.IsOpen = true;
            p.StaysOpen = true;
            var a = Cont.Content;*/
        });
    }
}
