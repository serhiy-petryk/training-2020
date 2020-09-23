using System;
using System.Windows;
using ColorInvestigation.Common;
using ColorInvestigation.Lib;
using ColorInvestigation.Views;

namespace ColorInvestigation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            // ColorSpacesTests.HueTest();
            // ColorSpacesTests.HslTest();
            // ColorSpacesTests.HsvTest();
            // ColorSpacesTests.LabTest();
            // ColorSpacesTests.XyzTest();

            // ColorSpacesTests.YCbCrTest(ColorSpaces.YCbCrStandard.BT601);
            ColorSpacesTests.YCbCrTest(ColorSpaces.YCbCrStandard.BT709);
            ColorSpacesTests.YCbCrTest(ColorSpaces.YCbCrStandard.BT2020);
        }

        private bool IsEqual(double d1, double d2) => Math.Abs(d1 - d2) < 0.0001;

        private void OnGrayScaleButtonClick(object sender, RoutedEventArgs e) => new GrayScale().Show();
        private void OnGrayScaleDiffButtonClick(object sender, RoutedEventArgs e) => new GrayScaleDiff().Show();
        private void OnCalcButtonClick(object sender, RoutedEventArgs e) => Temp.Calc.Calculate();
        private void OnColorSpacesButtonClick(object sender, RoutedEventArgs e) => new ColorSpacesForm().Show();
        private void OnForegroundButtonClick(object sender, RoutedEventArgs e) => new Foreground().Show();
        private void OnForegroundDiffButtonClick(object sender, RoutedEventArgs e) => new ForegroundDiff().Show();
        private void OnMonoChromaticButtonClick(object sender, RoutedEventArgs e) => new MonoChromatic().Show();
        private void OnColorPickerButtonClick(object sender, RoutedEventArgs e) => new ColorPickerTests().Show();

        private void OnChangeHueClick(object sender, RoutedEventArgs e)
        {
            var a1 = Application.Current.Resources["HueAndSaturation"] as DynamicBinding;
            var aa1 = ((string)a1.Value).Split(new[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
            var newHue = (int.Parse(aa1[0]) + 20) % 360;
            a1.Value = $"{newHue},{aa1[1]}";
        }

    }
}
