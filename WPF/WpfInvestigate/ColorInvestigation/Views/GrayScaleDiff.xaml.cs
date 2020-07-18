using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorInvestigation.Lib;

namespace ColorInvestigation.Views
{
    /// <summary>
    /// Interaction logic for GrayScale.xaml
    /// </summary>
    public partial class GrayScaleDiff : Window
    {
        private CultureInfo ci = CultureInfo.InvariantCulture;
        public GrayScaleDiff()
        {
            InitializeComponent();
        }

        public static IEnumerable<Color> GetColors(int step = 25)
        {
            var colors = new List<Color>();
            for (var k1 = 0; k1 < 256; k1 += step)
            for (var k2 = 0; k2 < 256; k2 += step)
            for (var k3 = 0; k3 < 256; k3 += step)
                colors.Add(Color.FromRgb(Convert.ToByte(k1), Convert.ToByte(k2), Convert.ToByte(k3)));
            return colors;
        }

        private void UpdateData()
        {
            var panel = new WrapPanel { Margin = new Thickness(10) };
            Panel.Children.Clear();
            Panel.Children.Add(panel);

            // Func<Color, double> func1 = ColorUtilities.ColorToGrayScale;
            // Func<Color, double> func2 = ContrastingForegroundColor;
            Func<Color, double> func1 = GetDataSetFunc(Function1.Text);
            Func<Color, double> func2 = GetDataSetFunc(Function2.Text);
            if (func1 == null || func2 == null)
                return;

            var pp = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
            // var colors = pp.Select(p => (Color)p.GetValue(null)).Distinct().ToArray();
            var colors = GetColors();
            var colors1 = Sorted.IsChecked == true
                ? colors.OrderBy(ColorUtilities.ColorToGrayScale1)
                : colors.OrderBy(ColorUtilities.ContrastingForegroundColor);

            var cnt = 0;

            var split1 = double.Parse(Split1.Text, ci)/100;
            var split2 = double.Parse(Split2.Text, ci)/100;
            foreach (var color in colors1)
            {
                var c1 = func1(color);
                var c2 = func2(color);
                // var a1 = c1 < 0.54 ? Colors.White : Colors.Black;// ColorUtilities.ColorToGrayScale;
                // var a1 = c1 < 0.54 ? Colors.White : Colors.Black;// ColorUtilities.ColorToGrayScale4;
                var a1 = c1 < split1 ? Colors.White : Colors.Black;// ColorUtilities.ColorToGrayScale4;
                // var a2 = c2 < 0.54 ? Colors.White : Colors.Black; // ColorUtilities.ColorToGrayScale4;
                // var a2 = c2 < 0.35 ? Colors.White : Colors.Black; // ContrastingForegroundColor
                // var a2 = c2 < 0.35 ? Colors.White : Colors.Black; // ContrastingForegroundColor
                var a2 = c2 < split2 ? Colors.White : Colors.Black; // ContrastingForegroundColor

                var x1 = Convert.ToInt16(c1 * 100);
                var x2 = Convert.ToInt16(c2 * 100);
                if (a1 == a2) continue;

                var text1 = new TextBox
                {
                    IsReadOnly = true,
                    Text = color.ToString()+ ": " + x1,
                    Background = new SolidColorBrush(color),
                    Foreground = new SolidColorBrush(a1),
                    Width = 110
                };
                panel.Children.Add(text1);

                var text2 = new TextBox
                {
                    IsReadOnly = true,
                    Text = color + ": " + x2,
                    Background = new SolidColorBrush(color),
                    Foreground = new SolidColorBrush(a2),
                    Width = 110
                };
                panel.Children.Add(text2);
                cnt++;
            }

            Count.Text = "Count: " + cnt;
        }

        private void OnCheckBoxClick(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        private void OnUpdateDtaButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        private Func<Color, double> GetDataSetFunc(string functionName)
        {
            switch (functionName)
            {
                case "":
                    return null;
                case "ColorToGrayScale":
                    return ColorUtilities.ColorToGrayScale;
                case "ColorToGrayScale 1":
                    return ColorUtilities.ColorToGrayScale1;
                case "ColorToGrayScale 2":
                    return ColorUtilities.ColorToGrayScale2;
                case "ColorToGrayScale 3":
                    return ColorUtilities.ColorToGrayScale3;
                case "ColorToGrayScale 4":
                    return ColorUtilities.ColorToGrayScale4;
                case "ColorToGrayScale 5":
                    return ColorUtilities.ColorToGrayScale5;
                case "ColorToGrayScale 6":
                    return ColorUtilities.ColorToGrayScale6;
                case "ContrastingForegroundColor":
                    return ColorUtilities.ContrastingForegroundColor;
                default:
                    throw new Exception("DDDDDDDD");
            }

        }
    }
}
