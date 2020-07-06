using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using First.Model;

namespace First
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

    public static bool IsNullableType(Type type) => type != null && Nullable.GetUnderlyingType(type) != null;
    public static Type GetNotNullableType(Type type) => IsNullableType(type) ? Nullable.GetUnderlyingType(type) : type;

    public static ObservableCollection<string> LogData = new ObservableCollection<string>();

    private DataGrid dg;
    private IList data;


    public MainWindow()
    {
      InitializeComponent();
      var log = (ComboBox) FindName("Log");
      log.ItemsSource = LogData;
      LogData.Add("Test");
    }

    private void DgClear()
    {
      dg.ItemsSource = null;
      dg.Columns.Clear();
      data = null;
    }
    private void CreateColumns()
    {
      dg.ItemsSource = data;
      var type = data.GetType().GenericTypeArguments[0];
      var types = type.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
      var cnt = 0;
      foreach (var t in types)
      {
        if (cnt++ > 10)
          break;

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

          default:
            column = (DataGridBoundColumn)FindResource("TextColumn");
            break;
        }

        column.Header = t.Name;
        column.Binding = new Binding(t.Name);
        dg.Columns.Add(column);
      }
    }

    private void Load_OnClick(object sender, RoutedEventArgs e)
    {
      DgClear();
      var recs = int.Parse(((TextBox) FindName("Records")).Text);
      data = new ObservableCollection<GlDocLine>(Utils.Data.GetGlDocLine(recs));
      // data = new ObservableCollection<DataCommon.Models.GlDocLine>((IEnumerable<DataCommon.Models.GlDocLine>)Utils.Data.GetGlDocLineGlobal(null));
      CreateColumns();
    }
    private void LoadGlDocList_OnClick(object sender, RoutedEventArgs e)
    {
      DgClear();
      var recs = int.Parse(((TextBox)FindName("Records")).Text);
      data = new ObservableCollection<GlDocList>(Utils.Data.GetGlDocList(recs));
      CreateColumns();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
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
      MessageBox.Show("Memory: " +Common.MemoryUsedInKB);
    }

    private void Debug_OnClick(object sender, RoutedEventArgs e)
    {
      var x = 0;
    }

    private void ColumnHeader_Click(object sender, RoutedEventArgs e)
    {
      var columnHeader = sender as DataGridColumnHeader;
      if (columnHeader != null)
      {
        // dg.SelectedCells.Clear();
        foreach (var item in dg.Items)
        {
          dg.SelectedCells.Add(new DataGridCellInfo(item, columnHeader.Column));
        }
      }
    }
  }
}
