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
            ControlEffects.SetMonochrome(MonochromeButtonWhite, new SolidColorBrush(color));
            ControlEffects.SetMonochrome(MonochromeButtonBlack, new SolidColorBrush(color));
            ControlEffects.SetMonochromeAnimated(MonochromeAnimatedButtonWhite, new SolidColorBrush(color));
            ControlEffects.SetMonochromeAnimated(MonochromeAnimatedButtonBlack, new SolidColorBrush(color));
        }

        private void Bichrome_ChangeColor_OnClick(object sender, RoutedEventArgs e)
        {
            _hue = (_hue + 30) % 360;
            var color = new Common.ColorSpaces.HSL(_hue / 360.0, 1, 0.4).RGB.Color;
            ControlEffects.SetBichromeBackground(BichromeButtonWhite, new SolidColorBrush(color));
            ControlEffects.SetBichromeBackground(BichromeButtonBlack, new SolidColorBrush(color));
            ControlEffects.SetBichromeAnimatedBackground(BichromeAnimatedButtonWhite, new SolidColorBrush(color));
            ControlEffects.SetBichromeAnimatedBackground(BichromeAnimatedButtonBlack, new SolidColorBrush(color));
        }
    }
}
