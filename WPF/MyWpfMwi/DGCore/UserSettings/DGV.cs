using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace DGCore.UserSettings
{
  public class DGV
  {
    public int ExpandedGroupLevel;
    public bool ShowGroupsOfUpperLevels;
    public List<Filter> WhereFilter = new List<Filter>();
    public List<Filter> FilterByValue = new List<Filter>();
    public List<Column> AllColumns = new List<Column>();
    public List<string> FrozenColumns = new List<string>();
    public List<Sorting> Groups = new List<Sorting>();
    public List<Sorting> Sorts = new List<Sorting>();
    public List<List<Sorting>> SortsOfGroup = new List<List<Sorting>>();
    public List<TotalLine> TotalLines = new List<TotalLine>();
    public bool ShowTotalRow = false;
    public bool IsGridVisible = true;
    public Common.Enums.DGCellViewMode CellViewMode = Common.Enums.DGCellViewMode.OneRow;
    public Font BaseFont = null;
    public string TextFastFilter = null;
  }

  public class Filter
  {
    public string Name;
    public bool Not;
    public bool? IgnoreCase;
    public List<FilterLine> Lines = new List<FilterLine>();
  }

  public class FilterLine
  {
    public Common.Enums.FilterOperand Operand;
    public object Value1;
    public object Value2;
  }

  public class Column
  {
    public string Id;
    public string DisplayName;
    public bool IsHidden;
    public int? Width;
    public override string ToString() => $"Id={Id}, DisplayName={DisplayName}, IsHidden={IsHidden}, Width={Width}";
  }

  public class Sorting
  {
    public string Id;
    public ListSortDirection SortDirection;
    public override string ToString() => $"Id={Id}, Direction={SortDirection}";
  }

  public class TotalLine: Common.ITotalLine
  {
    public string Id { get; set; }
    public int DecimalPlaces { get; set; }
    public Common.Enums.TotalFunction TotalFunction { get; set; }
    public override string ToString() => $"Id={Id}, DecimalPlaces={DecimalPlaces}, Function={TotalFunction}";
  }
}