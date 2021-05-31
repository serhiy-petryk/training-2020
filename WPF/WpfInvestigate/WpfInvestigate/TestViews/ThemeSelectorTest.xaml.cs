﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;
using WpfInvestigate.Samples;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ThemeSelectorTest.xaml
    /// </summary>
    public partial class ThemeSelectorTest : Window
    {
        public ThemeSelectorTest()
        {
            InitializeComponent();
        }

        private void OnOpenColorControlClick(object sender, RoutedEventArgs e)
        {
            var a1 = new DialogAdorner(Host) { CloseOnClickBackground = false };

            var themeSelector = new ThemeSelector {Margin = new Thickness(0)};
            themeSelector.ColorControl.Color = Colors.Orange;
            var mwiChild = new MwiChild
            {
                Content = themeSelector,
                Width = 700,
                Height = 600,
                LimitPositionToPanelBounds = true,
                Title = "Theme Selector",
                VisibleButtons = MwiChild.Buttons.Close
            };
            mwiChild.SetBinding(BackgroundProperty, new Binding("Color") { Source = themeSelector, Converter = ColorHslBrush.Instance });
            mwiChild.SetBinding(MwiChild.ThemeProperty, new Binding("Theme") { Source = themeSelector});
            a1.ShowContentDialog(mwiChild);
        }
    }
}
