using System;
using System.Windows;
using System.Windows.Media;
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
            var a32 = ColorUtilities.ColorToXyz(Color.FromRgb(0, 0, 0x9E));
            var a42 = ColorUtilities.ColorToLab(Color.FromRgb(0, 0, 0x20));
            var rgb = new ColorSpaces.RGB(Color.FromRgb(0, 0, 0x20));
            var lab = new ColorSpaces.LAB(rgb);
            // ColorSpacesTests.HueTest();
            // ColorSpacesTests.HslTest();
            // ColorSpacesTests.HsvTest();
            ColorSpacesTests.XyzTest();

            var c21 = Color.FromRgb(169, 104, 54);
            var c22 = ColorUtilities.ColorToYCbCr(c21, ColorUtilities.YCbCrStandard.BT601);
            var c23 = ColorUtilities.YCbCrToColor(c22.Item1, c22.Item2, c22.Item3, ColorUtilities.YCbCrStandard.BT601);
            ColorSpacesTests.YCbCrTest(ColorSpaces.YCbCrStandard.BT601);
            ColorSpacesTests.YCbCrTest(ColorSpaces.YCbCrStandard.BT709);
            ColorSpacesTests.YCbCrTest(ColorSpaces.YCbCrStandard.BT2020);


            var a1 = ColorUtilities.ColorToXyz(Colors.White);
            var a2 = ColorUtilities.XyzToColor(a1.Item1, a1.Item2, a1.Item3);
            var a3 = ColorUtilities.XyzToColor(95.047, 100.000, 108.883);
            var a31 = ColorUtilities.XyzToColor(95, 100.000, 109);

            // ColorXyz.Test(); // OK! 
            // ColorLab.Test(); // OK! 
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
