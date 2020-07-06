using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using DataCommon.DataTasks;
using DataCommon.Models;
using Common = First.Model.Common;
using GlDocLine = First.Model.GlDocLine;
using GlDocList = First.Model.GlDocList;

namespace First
{
  /// <summary>
  /// Interaction logic for MainWindowSort.xaml
  /// </summary>
  public partial class MainWindowSort : Window
  {
    public static readonly DependencyProperty IsCommandBarEnabledProperty =
      DependencyProperty.Register("IsCommandBarEnabled", typeof(bool), typeof(MainWindowSort), new UIPropertyMetadata(true));
    public bool IsCommandBarEnabled
    {
      get { return (bool)GetValue(IsCommandBarEnabledProperty); }
      set { SetValue(IsCommandBarEnabledProperty, value); }
    }
    //===========
    public static bool IsNullableType(Type type) => type != null && Nullable.GetUnderlyingType(type) != null;
    public static Type GetNotNullableType(Type type) => IsNullableType(type) ? Nullable.GetUnderlyingType(type) : type;

    public static ObservableCollection<string> LogData = new ObservableCollection<string>();
    private static Type _dataType;


    private DataGrid dg;
    // private IList originalData;
    // private IList data;

    private List<(string, ListSortDirection)> _sortedList = new List<(string, ListSortDirection)>();

    public MainWindowSort()
    {
      InitializeComponent();
      var log = (ComboBox)FindName("Log");
      log.ItemsSource = LogData;
      LogData.Add("Test");
    }

    private void DgClear()
    {
      dg.ItemsSource = null;
      dg.Columns.Clear();
    }

    private void CreateColumns()
    {
      // var type = dg.ItemsSource.GetType().GenericTypeArguments[0];
      CreateColumnsRecursive(_dataType, new List<string>(), 0);
    }

    private void CreateColumnsRecursive(Type type, List<string> prefixes, int level)
    {
      if (level > 10)
        return;

      var types = type.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
      var cnt = 0;
      foreach (var t in types)
      {
        DataGridBoundColumn column = null;

        var t1 = GetNotNullableType(t.PropertyType);

        switch (Type.GetTypeCode(t1))
        {
          case TypeCode.Boolean:
            column = new DataGridCheckBoxColumn();
            break;

          case TypeCode.Byte:
          case TypeCode.Decimal:
          case TypeCode.Double:
          case TypeCode.Int16:
          case TypeCode.Int32:
          case TypeCode.Int64:
          case TypeCode.SByte:
          case TypeCode.Single:
          case TypeCode.UInt16:
          case TypeCode.UInt32:
          case TypeCode.UInt64:
            column = (DataGridBoundColumn)FindResource("NumberColumn");
            break;

          case TypeCode.String:
          case TypeCode.DateTime:
            column = (DataGridBoundColumn)FindResource("TextColumn");
            break;

          case TypeCode.Object:
            var newPrefixes = new List<string>(prefixes);
            newPrefixes.Add(t.Name);
            CreateColumnsRecursive(t1, newPrefixes, level + 1);
            break;

          default:
            throw new Exception("Check CreateColumns method");
        }

        if (column != null)
        {
          column.Header = (string.Join(" ", prefixes) + " " + t.Name).Trim();
          column.Binding = new Binding(prefixes.Count == 0 ? t.Name : string.Join(".", prefixes) + "." + t.Name);
          // ??? Sort support for BindingList=> doesn't work column.SortMemberPath = prefixes.Count == 0 ? t.Name : string.Join(".", prefixes) + "." + t.Name;
          dg.Columns.Add(column);
        }
      }
    }

    private void Load_OnClick(object sender, RoutedEventArgs e)
    {
      if (!IsCommandBarEnabled) return;
      IsCommandBarEnabled = false;

      DgClear();
      _dataType = typeof(DataCommon.Models.GlDocLine);

      var task = Task.Factory.StartNew(() =>
      {
        var sw = new Stopwatch();
        sw.Start();
        ItemHelper.LoadData(_dataType);
        sw.Stop();
        var d1 = sw.Elapsed.TotalMilliseconds;
        sw.Reset();
        sw.Start();
        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
          (Action)delegate ()
          {
            dg.ItemsSource = ItemHelper.GetDataNoCache(_dataType, _sortedList);
            sw.Stop();
            var d2 = sw.Elapsed.TotalMilliseconds;
            LogData.Add("Load data time: " + d1);
            LogData.Add("Get data time: " + d2);
            CreateColumns();
            IsCommandBarEnabled = true;
          }
        );
      });
    }

