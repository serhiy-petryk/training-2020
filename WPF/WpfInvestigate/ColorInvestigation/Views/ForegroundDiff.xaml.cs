using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorInvestigation.Common;
using ColorInvestigation.Temp;

namespace ColorInvestigation.Views
{
    /// <summary>
    /// Interaction logic for ForegroundDiff.xaml
    /// </summary>
    public partial class ForegroundDiff : Window
    {
        public ForegroundDiff()
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

            Func<Color, double> func1 = GetDataSetFunc(Function1.Text);
            Func<Color, double> func2 = GetDataSetFunc(Function2.Text);
            if (func1 == null || func2 == null)
                return;

            var pp = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
            // var colors = pp.Select(p => (Color)p.GetValue(null)).Distinct().ToArray();
            var colors = GetColors();
            var colors1 = Sorted.IsChecked == true
                ? colors.OrderBy(func1)
                : colors.OrderBy(func2);

            var cnt = 0;

            var split1 = double.Parse(Split1.Text, Tips.InvariantCulture)/100;
            var split2 = double.Parse(Split2.Text, Tips.InvariantCulture)/100;
            foreach (var color in colors1)
            {
                var c1 = func1(color);
                var c2 = func2(color);
                var a1 = c1 < split1 ? Colors.White : Colors.Black;
                var a2 = c2 < split2 ? Colors.White : Colors.Black;

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
                case "HSL":
                    return Views.Foreground.HslToGray;
                case "YCbCr.BT601":
                    return Views.Foreground.YCbCrBT601;
                case "YCbCr.BT709":
                    return Views.Foreground.YCbCrBT709;
                case "YCbCr.BT2020":
                    return Views.Foreground.YCbCrBT2020;
                case "YCbCr.My":
                    return Views.Foreground.YCbCrMy;
                case "ContrastingForegroundColor":
                    return GrayScales.ContrastingForegroundColor;
                default:
                    throw new Exception("DDDDDDDD");
            }

        }
    }
}
