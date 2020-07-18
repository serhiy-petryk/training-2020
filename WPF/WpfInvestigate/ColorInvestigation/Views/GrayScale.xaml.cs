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
    public partial class GrayScale : Window
    {
        private CultureInfo ci = CultureInfo.InvariantCulture;
        public GrayScale()
        {
            InitializeComponent();
        }

        private void UpdateData()
        {
            Func<Color, double> func = null;
            switch (Function.Text)
            {
                case "":
                    return;
                case "ColorToGrayScale":
                    func = ColorUtilities.ColorToGrayScale;
                    break;
                case "ColorToGrayScale 1":
                    func = ColorUtilities.ColorToGrayScale1;
                    break;
                case "ColorToGrayScale 2":
                    func = ColorUtilities.ColorToGrayScale2;
                    break;
                case "ColorToGrayScale 3":
                    func = ColorUtilities.ColorToGrayScale3;
                    break;
                case "ColorToGrayScale 4":
                    func = ColorUtilities.ColorToGrayScale4;
                    break;
                case "ColorToGrayScale 5":
                    func = ColorUtilities.ColorToGrayScale5;
                    break;
                case "ColorToGrayScale 6":
                    func = ColorUtilities.ColorToGrayScale6;
                    break;
                case "ContrastingForegroundColor":
                    func = ColorUtilities.ContrastingForegroundColor;
                    break;
                default:
                    throw new Exception("DDDDDDDD");
            }
            Panel.Children.Clear();
            Panel.Children.Add(GetPanel(func));
        }

        private Panel GetPanel(Func<Color, double> func)
        {
            double min = double.Parse(From.Text, ci);
            double max = double.Parse(To.Text, ci);
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
                        Foreground = new SolidColorBrush(Colors.White),
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
                        Foreground = new SolidColorBrush(Colors.Black),
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
