using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
        private void DatePickerExtensionTest_OnClick(object sender, RoutedEventArgs e) => new DatePickerExtensionTests().Show();
        private void WiPTest_OnClick(object sender, RoutedEventArgs e) => new WiPTests().Show();
        private void RippleEffectTest_OnClick(object sender, RoutedEventArgs e) => new RippleEffectTests().Show();
        private void CalculatorTest_OnClick(object sender, RoutedEventArgs e) => new CalculatorTests().Show();
        private void DropDownButtonTest_OnClick(object sender, RoutedEventArgs e) => new DropDownButtonTests().Show();
        private void NumericBoxTest_OnClick(object sender, RoutedEventArgs e) => new NumericBoxTests().Show();
        private void KeyboardTest_OnClick(object sender, RoutedEventArgs e) => new VirtualKeyboardTests().Show();
        private void ColorPickerTest_OnClick(object sender, RoutedEventArgs e) => new ColorPickerTests().Show();
        private void ControlEffectTests_OnClick(object sender, RoutedEventArgs e) => new ControlEffectTests().Show();
        private void BootstrapButtonTests_OnClick(object sender, RoutedEventArgs e) => new BootstrapButtonTests().Show();
        private void ChromeTest_OnClick(object sender, RoutedEventArgs e) => new ChromeTests().Show();
        private void ButtonStyleTests_OnClick(object sender, RoutedEventArgs e) => new ButtonStyleTests().Show();

        private void ObsoleteNumericUpDownTest_OnClick(object sender, RoutedEventArgs e) => new NumericUpDownTests().Show();
        private void ObsoleteRippleButtonTest_OnClick(object sender, RoutedEventArgs e) => new RippleButtonTests().Show();
        private void ObsoleteControlLibrary_OnClick(object sender, RoutedEventArgs e) => new ObsoleteControlLibrary().Show();
        private void ObsoleteMonochromeButtonTest_OnClick(object sender, RoutedEventArgs e) => new MonochromeButtonTests().Show();
        private void ObsoleteDualPathToggleButtonEffectTest_OnClick(object sender, RoutedEventArgs e) => new DualPathToggleButtonEffectTests().Show();
        private void ObsoleteFlatButtonTest_OnClick(object sender, RoutedEventArgs e) => new FlatButtonTests().Show();

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
        }

        private void ControlDemo_OnClick(object sender, RoutedEventArgs e) => new ControlDemo().Show();

        private void TempControl_OnClick(object sender, RoutedEventArgs e) => new TempControl().Show();

        private void DialogItemsTests_OnClick(object sender, RoutedEventArgs e) => new DialogTests().Show();
    }
}
