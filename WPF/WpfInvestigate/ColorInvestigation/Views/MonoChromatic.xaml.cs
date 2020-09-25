using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorInvestigation.Common.ColorSpaces;

namespace ColorInvestigation.Views
{
    /// <summary>
    /// Interaction logic for MonoChromatic.xaml
    /// </summary>
    public partial class MonoChromatic : Window
    {
        public MonoChromatic()
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
                var back = new HSL(h / 100.0, s / 100.0, l / 100.0).RGB.Color;
                var grayLevel = Convert.ToByte(ColorUtils.GetGrayLevel(new RGB(back)));
                var btn = new Button { Width = 80, Height = 25, Content = h * 3.6 + " " + s + " " + l + " " + grayLevel, Background = new SolidColorBrush(back), Style = style };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnBlackAndWhiteAbsoluteStyleClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("BlackAndWhiteAbsoluteStyle") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = new HSL(h / 100.0, s / 100.0, l / 100.0).RGB.Color;
                var grayLevel = Convert.ToByte(ColorUtils.GetGrayLevel(new RGB(back)));
                var btn = new Button { Width = 80, Height = 25, Content = h * 3.6 + " " + s + " " + l + " " + grayLevel, Background = new SolidColorBrush(back), Style = style };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnBlackAndWhiteRelativeStyleClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("BlackAndWhiteRelativeStyle") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = new HSL(h / 100.0, s / 100.0, l / 100.0).RGB.Color;
                var grayLevel = Convert.ToByte(ColorUtils.GetGrayLevel(new RGB(back)));
                var btn = new Button { Width = 80, Height = 25, Content = h * 3.6 + " " + s + " " + l + " " + grayLevel, Background = new SolidColorBrush(back), Style = style };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnLabStyleClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("LabColorStyle") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var color = new HSL(h / 100.0, s / 100.0, l / 100.0).RGB.Color;
                var grayLevel = Convert.ToByte(ColorUtils.GetGrayLevel(new RGB(color)));
                var lab = new LAB(new RGB(color));
                var l2 = Convert.ToInt32(lab.L);
                var a = Convert.ToInt32(lab.A);
                var b = Convert.ToInt32(lab.B);
                var btn = new Button { Width = 80, Height = 25, Content = l2 + " " + a + " " + b + " " + grayLevel, Background = new SolidColorBrush(color), Style = style, BorderThickness = new Thickness(3) };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnTestStyleClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("TestColorStyle") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = new HSL(h / 100.0, s / 100.0, l / 100.0).RGB.Color;
                var grayLevel = Convert.ToByte(ColorUtils.GetGrayLevel(new RGB(back)));
                var btn = new Button { Width = 80, Height = 25, Content = h * 3.6 + " " + s + " " + l + " " + grayLevel, Background = new SolidColorBrush(back), Style = style };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnMonochromaticButtonStyleClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("MonochromeButtonStyle") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = new HSL(h / 100.0, s / 100.0, l / 100.0).RGB.Color;
                var grayLevel = Convert.ToByte(ColorUtils.GetGrayLevel(new RGB(back)));
                var btn = new Button
                {
                    Width = 80, Height = 25, Content = h * 3.6 + " " + s + " " + l + " " + grayLevel,
                    Background = new SolidColorBrush(back), BorderThickness = new Thickness(2), Style = style
                };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }
    }
}
