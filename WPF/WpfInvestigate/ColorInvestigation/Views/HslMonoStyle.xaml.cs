using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorInvestigation.Lib;

namespace ColorInvestigation.Views
{
    /// <summary>
    /// Interaction logic for MonoChromatic.xaml
    /// </summary>
    public partial class HslMonoStyle : Window
    {
        public HslMonoStyle()
        {
            InitializeComponent();
        }

        private void OnHslMonoStyleClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("HslMonoStyle") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + l, Background = new SolidColorBrush(back), Style = style};
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 1800;
        }

        private void OnHslMonoStyleClick_BlackAndWhiteForeground(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("HslMonoStyle_BlackAndWhiteForeground") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var color = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                var grayLevel = Convert.ToByte(ColorUtilities.ColorToGrayLevel(color));
                var sGrayLevel = Math.Round(grayLevel / 255.0 * 100.0);
                var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + l + " " + sGrayLevel, Background = new SolidColorBrush(color), Style = style };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 1800;
        }
    }
}
