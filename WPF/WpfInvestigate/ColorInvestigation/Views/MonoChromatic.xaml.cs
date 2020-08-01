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
    public partial class MonoChromatic : Window
    {
        public MonoChromatic()
        {
            InitializeComponent();
        }

        private void OnConverterTestClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("ConverterTest") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + l, Background = new SolidColorBrush(back), Style = style };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnConverterTestWithSplitClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("ConverterTestWithSplit") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                var grayLevel = Convert.ToByte(ColorUtilities.OldColorToGrayLevel(back));
                var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + l + " " +grayLevel, Background = new SolidColorBrush(back), Style = style };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnConverterTestWithSplitRelativeStyleClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("ConverterTestWithSplitRelativeStyle") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                var grayLevel = Convert.ToByte(ColorUtilities.OldColorToGrayLevel(back));
                var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + l + " " + grayLevel, Background = new SolidColorBrush(back), Style = style };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnHslMonoStyleClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("OldHslMonoStyle") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + l, Background = new SolidColorBrush(back), Style = style};
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
                var color = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                var grayLevel = Convert.ToByte(ColorUtilities.OldColorToGrayLevel(color));
                var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + l + " " + grayLevel, Background = new SolidColorBrush(color), Style = style, BorderThickness = new Thickness(3)};
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
                var color = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                var grayLevel = Convert.ToByte(ColorUtilities.OldColorToGrayLevel(color));
                var btn = new Button { Width = 80, Height = 25, Content = h + " " + s + " " + l + " " + grayLevel, Background = new SolidColorBrush(color), Style = style, BorderThickness=new Thickness(3) };
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
                var color = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                var grayLevel = Convert.ToByte(ColorUtilities.OldColorToGrayLevel(color));
                var lab = ColorUtilities.ColorToLab(color);
                var l2 = Convert.ToInt32(lab.Item1);
                var a = Convert.ToInt32(lab.Item2);
                var b = Convert.ToInt32(lab.Item3);
                // var grayLevel = Convert.ToByte(ColorUtilities.ColorToGrayLevel(color));
                        var btn = new Button { Width = 80, Height = 25, Content = l2 + " " + a + " " + b + " " + grayLevel, Background = new SolidColorBrush(color), Style = style, BorderThickness = new Thickness(3) };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }

        private void OnNewLabStyleClick(object sender, RoutedEventArgs e)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource("NewLabColorStyle") as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var color = ColorUtilities.HslToColor(h / 100.0, s / 100.0, l / 100.0);
                var grayLevel = Convert.ToByte(ColorUtilities.OldColorToGrayLevel(color));
                var lab = ColorUtilities.ColorToLab(color);
                var l2 = Convert.ToInt32(lab.Item1);
                var a = Convert.ToInt32(lab.Item2);
                var b = Convert.ToInt32(lab.Item3);
                // var grayLevel = Convert.ToByte(ColorUtilities.ColorToGrayLevel(color));
                var btn = new Button { Width = 80, Height = 25, Content = l2 + " " + a + " " + b + " " + grayLevel, Background = new SolidColorBrush(color), Style = style, BorderThickness = new Thickness(3) };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
        }
    }
}
