using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCommon.Models;

namespace WndTests
{
  public partial class Form1 : Form
  {
    public static bool IsNullableType(Type type) => type != null && Nullable.GetUnderlyingType(type) != null;
    public static Type GetNotNullableType(Type type) => IsNullableType(type) ? Nullable.GetUnderlyingType(type) : type;

    // public static ObservableCollection<string> LogData = new ObservableCollection<string>();
    public static BindingList<string> LogData = new BindingList<string>();
    private static Type _dataType;
    private List<(string, ListSortDirection)> _sortedList = new List<(string, ListSortDirection)>();

    public Form1()
    {
      InitializeComponent();

      /*typeof(DataGridView).InvokeMember(
        "DoubleBuffered",
        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
        null,
        dgv,
        new object[] { true });*/

      this.dgv.AutoGenerateColumns = false;
      this.comboBox1.DataSource = LogData;

    }

    private void CreateColumns()
    {
      this.dgv.Columns.Clear();
      // var type = dg.ItemsSource.GetType().GenericTypeArguments[0];
      CreateColumnsRecursive(_dataType, new List<string>(), 0);

      /*var t = typeof(List<>).MakeGenericType(_dataType);
      this.dgv.DataSource = Activator.CreateInstance(t);
      foreach (DataGridViewColumn col in this.dgv.Columns)
        if (col.SortMode == DataGridViewColumnSortMode.Automatic)
          col.SortMode = DataGridViewColumnSortMode.Programmatic;*/
    }

    private void CreateColumnsRecursive(Type type, List<string> prefixes, int level)
    {
      if (level > 10)
        return;

      var types = type.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
      var cnt = 0;
      foreach (var t in types)
      {
        DataGridViewColumn column = null;
        // DataGridBoundColumn column = null;

        var t1 = GetNotNullableType(t.PropertyType);

        switch (Type.GetTypeCode(t1))
        {
          case TypeCode.Boolean:
            column = new DataGridViewCheckBoxColumn();
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
            column = new DataGridViewTextBoxColumn() ;
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            break;

          case TypeCode.String:
          case TypeCode.DateTime:
            column = new DataGridViewTextBoxColumn();
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            break;

          case TypeCode.Object:
            // var newPrefixes = new List<string>(prefixes);
            // newPrefixes.Add(t.Name);
            // CreateColumnsRecursive(t1, newPrefixes, level + 1);
            break;

          default:
            throw new Exception("Check CreateColumns method");
        }

        if (column != null)
        {
          column.DataPropertyName = t.Name;
          column.SortMode = DataGridViewColumnSortMode.Programmatic;
          column.HeaderText = t.Name;
          dgv.Columns.Add(column);
          /*column.Header = (string.Join(" ", prefixes) + " " + t.Name).Trim();
          column.Binding = new Binding(prefixes.Count == 0 ? t.Name : string.Join(".", prefixes) + "." + t.Name);
          // ??? Sort support for BindingList=> doesn't work column.SortMemberPath = prefixes.Count == 0 ? t.Name : string.Join(".", prefixes) + "." + t.Name;
          dgv.Columns.Add(column);*/
        }
      }
    }

    private void DgClear()
    {
      dgv.DataSource = null;
      dgv.Columns.Clear();
    }

    private void BtnLoadData_Click(object sender, EventArgs e)
    {
      DgClear();
      _dataType = typeof(DataCommon.Models.GlDocLine);

      var sw = new Stopwatch();
      sw.Start();
      ItemHelper.LoadData(_dataType);
      sw.Stop();
      var d1 = sw.Elapsed.TotalMilliseconds;
      sw.Reset();
      sw.Start();

      // Task.Factory.StartNew(()=>
      // {
      dgv.DataSource = ItemHelper.GetDataNoCache(_dataType, _sortedList);
      CreateColumns();
        sw.Stop();
        var d2 = sw.Elapsed.TotalMilliseconds;
        LogData.Add("Load data time: " + d1);
        LogData.Add("Get data time: " + d2);
      // });

      /*var task = Task.Factory.StartNew(() =>
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
            dgv.DataSource = ItemHelper.GetDataNoCache(_dataType, _sortedList);
            sw.Stop();
            var d2 = sw.Elapsed.TotalMilliseconds;
            LogData.Add("Load data time: " + d1);
            LogData.Add("Get data time: " + d2);
            CreateColumns();
          }
        );
      });*/

    }

    private void Dgv_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
      var sw = new Stopwatch();
      sw.Start();
      var name = dgv.Columns[e.ColumnIndex].DataPropertyName;

      var a1 = _sortedList.Where(i => i.Item1 == name).ToArray();
      if (a1.Length == 0)
        _sortedList.Insert(0, (name, ListSortDirection.Ascending));
      else if (a1[0].Item2 == ListSortDirection.Ascending)
      {
        _sortedList.Remove(a1[0]);
        _sortedList.Insert(0, (name, ListSortDirection.Descending));
      }
      else
        _sortedList.Remove(a1[0]);

      /*var a2 = (BindingList<GlDocLine>) dgv.DataSource;
      a2.RaiseListChangedEvents = false;
      a2.Clear();
      // foreach (var item in ItemHelper.GetDataNoCache(_dataType, _sortedList))
      // a2.Add((GlDocLine)item);
      a2.ResetBindings();
      foreach (var item in ItemHelper.GetDataNoCache(_dataType, _sortedList))
       a2.Add((GlDocLine)item);
      a2.RaiseListChangedEvents = true;
      a2.ResetBindings();*/

      var a2 = (List<GlDocLine>) dgv.DataSource;
      a2.Clear();
      a2.AddRange((IEnumerable<GlDocLine>)ItemHelper.GetDataNoCache(_dataType, _sortedList));
      // foreach (var item in ItemHelper.GetDataNoCache(_dataType, _sortedList))
       // a2.Add((GlDocLine)item);

      // dgv.DataSource = ItemHelper.GetDataNoCache(_dataType, _sortedList);
      var pi = (PropertyInfo) dgv.GetType().GetMember("DataConnection",BindingFlags.Instance | BindingFlags.NonPublic)[0];
      var a4 = pi.GetValue(dgv);
      var pi2 = (FieldInfo) a4.GetType().GetMember("currencyManager", BindingFlags.Instance | BindingFlags.NonPublic)[0];
      var a5 = (CurrencyManager) pi2.GetValue(a4);
      // var a3 = dgv.DataSource.DataCo
      a5.Refresh();

      sw.Stop();
      var d1 = sw.Elapsed.TotalMilliseconds;
      LogData.Add("Get data time: " + d1);
    }
  }
}
