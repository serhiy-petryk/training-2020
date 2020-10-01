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
        private void ChangeColor_OnClick(object sender, RoutedEventArgs e)
        {
            _hue = (_hue + 30) % 360;
            var color = new Common.ColorSpaces.HSL(_hue / 360.0, 1, 0.4).RGB.Color;
            ControlEffects.SetMonochrome(TestButtonWhite, new SolidColorBrush(color));
            ControlEffects.SetMonochrome(TestButtonBlack, new SolidColorBrush(color));
        }
    }
}
