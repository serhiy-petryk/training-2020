using System.Windows;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for VirtualKeyboardTests.xaml
    /// </summary>
    public partial class VirtualKeyboardTests : Window
    {
        public VirtualKeyboardTests()
        {
            InitializeComponent();
        }

        private void ChangeHsl_OnClick(object sender, RoutedEventArgs e)
        {
            var hsl = Keyboard.BaseHsl;
            var a = (hsl.Hue + 30.0) % 360;
            hsl.Hue = a;
        }
    }
}
