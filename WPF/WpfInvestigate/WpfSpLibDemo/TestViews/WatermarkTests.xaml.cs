using System.Windows;
using System.Windows.Media;
using WpfSpLib.Effects;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for TextBoxWatermarkTests.xaml
    /// </summary>
    public partial class WatermarkTests
    {
        public WatermarkTests()
        {
            InitializeComponent();
        }

        private void ClearCombobox_OnClick(object sender, RoutedEventArgs e)
        {
            // cb.SelectedIndex = -1;
        }

        private void ChangeWatermarkText_OnClick(object sender, RoutedEventArgs e)
        {
            var a = TestBox.GetValue(WatermarkEffect.WatermarkProperty) as string;
            TestBox.SetValue(WatermarkEffect.WatermarkProperty, a+a?.Length);
        }

        private void ChangeWatermarkForeground_OnClick(object sender, RoutedEventArgs e)
        {
            var a = TestBox.GetValue(WatermarkEffect.ForegroundProperty) as Brush;
            if (a!=null && a is SolidColorBrush && ((SolidColorBrush)a).Color == Colors.Red)
                TestBox.SetValue(WatermarkEffect.ForegroundProperty, Brushes.Green);
            else
                TestBox.SetValue(WatermarkEffect.ForegroundProperty, Brushes.Red);
        }
    }
}
