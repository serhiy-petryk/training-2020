using System.Windows;
using WpfInvestigate.Effects;

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
            ChromeEffect.SetMonochrome(MonochromeButtonWhite, color);
            ChromeEffect.SetMonochrome(MonochromeButtonBlack, color);
            ChromeEffect.SetMonochromeAnimated(MonochromeAnimatedButtonWhite, color);
            ChromeEffect.SetMonochromeAnimated(MonochromeAnimatedButtonBlack, color);
        }

        private void Bichrome_ChangeColor_OnClick(object sender, RoutedEventArgs e)
        {
            _hue = (_hue + 30) % 360;
            var color = new Common.ColorSpaces.HSL(_hue / 360.0, 1, 0.4).RGB.Color;
            ChromeEffect.SetBichromeBackground(BichromeButtonWhite, color);
            ChromeEffect.SetBichromeBackground(BichromeButtonBlack, color);
            ChromeEffect.SetBichromeAnimatedBackground(BichromeAnimatedButtonWhite, color);
            ChromeEffect.SetBichromeAnimatedBackground(BichromeAnimatedButtonBlack, color);
        }
    }
}
