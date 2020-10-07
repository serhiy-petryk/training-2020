using System.Windows;
using System.Windows.Media;
using WpfInvestigate.Controls.Effects;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for BootstrapButtonTests.xaml
    /// </summary>
    public partial class BootstrapButtonTests : Window
    {
        public BootstrapButtonTests()
        {
            InitializeComponent();
        }

        private int _hue;
        private void Monochrome_ChangeColor_OnClick(object sender, RoutedEventArgs e)
        {
            _hue = (_hue + 30) % 360;
            var color = new Common.ColorSpaces.HSL(_hue / 360.0, 1, 0.4).RGB.Color;
            ChromeEffect.SetMonochrome(MonochromeButtonWhite, new SolidColorBrush(color));
            ChromeEffect.SetMonochrome(MonochromeButtonBlack, new SolidColorBrush(color));
            ChromeEffect.SetMonochromeAnimated(MonochromeAnimatedButtonWhite, new SolidColorBrush(color));
            ChromeEffect.SetMonochromeAnimated(MonochromeAnimatedButtonBlack, new SolidColorBrush(color));
        }

        private void Bichrome_ChangeColor_OnClick(object sender, RoutedEventArgs e)
        {
            _hue = (_hue + 30) % 360;
            var color = new Common.ColorSpaces.HSL(_hue / 360.0, 1, 0.4).RGB.Color;
            ChromeEffect.SetBichromeBackground(BichromeButtonWhite, new SolidColorBrush(color));
            ChromeEffect.SetBichromeBackground(BichromeButtonBlack, new SolidColorBrush(color));
            ChromeEffect.SetBichromeAnimatedBackground(BichromeAnimatedButtonWhite, new SolidColorBrush(color));
            ChromeEffect.SetBichromeAnimatedBackground(BichromeAnimatedButtonBlack, new SolidColorBrush(color));
        }
    }
}
