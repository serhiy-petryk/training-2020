using System.Windows;
using System.Windows.Media;
using WpfInvestigate.Controls;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ControlEffectTests.xaml
    /// </summary>
    public partial class ControlEffectTests : Window
    {
        public ControlEffectTests()
        {
            InitializeComponent();
        }

        private int _hue;
        private void Monochrome_ChangeColor_OnClick(object sender, RoutedEventArgs e)
        {
            _hue = (_hue + 30) % 360;
            var color = new Common.ColorSpaces.HSL(_hue / 360.0, 1, 0.4).RGB.Color;
            ChromeEffects.SetMonochrome(MonochromeButtonWhite, new SolidColorBrush(color));
            ChromeEffects.SetMonochrome(MonochromeButtonBlack, new SolidColorBrush(color));
            ChromeEffects.SetMonochromeAnimated(MonochromeAnimatedButtonWhite, new SolidColorBrush(color));
            ChromeEffects.SetMonochromeAnimated(MonochromeAnimatedButtonBlack, new SolidColorBrush(color));
        }

        private void Bichrome_ChangeColor_OnClick(object sender, RoutedEventArgs e)
        {
            _hue = (_hue + 30) % 360;
            var color = new Common.ColorSpaces.HSL(_hue / 360.0, 1, 0.4).RGB.Color;
            ChromeEffects.SetBichromeBackground(BichromeButtonWhite, new SolidColorBrush(color));
            ChromeEffects.SetBichromeBackground(BichromeButtonBlack, new SolidColorBrush(color));
            ChromeEffects.SetBichromeAnimatedBackground(BichromeAnimatedButtonWhite, new SolidColorBrush(color));
            ChromeEffects.SetBichromeAnimatedBackground(BichromeAnimatedButtonBlack, new SolidColorBrush(color));
        }
    }
}
