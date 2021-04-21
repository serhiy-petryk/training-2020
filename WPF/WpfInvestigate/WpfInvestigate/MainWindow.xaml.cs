using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfInvestigate.Common;
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

        private void MwiStartup_OnClick(object sender, RoutedEventArgs e) => new MwiStartup().Show();
        private void MwiBootstrapColorTests_OnClick(object sender, RoutedEventArgs e) => new MwiBootstrapColorTests().Show();
        private void MwiTests_OnClick(object sender, RoutedEventArgs e) => new MwiTests().Show();
        private void ResizingControlTests_OnClick(object sender, RoutedEventArgs e) => new ResizingControlTests().Show();
        private void TimePickerTest_OnClick(object sender, RoutedEventArgs e) => new TimePickerTests().Show();
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
        private void ObsoleteDialogItemsTests_OnClick(object sender, RoutedEventArgs e) => new DialogItemsTests().Show();
        private void OldButtonStyleTest_OnClick(object sender, RoutedEventArgs e) => new XButtonStyleTests().Show();

        private void ControlDemo_OnClick(object sender, RoutedEventArgs e) => new ControlDemo().Show();
        private void TempControl_OnClick(object sender, RoutedEventArgs e) => new TempControl().Show();
        private void MwiTemplate_OnClick(object sender, RoutedEventArgs e) => new MwiTemplate().Show();

        private void MemoryLeakInvestigation_OnClick(object sender, RoutedEventArgs e) => new MemoryLeakInvestigation().Show();

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
        }

        private void TestButtonChangeStyle(object sender, RoutedEventArgs e)
        {
            var a1 = TestButton2.Foreground;
            var styleRed = FindResource("StyleRed") as Style;
            var styleYellow = FindResource("StyleYellow") as Style;
            TestButton2.Style = Equals(TestButton2.Style, styleRed) ? styleYellow : styleRed;
        }

        private void TestButtonChangeForeground(object sender, RoutedEventArgs e)
        {
            var a1 = this.TryFindResource("TestBrush") as SolidColorBrush;
            if (a1!= null)
                Resources["TestBrush"]= new SolidColorBrush(Colors.Green);
        }

        private void MemoryUsageOnClick(object sender, RoutedEventArgs e)
        {
            var o = new object();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            o = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            //
            var a1 = GC.GetTotalMemory(true);
            Debug.Print($"Memory usage: {a1.ToString("N0")}");
            // Tips.ClearAllBindings(WpfInvestigate.TestViews.MwiBootstrapColorTests.Instance);
        }

        private async void OnMwiContainerMemoryTestClick(object sender, RoutedEventArgs e)
        {
            for (var k = 0; k < 5; k++)
                await MwiContainerMemoryTestStep(k);
        }
        private async Task MwiContainerMemoryTestStep(int step)
        {
            var wnd = new MwiBootstrapColorTests();
            wnd.Show();

            await Task.Delay(1000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var a11 = GC.GetTotalMemory(true);

            wnd.Close();

            await Task.Delay(1000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var a12 = GC.GetTotalMemory(true);

            Debug.Print($"Test{step}: {a11:N0}, {a12:N0}");

            await Task.Delay(1000);
        }

        private void TestButton3_OnClick(object sender, RoutedEventArgs e)
        {
            var zz1 = EventManager.GetRoutedEvents();

            Debug.Print($"TestButton3_OnClick");
            // var x1 = DependencyObjectHelper.GetDependencyPropertiesForType(typeof(Type));
            // var x2 = DependencyObjectHelper.GetDependencyPropertiesForType(typeof(Button));
            //TestButton3.Unloaded += TestButton3_Unloaded;
            //TestButton3.Click += TestButton3_Click;
            //TestButton3.Click += (o, args) => Debug.Print($"AAAAAA");
            TestButton3.IsHitTestVisibleChanged += TestButton3_IsHitTestVisibleChanged;
            TestButton3.IsHitTestVisibleChanged += (o, args) => Debug.Print($"XXX");
            var dpd1 = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
            dpd1.AddValueChanged(TestButton3, (o, args) => Debug.Print($"DependencyPropertyDescriptor: {dpd1.Name}"));
            dpd1.AddValueChanged(TestButton3, (o, args) => Debug.Print($"DependencyPropertyDescriptor2: {dpd1.Name}"));

            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.EffectProperty, typeof(UIElement));
            dpd.AddValueChanged(TestButton3, (o, args) => Debug.Print($"DependencyPropertyDescriptor: {dpd.Name}"));
            dpd.AddValueChanged(TestButton3, (o, args) => Debug.Print($"DependencyPropertyDescriptor2: {dpd.Name}"));

            EventHelper.RemoveDependencyPropertyEventHandlers(TestButton3);
            EventHelper.RemovePropertyChangeEventHandlers(TestButton3);

            var xx1 = Events.GetDependencyProperties(TestButton3);
            var xx2 = Events.EnumerateDependencyProperties(TestButton3);
            var xx3 = TestButton3.GetType().GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == typeof(DependencyProperty)).Select(fieldInfo => fieldInfo.GetValue(null) as DependencyProperty);

            //==========  DependencyPropertyDescriptor  ========
            var piProperty = dpd.GetType().GetProperty("Property", BindingFlags.NonPublic | BindingFlags.Instance);
            var property = piProperty.GetValue(dpd);
            var fiTrackers = property.GetType().GetField("_trackers", BindingFlags.Instance | BindingFlags.NonPublic);
            var trackers = fiTrackers.GetValue(property) as IDictionary;
            if (trackers.Contains(TestButton3))
            {
                var a1 = trackers[TestButton3];
                var fiChanged = a1.GetType().GetField("Changed", BindingFlags.Instance | BindingFlags.NonPublic);
                var changed = fiChanged.GetValue(a1) as EventHandler;
                dpd.RemoveValueChanged(TestButton3, changed);
            }

            //==========  DependencyPropertyChangedEventHandler  ========
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            var eventHandlersStore = eventHandlersStoreProperty.GetValue(TestButton3, null);
            if (eventHandlersStore == null) return;

            var fiEntries = eventHandlersStore.GetType().GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
            var entries = fiEntries.GetValue(eventHandlersStore);
            var piCount = entries.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
            var count = (int)piCount.GetValue(entries);
            var miGetKeyValue = entries.GetType().GetMethod("GetKeyValuePair", BindingFlags.Public | BindingFlags.Instance);
            var values = new List<Tuple<int, object>>();
            for (var k = 0; k < count; k++)
            {
                var args = new object[] {k, null, null};
                miGetKeyValue.Invoke(entries, args);
                values.Add(new Tuple<int, object>((int)args[1], args[2]));
            }

            var t1 = Tips.TryGetType("System.Windows.GlobalEventManager");
            var fi = t1.GetField("_globalIndexToEventMap", BindingFlags.Static | BindingFlags.NonPublic);
            var o1 = fi.GetValue(null) as ArrayList;
            // this.EventHandlersStoreRemove(UIElement.IsHitTestVisibleChangedKey, value);
            return;
            var aa1 = values[4].Item2 as DependencyPropertyChangedEventHandler;

            var mi = typeof(UIElement).GetMethod("EventHandlersStoreRemove", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(TestButton3, new[] { o1[values[4].Item1], aa1 });
        }

        private void TestButton3_IsHitTestVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Debug.Print($"TestButton3_IsHitTestVisibleChanged");
        }

        private void TestButton3_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print($"Click");
        }

        private void TestButton3_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.Print($"Unloaded");
        }
    }
}
