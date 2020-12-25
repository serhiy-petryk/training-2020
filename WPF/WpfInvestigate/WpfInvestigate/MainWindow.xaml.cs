using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Effects;
using WpfInvestigate.Helpers;
using WpfInvestigate.Obsolete;
using WpfInvestigate.Obsolete.TestViews;
using WpfInvestigate.Temp;
using WpfInvestigate.TestViews;

namespace WpfInvestigate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            cbCulture.ItemsSource = CultureInfos;
            cbCulture.SelectedValue = Thread.CurrentThread.CurrentUICulture;

            cbDataType.ItemsSource = Enum.GetValues(typeof(DataTypeMetadata.DataType)).Cast<DataTypeMetadata.DataType>();
            cbDataType.SelectedValue = DataTypeMetadata.DataType.Date;

            ControlHelper.HideInnerBorderOfDatePickerTextBox(this, true);
        }

        private static string[] _cultures = { "", "sq-AL", "uk-UA", "en-US", "km-KH", "yo-NG" };

        public List<CultureInfo> CultureAllInfos { get; set; } = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).OrderBy(c => c.DisplayName).ToList();
        public List<CultureInfo> CultureInfos { get; set; } = CultureInfo
            .GetCultures(CultureTypes.InstalledWin32Cultures).Where(c => Array.IndexOf(_cultures, c.Name) != -1)
            .OrderBy(c => c.DisplayName).ToList();

        private void CbCulture_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var newCulture = e.AddedItems[0] as CultureInfo;
                Thread.CurrentThread.CurrentCulture = newCulture; // MainCulture for DatePicker
                Thread.CurrentThread.CurrentUICulture = newCulture;
                CultureInfo.DefaultThreadCurrentCulture = newCulture;
                CultureInfo.DefaultThreadCurrentUICulture = newCulture;
            }
        }

        private void TimepickerTest_OnClick(object sender, RoutedEventArgs e) => new TimePickerTests().Show();
        private void ObjectEditorTest_OnClick(object sender, RoutedEventArgs e) => new ObjectEditorTests().Show();
        private void WatermarkTest_OnClick(object sender, RoutedEventArgs e) => new WatermarkTests().Show();
        private void DatePickerEffectTest_OnClick(object sender, RoutedEventArgs e) => new DatePickerEffectTests().Show();
        private void WiPTest_OnClick(object sender, RoutedEventArgs e) => new WiPTests().Show();
        private void RippleEffectTest_OnClick(object sender, RoutedEventArgs e) => new RippleEffectTests().Show();
        private void CalculatorTest_OnClick(object sender, RoutedEventArgs e) => new CalculatorTests().Show();
        private void DropDownButtonTest_OnClick(object sender, RoutedEventArgs e) => new DropDownButtonTests().Show();
        private void NumericBoxTest_OnClick(object sender, RoutedEventArgs e) => new NumericBoxTests().Show();
        private void KeyboardTest_OnClick(object sender, RoutedEventArgs e) => new VirtualKeyboardTests().Show();
        private void ColorControlTest_OnClick(object sender, RoutedEventArgs e) => new ColorControlTests().Show();
        private void ControlEffectTests_OnClick(object sender, RoutedEventArgs e) => new ControlEffectTests().Show();
        private void BootstrapButtonTests_OnClick(object sender, RoutedEventArgs e) => new BootstrapButtonTests().Show();
        private void ChromeTest_OnClick(object sender, RoutedEventArgs e) => new ChromeTests().Show();
        private void ButtonStyleTests_OnClick(object sender, RoutedEventArgs e) => new ButtonStyleTests().Show();
        private void FocusEffectTests_OnClick(object sender, RoutedEventArgs e) => new FocusEffectTests().Show();
        private void TextBoxTests_OnClick(object sender, RoutedEventArgs e) => new TextBoxTests().Show();

        private void ObsoleteNumericUpDownTest_OnClick(object sender, RoutedEventArgs e) => new NumericUpDownTests().Show();
        private void ObsoleteRippleButtonTest_OnClick(object sender, RoutedEventArgs e) => new RippleButtonTests().Show();
        private void ObsoleteControlLibrary_OnClick(object sender, RoutedEventArgs e) => new ObsoleteControlLibrary().Show();
        private void ObsoleteMonochromeButtonTest_OnClick(object sender, RoutedEventArgs e) => new MonochromeButtonTests().Show();
        private void ObsoleteDualPathToggleButtonEffectTest_OnClick(object sender, RoutedEventArgs e) => new DualPathToggleButtonEffectTests().Show();
        private void ObsoleteFlatButtonTest_OnClick(object sender, RoutedEventArgs e) => new FlatButtonTests().Show();
        private void ObsoleteShadowEffectTest_OnClick(object sender, RoutedEventArgs e) => new ShadowEffectTests().Show();
        private void OldButtonStyleTest_OnClick(object sender, RoutedEventArgs e) => new XButtonStyleTests().Show();

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            var grid = Tips.GetVisualChildren(TestTextBox).OfType<Grid>().FirstOrDefault();
            if (grid != null)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                // var style = FindResource("ClearBichromeAnimatedButtonStyle") as Style;
                var keyboardButton = new Button
                {
                    Name = "KeyboardButton",
                    // Style = style,
                    Width = 16,
                    Margin = new Thickness(1,0,0,0),
                    Padding = new Thickness(0)
                };

                ChromeEffect.SetBichromeAnimatedBackground(keyboardButton, Colors.Yellow);
                ChromeEffect.SetBichromeAnimatedForeground(keyboardButton, Colors.Blue);
                IconEffect.SetGeometry(keyboardButton, FindResource("KeyboardGeometry") as Geometry);

                //if (dp.Background == null || dp.Background == Brushes.Transparent)
                //  dp.Background = Tips.GetActualBackgroundBrush(dp);

                // clearButton.Click += ClearButton_Click;
                grid.Children.Add(keyboardButton);
                Grid.SetColumn(keyboardButton, grid.ColumnDefinitions.Count - 1);

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                var style = FindResource("ClearBichromeAnimatedButtonStyle") as Style;
                var clearButton = new Button
                {
                    Name = "ClearButtonName",
                    Style = style,
                    Width = 14,
                    // Margin = new Thickness(-2, 0, 1 - dp.Padding.Right, 0),
                    Margin = new Thickness(1, 0, 0, 0),
                    Padding = new Thickness(1)
                };

                //if (dp.Background == null || dp.Background == Brushes.Transparent)
                //  dp.Background = Tips.GetActualBackgroundBrush(dp);

                // clearButton.Click += ClearButton_Click;
                grid.Children.Add(clearButton);
                Grid.SetColumn(clearButton, grid.ColumnDefinitions.Count - 1);

            }
        }

        private void ControlDemo_OnClick(object sender, RoutedEventArgs e) => new ControlDemo().Show();

        private void TempControl_OnClick(object sender, RoutedEventArgs e) => new TempControl().Show();

        private void DialogItemsTests_OnClick(object sender, RoutedEventArgs e) => new DialogTests().Show();

        private void ChangeBackground_OnClick(object sender, RoutedEventArgs e)
        {
            if (TestTextBox.Background == Brushes.Yellow)
                TestTextBox.Background = Brushes.YellowGreen;
            else
                TestTextBox.Background = Brushes.Yellow;
        }

        private void ChangeForeground_OnClick(object sender, RoutedEventArgs e)
        {
            if (TestTextBox.Foreground == Brushes.Blue)
                TestTextBox.Foreground = Brushes.Violet;
            else
                TestTextBox.Foreground = Brushes.Blue;
        }

        private void ChangeButton_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = (int)TextBoxEffects.GetVisibleButtons(TestTextBox);
            var a2 = (a1+1) % 16;
            TextBoxEffects.SetVisibleButtons(TestTextBox, (TextBoxEffects.Buttons)a2);

            var grid = Tips.GetVisualChildren(TestTextBox).OfType<Grid>().FirstOrDefault();
            if (grid !=null)
                Debug.Print($"ColumnsAfter: {grid.ColumnDefinitions.Count}");

        }

    }
}
