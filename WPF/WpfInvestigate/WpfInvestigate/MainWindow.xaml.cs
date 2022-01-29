using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WpfInvestigate.Temp.TreeViewClasses;
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

            InitNodes();
            // TreeView.ItemsSource = nodes;

            InitGroupingNodes();
            TreeView.ItemsSource = GroupNodes;

            ControlHelper.HideInnerBorderOfDatePickerTextBox(this, true);
        }

        // private static string[] _cultures = { "", "sq-AL", "uk-UA", "en-US", "km-KH", "yo-NG" };
        private static string[] _cultures = { "", "sq", "uk", "en", "km", "yo" };

        public List<CultureInfo> CultureAllInfos { get; set; } = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).OrderBy(c => c.DisplayName).ToList();
        public List<CultureInfo> CultureInfos { get; set; } = CultureInfo
            .GetCultures(CultureTypes.InstalledWin32Cultures).Where(c => Array.IndexOf(_cultures, c.Name) != -1)
            .OrderBy(c => c.DisplayName).ToList();

        private void CbCulture_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var newCulture = e.AddedItems[0] as CultureInfo;
                App.Language = newCulture;
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
        private void BootstrapButtonTests_OnClick(object sender, RoutedEventArgs e) => new BootstrapButtonTests().Show();
        private void ChromeTest_OnClick(object sender, RoutedEventArgs e) => new ChromeTests().Show();
        private void ButtonStyleTests_OnClick(object sender, RoutedEventArgs e) => new ButtonStyleTests().Show();
        private void FocusEffectTests_OnClick(object sender, RoutedEventArgs e) => new FocusEffectTests().Show();
        private void DragDropTests_OnClick(object sender, RoutedEventArgs e) => new DragDropTests().Show();
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
            if (Debugger.IsAttached)
                Debug.Print($"Memory usage: {a1.ToString("N0")}");
            else
                MessageBox.Show($"Memory usage: {a1.ToString("N0")}");
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

            /*GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var a11 = GC.GetTotalMemory(true);*/

            wnd.Close();

            await Task.Delay(1000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var a12 = GC.GetTotalMemory(true);

            Debug.Print($"Test{step}: {a12:N0}");

            await Task.Delay(1000);
        }

        private async void OnMwiStartupMemoryTestClick(object sender, RoutedEventArgs e)
        {
            for (var k = 0; k < 5; k++)
                await MwiStartupMemoryTestStep(k);
        }
        private async Task MwiStartupMemoryTestStep(int step)
        {
            var wnd = new MwiStartup();
            wnd.Show();

            await Task.Delay(1000);

            /*GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var a11 = GC.GetTotalMemory(true);*/

            wnd.Close();

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

            Debug.Print($"Test{step}: {a12:N0}");
            // Debug.Print($"Test{step}: {a12:N0}, {EventHelper._cnt1}, {EventHelper._cnt2}, {EventHelper._cnt3}, {EventHelper._cnt4}");

            await Task.Delay(1000);
        }


        private Hashtable weakRefDataCopy;
        private Hashtable weakRefDataCopy2;
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
            // var aa1 = UnloadingHelper.AAA;

            Debug.Print($"ClearLog");
            UnloadingHelper.EventLog.Clear();
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
            // foreach(var a1 in diffKeys)
            //  data.Remove(a1);
            var log = UnloadingHelper.EventLog;
            if (Debugger.IsAttached)
                Debug.Print($"New WeakRefs: {diffKeys.Count}, {log.Count}");
            else
                MessageBox.Show($"New WeakRefs: {diffKeys.Count}, {log.Count}");
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

        public ObservableCollection<Node> nodes { get; set; }
        public ObservableCollection<SortingItemModel> GroupNodes { get; set; }

        private void InitGroupingNodes()
        {
            GroupNodes = new ObservableCollection<SortingItemModel>();
            var rootItem = new PropertyItem( "SortRoot");
            var rootGroup = new GroupingItemModel(rootItem, ListSortDirection.Ascending, null);
            GroupNodes.Add(rootGroup);
            var groupItem1 = new PropertyItem("Group item 1");
            var sortGroupItem1 = new PropertyItem("Sorting item 1");
            var group1 = new GroupingItemModel(groupItem1, ListSortDirection.Ascending, new[] {new SortingItemModel(sortGroupItem1, ListSortDirection.Descending)});
            rootGroup.AddGroup(group1);
        }

        private void InitNodes()
        {
            nodes = new ObservableCollection<Node>
            {
                new Node
                {
                    Name ="Европа",
                    Children = new ObservableCollection<Node>
                    {
                        new Node {Name="Германия" },
                        new Node {Name="Франция" },
                        new Node
                        {
                            Name ="Великобритания",
                            Children = new ObservableCollection<Node>
                            {
                                new Node {Name="Англия" },
                                new Node {Name="Шотландия" },
                                new Node {Name="Уэльс" },
                                new Node {Name="Сев. Ирландия" },
                            }
                        }
                    }
                },
                new Node
                {
                    Name ="Азия",
                    Children = new ObservableCollection<Node>
                    {
                        new Node {Name="Китай" },
                        new Node {Name="Япония" },
                        new Node { Name ="Индия" }
                    }
                },
                new Node { Name="Африка" },
                new Node { Name="Америка" },
                new Node { Name="Австралия" }
            };
        }
    }
}
