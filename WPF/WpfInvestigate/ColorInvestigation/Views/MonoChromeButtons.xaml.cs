using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorInvestigation.Common.ColorSpaces;

namespace ColorInvestigation.Views
{
    /// <summary>
    /// Interaction logic for MonoChromeButtons.xaml
    /// </summary>
    public partial class MonoChromeButtons : Window
    {
        public MonoChromeButtons()
        {
            InitializeComponent();
        }

        private void SetStyle(string styleName)
        {
            Panel.Children.Clear();
            var step = 10;
            var style = Application.Current.TryFindResource(styleName) as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = new HSL(h / 100.0, s / 100.0, l / 100.0).RGB.Color;
                var grayLevel = Convert.ToByte(ColorUtils.GetGrayLevel(new RGB(back)) * 100);
                        var btn = new Button
                        {
                            Width = 80,
                            Height = 25,
                            Margin = new Thickness(3),
                    Content = h * 3.6 + " " + s + " " + l + " " + grayLevel,
                    Background = new SolidColorBrush(back),
                    BorderThickness = new Thickness(2),
                    Style = style
                };
                Panel.Children.Add(btn);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 1000;
            CurrentStyle.Text = styleName;
        }

        private void CompareStyles(string styleName1, string styleName2)
        {
            Panel.Children.Clear();
            var step = 10;
            var style1 = Application.Current.TryFindResource(styleName1) as Style;
            var style2 = Application.Current.TryFindResource(styleName2) as Style;
            for (var h = 0; h <= 100; h += step)
            for (var s = 0; s <= 100; s += step)
            for (var l = 0; l <= 100; l += step)
            {
                var back = new HSL(h / 100.0, s / 100.0, l / 100.0).RGB.Color;
                var grayLevel = Convert.ToByte(ColorUtils.GetGrayLevel(new RGB(back)) * 100);
                var btn1 = new Button
                {
                    Width = 80,
                    Height = 25,
                    Content = h * 3.6 + " " + s + " " + l + " " + grayLevel,
                    Background = new SolidColorBrush(back),
                    BorderThickness = new Thickness(2),
                    Style = style1
                };
                var btn2 = new Button
                {
                    Width = 80,
                    Height = 25,
                    Content = h * 3.6 + " " + s + " " + l + " " + grayLevel,
                    Background = new SolidColorBrush(back),
                    BorderThickness = new Thickness(2),
                    Style = style2
                };
                Panel.Children.Add(btn1);
                Panel.Children.Add(btn2);
            }
            var wnd = GetWindow(this);
            if (wnd != null) wnd.Width = 950;
            CurrentStyle.Text = styleName1;
        }

        private void OnMonoChromeAnimatedStyleClick(object sender, RoutedEventArgs e)
        {
            SetStyle("MonochromeAnimatedButtonStyle");
        }

        private void OnHslMonoStyleClick(object sender, RoutedEventArgs e)
        {
            SetStyle("HslMonoStyle");
        }

        private void OnColorPickerHslMonoStyleClick(object sender, RoutedEventArgs e)
        {
            SetStyle("ColorPickerHslMonoStyle");
        }

        private void OnTestStyleClick(object sender, RoutedEventArgs e)
        {
            SetStyle("TestColorStyle");
        }

        private void OnTest2StyleClick(object sender, RoutedEventArgs e)
        {
            SetStyle("TestColorStyle2");
        }

        private void OnCompareMonochromeClick(object sender, RoutedEventArgs e)
        {
        }

        private void OnMonoChromeStyleClick(object sender, RoutedEventArgs e)
        {
            SetStyle("MonochromeButtonStyle");
        }

        private void OnMouseOverTestClick(object sender, RoutedEventArgs e)
        {
            SetStyle("OnMouseOverTestStyle");
        }

        private void OnPressedTestClick(object sender, RoutedEventArgs e)
        {
            SetStyle("OnPressedTestStyle");
        }

        private void OnTest3StyleClick(object sender, RoutedEventArgs e)
        {
            SetStyle("TestColorStyle3");
        }
    }
}
