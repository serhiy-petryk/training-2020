using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using ColorInvestigation.Common;
using ColorInvestigation.Common.ColorSpaces;
using ColorInvestigation.Lib;
using ColorInvestigation.Views;

namespace ColorInvestigation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            // ColorSpacesTests.HueTest();
            // ColorSpacesTests.HslTest();
            // ColorSpacesTests.HsvTest();
            // ColorSpacesTests.LabTest();
            // ColorSpacesTests.XyzTest();

            // ColorSpacesTests.YCbCrTest(ColorSpaces.YCbCrStandard.BT601);
            ColorSpacesTests.YCbCrTest(YCbCrStandard.BT709);
            ColorSpacesTests.YCbCrTest(YCbCrStandard.BT2020);
        }

        private bool IsEqual(double d1, double d2) => Math.Abs(d1 - d2) < 0.0001;

        private void OnGrayScaleButtonClick(object sender, RoutedEventArgs e) => new GrayScale().Show();
        private void OnGrayScaleDiffButtonClick(object sender, RoutedEventArgs e) => new GrayScaleDiff().Show();
        private void OnCalcButtonClick(object sender, RoutedEventArgs e) => Temp.Calc.Calculate();
        private void OnColorSpacesButtonClick(object sender, RoutedEventArgs e) => new ColorSpacesForm().Show();
        private void OnForegroundButtonClick(object sender, RoutedEventArgs e) => new Foreground().Show();
        private void OnForegroundDiffButtonClick(object sender, RoutedEventArgs e) => new ForegroundDiff().Show();
        private void OnMonoChromaticButtonClick(object sender, RoutedEventArgs e) => new MonoChromatic().Show();
        private void OnColorPickerButtonClick(object sender, RoutedEventArgs e) => new ColorPickerTests().Show();
        private void OnMonoChromeButtonsButtonClick(object sender, RoutedEventArgs e) => new MonoChromeButtons().Show();

        private void OnChangeHueClick(object sender, RoutedEventArgs e)
        {
            var a1 = Application.Current.Resources["HueAndSaturation"] as DynamicBinding;
            var aa1 = ((string)a1.Value).Split(new[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
            var newHue = (int.Parse(aa1[0]) + 20) % 360;
            a1.Value = $"{newHue},{aa1[1]}";
        }

        private void MonochromeTestButton_OnLoaded(object sender, RoutedEventArgs e)
        {
            var button = sender as ButtonBase;
            if (button != null)
            {
                button.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    var dpd1 = DependencyPropertyDescriptor.FromProperty(Control.BackgroundProperty, typeof(Control));
                    dpd1.AddValueChanged(button, OnBackgroundChanged);
                    OnBackgroundChanged(button, null);
                }));
            }
        }
        private void OnBackgroundChanged(object sender, EventArgs e)
        {
            var button = sender as ButtonBase;
            if (button != null)
            {
                // Refresh button colors
                var buttonChild = VisualTreeHelper.GetChild(button, 0) as FrameworkElement;
                var stateGroup = (VisualStateManager.GetVisualStateGroups(buttonChild) as IList<VisualStateGroup>).FirstOrDefault(g => g.Name == "CommonStates");
                stateGroup?.CurrentState?.Storyboard.Begin(buttonChild);
                // Refresh shapes colors
                // SetColorsForShapes(sender, e);
            }
        }

        private void ChangeColor_OnClick(object sender, RoutedEventArgs e)
        {
            if (((SolidColorBrush)AA.Background).Color == Colors.Blue)
                AA.Background = new SolidColorBrush(Colors.Green);
            else
                AA.Background = new SolidColorBrush(Colors.Blue);
        }

        private void PART_Border_OnLoaded(object sender, RoutedEventArgs e)
        {
            var aa1 = Tips.GetVisualParents(sender as FrameworkElement);
            //
            // throw new NotImplementedException();
        }
    }
}