    private void LoadGlDocList_OnClick(object sender, RoutedEventArgs e)
    {
      DgClear();
      _dataType = typeof(GlDocList);
      var recs = int.Parse(((TextBox)FindName("Records")).Text);
      GlDocList.RefreshData(recs);
      // dg.ItemsSource = new ObservableCollection<GlDocList>(GlDocList.Data.Values);
      dg.ItemsSource = new ObservableCollection<object>(GlDocList.Data.Values);
      // originalData = new ObservableCollection<GlDocList>(Utils.Data.GetGlDocList(recs));
      CreateColumns();
    }

    private void MainWindowSort_OnLoaded(object sender, RoutedEventArgs e)
    {
      dg = (DataGrid)this.FindName("DataGrid");
    }

    private void Clear_OnClick(object sender, RoutedEventArgs e)
    {
      DgClear();
    }

    private void Records_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      Regex regex = new Regex("[^0-9]+");
      e.Handled = regex.IsMatch(e.Text);
    }

    private void MemoryInUse_OnClick(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Memory: " + Common.MemoryUsedInKB);
    }

    private void Debug_OnClick(object sender, RoutedEventArgs e)
    {
      var x = dg.ItemsSource as BindingList<object>;
      /*if (x != null)
      {
        x.RaiseListChangedEvents = false;
        x.Insert(0, x[3]);
        x.RaiseListChangedEvents = true;
        // dg.Items.Refresh();
        x.ResetBindings();
      }
      var a = dg.ItemsSource as List<object>;
      if (a != null)
      {
        // x.RaiseListChangedEvents = false;
        a.Insert(0, a[3]);
        var a2 = dg.Items;
        dg.Items.Refresh();
        // x.RaiseListChangedEvents = true;
        // x.ResetBindings();
      }*/
      DataGridSortingEventArgs a = new DataGridSortingEventArgs(dg.Columns[0]);
      /*if (dg.Columns[0].SortDirection == ListSortDirection.Ascending)
        dg.Columns[0].SortDirection = ListSortDirection.Descending;
      else if (dg.Columns[0].SortDirection == ListSortDirection.Descending)
        dg.Columns[0].SortDirection = null;
      else
        dg.Columns[0].SortDirection = ListSortDirection.Ascending;*/
      FilteringDataGrid_Sorting(dg, a);
    }

    void FilteringDataGrid_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
    {
      if (!string.IsNullOrEmpty(e.Column.SortMemberPath))
      {
        var sw = new Stopwatch();
        sw.Start();
        ListSortDirection? newSortDirection = null;

        if (e.Column.SortDirection == null)
          newSortDirection = ListSortDirection.Ascending;
        else if (e.Column.SortDirection == ListSortDirection.Ascending)
          newSortDirection = ListSortDirection.Descending;

        var elements = _sortedList.Where(item => item.Item1 == e.Column.SortMemberPath).ToArray();
        foreach (var element in elements)
          _sortedList.Remove(element);
        if (newSortDirection.HasValue)
          _sortedList.Insert(0, (e.Column.SortMemberPath, newSortDirection.Value));

        /*var a1 = dg.ItemsSource as BindingList<object>;
        if (a1 == null)
          dg.ItemsSource = ItemHelper.GetDataGroupingSortNoCache(_dataType, _sortedList);
        else
        {
          var a2 = ItemHelper.GetDataGroupingSortNoCache(_dataType, _sortedList);
          a1.RaiseListChangedEvents = false;
          a1.Clear();
          foreach (var a3 in a2)
            a1.Add(a2);
          a1.RaiseListChangedEvents = true;
          a1.ResetBindings();
        }*/

        var helper = typeof(HelperSort<>).MakeGenericType(_dataType);
        var sortMI = helper.GetMethod("SortRecursive", BindingFlags.Public | BindingFlags.Static);
        var getDataMI = _dataType.GetMethod("GetData", BindingFlags.Public | BindingFlags.Static);
        var sourceData = getDataMI.Invoke(null, null);

        ((IList)dg.ItemsSource).Clear();
        var time = (double)sortMI.Invoke(null, new[] {_sortedList, sourceData, 0, dg.ItemsSource});
        dg.Items.Refresh();

        e.Column.SortDirection = null;
        foreach (var element in _sortedList)
          dg.Columns.Where(c => c.SortMemberPath == element.Item1).ToList().ForEach(c=> c.SortDirection = element.Item2);

        e.Handled = true;

        sw.Stop();
        var d1 = sw.Elapsed.TotalMilliseconds;
        LogData.Add("Get data time internal: " + time);
        LogData.Add("Get data time: " + d1);
      }
    }

    void FilteringDataGridOld_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
    {
      // if (e.Column.SortMemberPath == "LINENO")
      if (e.Column.SortMemberPath == "oACCOUNT.LONG_TEXT")
      {
        var sw = new Stopwatch();
        sw.Start();
        if (e.Column.SortDirection == null)
        {
          // dg.ItemsSource = (GlDocLine.Data.Values).OrderBy(item => item.LINENO);
          dg.ItemsSource = (GlDocLine.Data.Values).OrderBy(item => item.oACCOUNT?.LONG_TEXT);
          e.Column.SortDirection = ListSortDirection.Ascending;
        }
        else if (e.Column.SortDirection == ListSortDirection.Ascending)
        {
          // dg.ItemsSource = (GlDocLine.Data.Values).OrderByDescending(item => item.LINENO);
          dg.ItemsSource = (GlDocLine.Data.Values).OrderByDescending(item => item.oACCOUNT?.LONG_TEXT);
          e.Column.SortDirection = ListSortDirection.Descending;
        }
        else
        {
          dg.ItemsSource = GlDocLine.Data.Values;
          e.Column.SortDirection = null;
        }

        /*if (e.Column.SortDirection == ListSortDirection.Ascending)
          dg.ItemsSource = ((IEnumerable<GlDocLine>) originalData).OrderBy(item => item.DOCKEY);
        else if (e.Column.SortDirection == ListSortDirection.Descending)
          dg.ItemsSource = ((IEnumerable<GlDocLine>) originalData).OrderByDescending(item => item.DOCKEY);*/

        sw.Stop();
        var d1 = sw.Elapsed.TotalMilliseconds;
        e.Handled = true;
      }

    }

    void FilteringDataGrid_SortingX(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
    {
      var ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

      var view = CollectionViewSource.GetDefaultView(dg.Items);
      if (e.Column.SortDirection == null)
        e.Column.SortDirection = ListSortDirection.Ascending;
      else if (e.Column.SortDirection == ListSortDirection.Ascending)
        e.Column.SortDirection = ListSortDirection.Descending;
      else
        e.Column.SortDirection = null;

      /*var type = originalData.GetType().GenericTypeArguments[0];
      var codeGetter = CreateGetter(type, e.Column.SortMemberPath);
      var ii = type.GetMethods();
      var mm = typeof(System.Linq.Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.Name == "OrderBy" && m.GetParameters().Length == 2).ToList();
      var mm1 = typeof(System.Linq.Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2).ToList();

      var ocType = typeof(ObservableCollection<>);
      var t = ocType.MakeGenericType(type);
      var cc = t.GetConstructors().Where(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType.Name.StartsWith("IEnum")).ToList();

      object paramValue = null;
      if (e.Column.SortDirection == ListSortDirection.Ascending)
        paramValue = mm[0].MakeGenericMethod(typeof(object), typeof(object)).Invoke(null, new object[] { originalData, codeGetter });
      else if (e.Column.SortDirection == ListSortDirection.Descending)
        paramValue = mm1[0].Invoke(null, new object[] { originalData, codeGetter });

      if (paramValue != null)
      {
        // var a1 = Enumerable.OrderBy(originalData, codeGetter);
        dg.ItemsSource = (IEnumerable)cc[0].Invoke(null, new[] { paramValue });
      }*/

      e.Handled = true;

    }

    private void SetData()
    {
      if (GlDocLine.Data == null && GlDocList.Data == null)
        dg.ItemsSource = null;
      else if (GlDocLine.Data == null)
        dg.ItemsSource = new ObservableCollection<GlDocList>(GlDocList.Data.Values);
      else
        dg.ItemsSource = new ObservableCollection<GlDocLine>(GlDocLine.Data.Values);
    }

    public static Func<object, object> CreateGetter(Type runtimeType, string propertyName)
    {
      var propertyInfo = runtimeType.GetProperty(propertyName);

      // create a parameter (object obj)
      var obj = System.Linq.Expressions.Expression.Parameter(typeof(object), "obj");

      // cast obj to runtimeType
      var objT = System.Linq.Expressions.Expression.TypeAs(obj, runtimeType);

      // property accessor
      var property = System.Linq.Expressions.Expression.Property(objT, propertyInfo);

      var convert = System.Linq.Expressions.Expression.TypeAs(property, typeof(object));
      return (Func<object, object>)System.Linq.Expressions.Expression.Lambda(convert, obj).Compile();
    }
  }

}
