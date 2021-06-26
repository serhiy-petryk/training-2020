using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Controls;
using WpfSpLib.Helpers;
using WpfSpLibDemo.TestViews;

namespace WpfSpLibDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const int NumberOfTestSteps = 5;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            cbCulture.ItemsSource = CultureInfos;
            cbCulture.SelectedValue = Thread.CurrentThread.CurrentUICulture;

            ControlHelper.HideInnerBorderOfDatePickerTextBox(this, true);
            InitMemoryLeakTest();
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
        private void MwiStartupDemo_OnClick(object sender, RoutedEventArgs e) => new MwiStartupDemo().Show();
        private void TabDemo_OnClick(object sender, RoutedEventArgs e) => new TabDemo().Show();
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

        private void ControlDemo_OnClick(object sender, RoutedEventArgs e) => new ControlDemo().Show();
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

        private async void MemoryUsageOnClick(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            await Task.Delay(2000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            //
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

            var a1 = GC.GetTotalMemory(true);
            if (Debugger.IsAttached)
                Debug.Print($"Memory usage: {a1.ToString("N0")}");
            else
                MessageBox.Show($"Memory usage: {a1.ToString("N0")}");
        }

        private Hashtable weakRefDataCopy;
        private void OnSaveWeakRefsClick(object sender, RoutedEventArgs e)
        {
            var t = Tips.TryGetType("MS.Internal.WeakEventTable");
            // var pi = t.GetProperty("CurrentWeakEventTable", BindingFlags.NonPublic | BindingFlags.Static);
            var fi = t.GetField("_currentTable", BindingFlags.NonPublic | BindingFlags.Static);
            var fiData = t.GetField("_dataTable", BindingFlags.NonPublic | BindingFlags.Instance);
            var fiEventName = t.GetField("_eventNameTable", BindingFlags.NonPublic | BindingFlags.Instance);
            var fiManager = t.GetField("_managerTable", BindingFlags.NonPublic | BindingFlags.Instance);
            var table = fi.GetValue(null);
            var data = fiData.GetValue(table) as Hashtable;
            var eventName = fiEventName.GetValue(table) as Hashtable;
            var mamager = fiManager.GetValue(table) as Hashtable;
            // if (weakRefDataCopy == null)
                weakRefDataCopy = data.Clone() as Hashtable;
            Debug.Print($"Weak Data: {data.Count}");
        }

        private void OnCompareWeakRefsClick(object sender, RoutedEventArgs e)
        {
            var t = Tips.TryGetType("MS.Internal.WeakEventTable");
            // var pi = t.GetProperty("CurrentWeakEventTable", BindingFlags.NonPublic | BindingFlags.Static);
            var fi = t.GetField("_currentTable", BindingFlags.NonPublic | BindingFlags.Static);
            var fiData = t.GetField("_dataTable", BindingFlags.NonPublic | BindingFlags.Instance);
            var fiEventName = t.GetField("_eventNameTable", BindingFlags.NonPublic | BindingFlags.Instance);
            var fiManager = t.GetField("_managerTable", BindingFlags.NonPublic | BindingFlags.Instance);
            var table = fi.GetValue(null);
            var data = fiData.GetValue(table) as Hashtable;
            var eventName = fiEventName.GetValue(table) as Hashtable;
            var mamager = fiManager.GetValue(table) as Hashtable;
            var temp = data.Clone() as Hashtable;

            var diffKeys = temp.Keys.OfType<object>().Except(weakRefDataCopy.Keys.OfType<object>()).ToList();
            var diffData = new Hashtable();
            foreach (var key in temp.Keys)
            {
                if (diffKeys.Contains(key))
                    diffData.Add(key, temp[key]);
            }
            var aa1 = diffKeys.Select(a => a.GetType());
            var aa2 = diffKeys.Select(a=> GetStringOfEventKey(a)).ToList();
            var aa3 = aa2.GroupBy(a => a).Select(a => new { Key = a.Key, Count = a.Count() });
            if (Debugger.IsAttached)
                Debug.Print($"New WeakRefs: {diffKeys.Count}");
            else
                MessageBox.Show($"New WeakRefs: {diffKeys.Count}");
        }

        private string GetStringOfEventKey(object eventKey)
        {
            var o = GetValuesOfEventKey(eventKey);
            return $"{o.Item1.GetType().Name}, {(o.Item2 == null ? "Null" : o.Item2.GetType().Name)}";
        }
        private Tuple<object, object> GetValuesOfEventKey(object eventKey)
        {
            var t = eventKey.GetType();
            var piManager = t.GetProperty("Manager", BindingFlags.Instance | BindingFlags.NonPublic);
            var piSource = t.GetProperty("Source", BindingFlags.Instance | BindingFlags.NonPublic);
            var manager = piManager.GetValue(eventKey);
            var source = piSource.GetValue(eventKey);
            return new Tuple<object, object>(manager, source);
        }

        private void OnCleanupWeakRefTableClick(object sender, RoutedEventArgs e)
        {
            CleanupWeakEventTable();
        }
        private void CleanupWeakEventTable()
        {
            var t = Tips.TryGetType("MS.Internal.WeakEventTable");
            // var t = typeof(BindingOperations);
            var mi = t.GetMethod("Cleanup", BindingFlags.NonPublic | BindingFlags.Static);
            mi.Invoke(null, null);
        }

        private void OnGetInfoClick(object sender, RoutedEventArgs e)
        {
            var a12 = GC.GetTotalMemory(true);
            Debug.Print($"Memory usage: {a12:N0}");

            var t = Tips.TryGetType("MS.Internal.WeakEventTable");
            var fi = t.GetField("_currentTable", BindingFlags.NonPublic | BindingFlags.Static);
            var fiData = t.GetField("_dataTable", BindingFlags.NonPublic | BindingFlags.Instance);
            var table = fi.GetValue(null);
            var data = fiData.GetValue(table) as Hashtable;
            Debug.Print($"Weak refs: {data.Count}");
        }

        #region ============  Memory leak tests  =========
        //============================
        //============================
        private async Task RunTests(Func<Task> test, string testName)
        {
            for (var k = 0; k < NumberOfTestSteps; k++)
            {
                await test();

                await Task.Delay(1000);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                CleanupWeakEventTable();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                CleanupWeakEventTable();

                var a12 = GC.GetTotalMemory(true);

                Debug.Print($"Test{k}: {a12:N0}, {testName}");

                await Task.Delay(1000);
            }
        }

        private async Task RunSimpleTest(Type wndType)
        {
            await RunTests(async () =>
            {
                var wnd = Activator.CreateInstance(wndType) as Window;
                wnd.Show();
                await Task.Delay(300);
                await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;
                wnd.Close();
            }, wndType.Name);
        }

        private Dictionary<string, Func<Task>> _memoryLeakTests;

        private void InitMemoryLeakTest()
        {
            _memoryLeakTests = new Dictionary<string, Func<Task>>
            {
                {"MwiStartup",  async () => await RunSimpleTest(typeof(MwiStartupDemo))},
                {"MwiStartupThemeSelector",  MwiStartupThemeSelectorMemoryTest},
                {"MwiBootstrapColor",  async () => await RunSimpleTest(typeof(MwiBootstrapColorTests))},
                {"PopupResizeControl",  PopupResizeControlMemoryTest},
                {"MwiChild",  BootstrapColorMemoryTest},
                {"ResizingControl",  ResizingControlMemoryTest},
                {"DatePickerEffect",  async () => await RunSimpleTest(typeof(DatePickerEffectTests))},
                {"WatermarkEffect",  async () => await RunSimpleTest(typeof(WatermarkTests))},
                {"ButtonStyles",  async () => await RunSimpleTest(typeof(ButtonStyleTests))},
                {"RippleEffect",  async () => await RunSimpleTest(typeof(RippleEffectTests))},
                {"Calculator",  async () => await RunSimpleTest(typeof(CalculatorTests))},
                {"NumericBox",  async () => await RunSimpleTest(typeof(NumericBoxTests))},
                {"TimePicker",  async () => await RunSimpleTest(typeof(TimePickerTests))},
                {"ColorControl",  async () => await RunSimpleTest(typeof(ColorControlTests))},
                {"KnownColorsOfColorControl",  KnownColorsOfColorControlMemoryTest},
            };

            foreach (var kvp in _memoryLeakTests)
            {
                var btn = new Button {Margin = new Thickness(5), Content = kvp.Key};
                btn.Click += OnMemoryLeakTestClick;
                MemoryLeakTests.Children.Add(btn);
            }
        }

        //==============
        private async Task MwiStartupThemeSelectorMemoryTest()
        {
            await RunTests(async () =>
            {
                var wnd = new MwiStartupDemo();
                wnd.Show();

                await Task.Delay(300);
                await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                timer.Start();
                timer.Tick += (sender2, args) =>
                {
                    timer.Stop();
                    var a3 = wnd.TopControl.GetVisualChildren().OfType<MwiContainer>().FirstOrDefault();
                    var a4 = AdornerLayer.GetAdornerLayer(a3);
                    var selectorHost = a4.GetVisualChildren().OfType<MwiChild>().FirstOrDefault();
                    // var selectorHost = Keyboard.FocusedElement as MwiChild;
                    selectorHost?.CmdClose.Execute(null);
                };
                wnd.TopControl.CmdSelectTheme.Execute(null);

                await Task.Delay(300);
                await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

                wnd.Close();
            }, "MwiStartupThemeSelector");
        }

        private async Task PopupResizeControlMemoryTest()
        {
            await RunTests(new Func<Task>(async () =>
            {
                var wnd = new TextBoxTests();
                wnd.Show();

                await Task.Delay(300);
                await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

                var a1 = wnd.TestTextBox.GetVisualChildren().OfType<ToggleButton>().FirstOrDefault(a => a.Name.EndsWith("Keyboard"));
                if (a1 != null)
                    a1.IsChecked = true;

                await Task.Delay(300);
                await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

                if (a1 != null)
                    a1.IsChecked = false;

                await Task.Delay(300);
                await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

                wnd.Close();
            }), "PopupResizeControl");
        }

        private async Task BootstrapColorMemoryTest()
        {
            var wnd = new MwiBootstrapColorTests();
            wnd.Show();
            await Task.Delay(300);
            await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;
            await wnd.RunTest(NumberOfTestSteps);
            await Task.Delay(300);
            await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;
            wnd.Close();
        }

        private async Task ResizingControlMemoryTest()
        {
            var wnd = new ResizingControlTests();
            wnd.Show();
            await Task.Delay(300);
            await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;
            await wnd.AutomateAsync(NumberOfTestSteps);
            await Task.Delay(300);
            await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;
            wnd.Close();
        }

        private async Task KnownColorsOfColorControlMemoryTest()
        {
            await RunTests(async () =>
            {
                var wnd = new ColorControlTests();
                wnd.Show();

                await Task.Delay(300);
                await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

                var a1 = wnd.ColorControl.GetVisualChildren().OfType<TabControl>().FirstOrDefault();
                a1.SelectedIndex = 2;
                await Task.Delay(300);
                await wnd.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

                wnd.Close();
            }, "KnownColorsOfColorControl");
        }

        //==================
        private void OnMemoryLeakTestClick(object sender, RoutedEventArgs e)
        {
            var key = (string)((ContentControl)sender).Content;
            var fn = _memoryLeakTests[key];
            fn();
        }

        private async void OnRunAllTestsClick(object sender, RoutedEventArgs e)
        {
            // foreach (var kvp in _memoryLeakTests.Reverse())
            foreach (var kvp in _memoryLeakTests)
                await kvp.Value();
        }
        #endregion

        private void OnRunAllTestsAsyncClick(object sender, RoutedEventArgs e)
        {
            foreach (var kvp in _memoryLeakTests)
                kvp.Value();
        }
    }
}
