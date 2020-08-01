using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorInvestigation.Lib;

namespace ColorInvestigation.Views
{
    /// <summary>
    /// Interaction logic for MonoChromatic.xaml
    /// </summary>
    public partial class ColorSpaces : Window
    {
        public ColorSpaces()
        {
            InitializeComponent();
        }

        private void OnShowHSLClick_07(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 5;
            for (var h = 0; h <= 100; h += step)
                for (var s = 0; s <= 100; s += step)
                {
                    var back = ColorUtilities.HslToColor(h / 100.0, s / 100.0, 0.7);
                    // var btn = new Button { Width = 100, Height = 25, Content = back.ToString(), Background = new SolidColorBrush(back) };
                    var btn = new Button { Width = 80, Height = 25, Content = h + " " + s, Background = new SolidColorBrush(back) };
                    Panel.Children.Add(btn);
                }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 1750;
        }

        private void OnShowHSVClick_07(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 5;
            for (var h = 0; h <= 100; h += step)
                for (var s = 0; s <= 100; s += step)
                {
                    var back = ColorUtilities.HsvToColor(h / 100.0, s / 100.0, 0.7);
                    var btn = new Button { Width = 80, Height = 25, Content = h + " " + s, Background = new SolidColorBrush(back) };
                    Panel.Children.Add(btn);
                }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 1750;
        }

        private void OnShowHSLClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            for (var l = 0; l <= 100; l += step)
                for (var h = 0; h <= 100; h += step)
                    for (var s = 0; s <= 100; s += step)
                    {
                        var back = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                        var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + l, Background = new SolidColorBrush(back) };
                        Panel.Children.Add(btn);
                    }

            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnShowHSLClick_LOrder(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            for (var h = 0; h <= 100; h += step)
                for (var s = 0; s <= 100; s += step)
                    for (var l = 0; l <= 100; l += step)
                    {
                        var back = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                        var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + l, Background = new SolidColorBrush(back) };
                        Panel.Children.Add(btn);
                    }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnShowHSVClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            for (var v = 0; v <= 100; v += step)
                for (var h = 0; h <= 100; h += step)
                    for (var s = 0; s <= 100; s += step)
                    {
                        var back = ColorUtilities.HsvToColor(h / 100.0, s / 100.0, v / 100.0);
                        var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + v, Background = new SolidColorBrush(back) };
                        Panel.Children.Add(btn);
                    }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnShowHSVClick_VOrder(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            for (var h = 0; h <= 100; h += step)
                for (var s = 0; s <= 100; s += step)
                    for (var v = 0; v <= 100; v += step)
                    {
                        var back = ColorUtilities.HsvToColor(h / 100.0, s / 100.0, v / 100.0);
                        var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + v, Background = new SolidColorBrush(back) };
                        Panel.Children.Add(btn);
                    }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnShowXYZClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            for (var x = 0; x <= 100; x += step)
                for (var y = 0; y <= 100; y += step)
                    for (var z = 0; z <= 110; z += step)
                    {
                        var back = ColorUtilities.XyzToColor(1.0 * x, 1.0 * y, 1.0 * z);
                        var btn = new Button { Width = 80, Height = 25, Content = x + " " + y + " " + z, Background = new SolidColorBrush(back) };
                        Panel.Children.Add(btn);
                    }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 1000;
        }

        private void OnShowXYZClick_XOrder(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            for (var y = 0; y <= 100; y += step)
                for (var z = 0; z <= 110; z += step)
                    for (var x = 0; x <= 100; x += step)
                    {
                        var back = ColorUtilities.XyzToColor(1.0 * x, 1.0 * y, 1.0 * z);
                        var btn = new Button { Width = 80, Height = 25, Content = x + " " + y + " " + z, Background = new SolidColorBrush(back) };
                        Panel.Children.Add(btn);
                    }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnShowLabClick_LOrder(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var step2 = 16;
            for (var a = -128; a <= 128; a += step2)
            for (var b = -128; b <= 128; b += step2)
            for (var l = 0; l <= 100; l += step)
            {
                var back = ColorUtilities.LabToColor(1.0 * l, 1.0 * a, 1.0 * b);
                var btn = new Button { Width = 80, Height = 25, Content = l + " " + a + " " + b, Background = new SolidColorBrush(back) };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnShowLabClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var step2 = 16;
            for (var l = 0; l <= 100; l += step)
            for (var a = -128; a <= 128; a += step2)
            for (var b = -128; b <= 128; b += step2)
            {
                var back = ColorUtilities.LabToColor(1.0 * l, 1.0 * a, 1.0 * b);
                var btn = new Button { Width = 80, Height = 25, Content = l + " " + a + " " + b, Background = new SolidColorBrush(back) };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 1400;
        }

        private void ShowYCbCr_CbCrOrder(ColorUtilities.YCbCrType yCbCrType)
        {
            Panel.Children.Clear();
            var step = 16;
            var step2 = 16;
            for (var Y = 0; Y <= 256; Y += step)
            for (var cB = -128; cB <= 128; cB += step2)
            for (var cR = -128; cR <= 128; cR += step2)
            {
                var back = ColorUtilities.YCbCrToColor(Y, cB, cR, yCbCrType);
                var fore = Y < 128 ? Color.FromRgb(0xCC, 0xCC, 0xCC) : Color.FromRgb(0x33, 0x33, 0x33);
                var btn = new Button
                {
                    Width = 80,
                    Height = 25,
                    Content = Y + " " + cB + " " + cR,
                    Background = new SolidColorBrush(back),
                    Foreground = new SolidColorBrush(fore)
                };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 1400;
        }
        private void ShowYCbCr_YOrder(ColorUtilities.YCbCrType yCbCrType)
        {
            Panel.Children.Clear();
            var step = 16;
            var step2 = 16;
            for (var cB = -128; cB <= 128; cB += step2)
            for (var cR = -128; cR <= 128; cR += step2)
            for (var Y = 0; Y <= 256; Y += step)
            {
                var back = ColorUtilities.YCbCrToColor(Y, cB, cR, yCbCrType);
                var fore = Y < 128 ? Color.FromRgb(0xCC, 0xCC, 0xCC) : Color.FromRgb(0x33, 0x33, 0x33);
                var btn = new Button
                {
                    Width = 80,
                    Height = 25,
                    Content = Y + " " + cB + " " + cR,
                    Background = new SolidColorBrush(back),
                    Foreground = new SolidColorBrush(fore)
                };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 1400;
        }

        private void OnShowBT601Click_YOrder(object sender, RoutedEventArgs e)
        {
            ShowYCbCr_YOrder(ColorUtilities.YCbCrType.BT601);
        }

        private void OnShowBT709Click_YOrder(object sender, RoutedEventArgs e)
        {
            ShowYCbCr_YOrder(ColorUtilities.YCbCrType.BT709);
        }

        private void OnShowBT2020Click_YOrder(object sender, RoutedEventArgs e)
        {
            ShowYCbCr_YOrder(ColorUtilities.YCbCrType.BT2020);
        }

        private void OnShowBT601Click(object sender, RoutedEventArgs e)
        {
            ShowYCbCr_CbCrOrder(ColorUtilities.YCbCrType.BT601);
        }

        private void OnShowBT709Click(object sender, RoutedEventArgs e)
        {
            ShowYCbCr_CbCrOrder(ColorUtilities.YCbCrType.BT709);
        }

        private void OnShowBT2020Click(object sender, RoutedEventArgs e)
        {
            ShowYCbCr_CbCrOrder(ColorUtilities.YCbCrType.BT2020);
        }

    }
}
