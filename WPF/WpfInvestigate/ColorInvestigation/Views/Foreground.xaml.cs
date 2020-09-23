using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorInvestigation.Common;
using ColorInvestigation.Common.ColorSpaces;
using ColorInvestigation.Temp;

namespace ColorInvestigation.Views
{
    /// <summary>
    /// Interaction logic for Foreground.xaml
    /// </summary>
    public partial class Foreground : Window
    {
        public Foreground()
        {
            InitializeComponent();
        }

        public static double HslToGray(Color color)
        {
            var hsl = new HSL(new RGB(color));
            return hsl.L;
        }

        public static double YCbCrBT601(Color color)
        {
            var yCbCr = new YCbCr(new RGB(color), YCbCrStandard.BT601);
            return yCbCr.Y;
        }

        public static double YCbCrBT709(Color color)
        {
            var yCbCr = new YCbCr(new RGB(color), YCbCrStandard.BT709);
            return yCbCr.Y;
        }

        public static double YCbCrBT2020(Color color)
        {
            var yCbCr = new YCbCr(new RGB(color), YCbCrStandard.BT2020);
            return yCbCr.Y;
        }

        public static double YCbCrMy(Color color)
        {
            var yCbCr = new YCbCr(new RGB(color), YCbCrStandard.My);
            return yCbCr.Y;
        }

        private void UpdateData()
        {
            Func<Color, double> func = null;
            switch (Function.Text)
            {
                case "":
                    return;
                case "HSL":
                    func = HslToGray;
                    break;
                case "YCbCr.BT601":
                    func = YCbCrBT601;
                    break;
                case "YCbCr.BT709":
                    func = YCbCrBT709;
                    break;
                case "YCbCr.BT2020":
                    func = YCbCrBT2020;
                    break;
                case "YCbCr.My":
                    func = YCbCrMy;
                    break;
                case "ContrastingForegroundColor":
                    func = GrayScales.ContrastingForegroundColor;
                    break;
                default:
                    throw new Exception("DDDDDDDD");
            }
            Panel.Children.Clear();
            Panel.Children.Add(GetPanel(func));
        }

        private Panel GetPanel(Func<Color, double> func)
        {
            double min = double.Parse(From.Text, Tips.InvariantCulture);
            double max = double.Parse(To.Text, Tips.InvariantCulture);
            var pp = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
            IEnumerable<Color> colors;
            if (DataSet.IsChecked == false)
                colors = pp.Select(p => (Color)p.GetValue(null)).Distinct().OrderBy(func).ToArray();
            else
                colors = GrayScaleDiff.GetColors();

            var panel = new WrapPanel { Margin = new Thickness(10) };
            foreach (var color in colors.OrderBy(func))
            {
                var a = Convert.ToByte(func(color) * 100);
                if (a < max && a > min)
                {
                    var text1 = new TextBox
                    {
                        IsReadOnly = true,
                        Text = color + ": " + a,
                        Background = new SolidColorBrush(color),
                        Foreground = Brushes.White,
                        Width = 90
                    };
                    panel.Children.Add(text1);
                }

                if (a > min && a < max)
                {
                    var text2 = new TextBox
                    {
                        IsReadOnly = true,
                        Text = color + ": " + a,
                        Background = new SolidColorBrush(color),
                        Foreground = Brushes.Black,
                        Width = 90
                    };
                    panel.Children.Add(text2);
                }
            }
            return panel;
        }

        private void OnUpdateDtaButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }
    }
}
