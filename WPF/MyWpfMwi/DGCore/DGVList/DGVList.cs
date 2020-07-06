using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace DGCore.DGVList
{
  public partial class DGVList<TItem> : BindingList<object>, IDGVList
  {
    public PropertyDescriptorCollection Properties => UnderlyingData.Properties;
    public Sql.DataSourceBase UnderlyingData { get; }
    public List<ListSortDescription> Sorts { get; } = new List<ListSortDescription>();
    public List<ListSortDescription> Groups { get; } = new List<ListSortDescription>();
    public List<List<ListSortDescription>> SortsOfGroups { get; } = new List<List<ListSortDescription>>();
    public Misc.TotalLine[] TotalLines { get; }
    public List<Misc.TotalLine> LiveTotalLines { get; } = new List<Misc.TotalLine>();
    public int LastRefreshedTimeInMsecs { get; private set; } = 0;

    private int _expandedGroupLevel = 1;
    public int ExpandedGroupLevel
    {
      get
      {
        if (Groups.Count > 0 && _expandedGroupLevel < 1)
          _expandedGroupLevel = 1;
        if (_expandedGroupLevel != int.MaxValue && _expandedGroupLevel > Groups.Count)
          _expandedGroupLevel = Groups.Count;

        return _expandedGroupLevel;
      }
      set => _expandedGroupLevel = value;
    }

    public bool ShowGroupsOfUpperLevels { get; set; } = true;
    public bool ShowTotalRow { get; private set; }
    public bool IsGroupMode => Groups.Count > 0 || ShowTotalRow;
    public int CurrentExpandedGroupLevel { get; private set; } = 0;
    public int FilteredRowCount { get; private set; } = 0;

    public Filters.FilterList WhereFilter { get; private set; }
    public Filters.FilterList FilterByValue { get; private set; }
    public string TextFastFilter { get; private set; }

    public bool IsPropertyVisible(string propertyName) =>
      CurrentExpandedGroupLevel == int.MaxValue ||
      Enumerable.Range(0, CurrentExpandedGroupLevel).Any(i =>
        Groups[i].PropertyDescriptor.Name == propertyName ||
        propertyName.StartsWith(Groups[i].PropertyDescriptor.Name + ".")) || LiveTotalLines.Any(tl =>
        tl.PropertyDescriptor.Name == propertyName || propertyName.StartsWith(tl.PropertyDescriptor.Name + "."));

    public bool IsGroupColumnVisible(int groupIndex) =>
      (Groups.Count > 0 && groupIndex < CurrentExpandedGroupLevel &&
       (ShowGroupsOfUpperLevels || groupIndex >= (ExpandedGroupLevel - 1)));

    public async void RequeryData() => await Task.Factory.StartNew((Action) (() => UnderlyingData.GetData(true)));

    public event Sql.DataSourceBase.dlgDataEvent DataStateChanged;

    private Func<Utils.DGVColumnHelper[]> _getColumnHelpers;

    public DGVList(Sql.DataSourceBase dataSource, Func<Utils.DGVColumnHelper[]> getColumnHelpers)
    {
      UnderlyingData = dataSource;
      _getColumnHelpers = getColumnHelpers;
      WhereFilter = new Filters.FilterList(Properties);
      // FilterByValue = null;

      TotalLines = UnderlyingData.Properties.Cast<PropertyDescriptor>()
        .Where(pd => pd.IsBrowsable && Misc.TotalLine.IsTypeSupport(Utils.Types.GetNotNullableType(pd.PropertyType)))
        .Select(pd => new Misc.TotalLine(pd)).ToArray();

      UnderlyingData.DataEventHandler += UnderlyingData_DataEventHandler;
    }

    private void UnderlyingData_DataEventHandler(object sender, Sql.DataSourceBase.SqlDataEventArgs e) => DataStateChanged?.Invoke(this, e);

    protected override object AddNewCore()
    {
      var o = Activator.CreateInstance(this.UnderlyingData.ItemType);
      base.Add(o);
      return o;
    }

    // ===========  IDisposable  ================
    private bool _isDisposing = false;
    public async void Dispose()
    {
      if (_isDisposing)
        return;

      _isDisposing = true;
      await _refreshLock.WaitAsync();
      try
      {
        // _sqlDataSource?.Dispose(); // error on dgv clone: 
        UnderlyingData.DataEventHandler -= UnderlyingData_DataEventHandler;

        RaiseListChangedEvents = false;
        Items.Clear();
        WhereFilter = null;
        FilterByValue = null;
        LiveTotalLines.Clear();
        _getColumnHelpers = null;
        this._formattedValueObjects = null;
        this._helpersGroup = null;
        this._helpersSort = null;
        this._rootGroup = null;
        //        if (Disposed != null) Disposed.Invoke(this, new EventArgs());
      }
      // ??? catch (Exception ex)
      finally
      {
        _refreshLock.Release();
        _refreshLock.Dispose();
      }
    }

    // =========  ITypedList  ============
    public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors) => UnderlyingData.Properties;
    public string GetListName(PropertyDescriptor[] listAccessors) => UnderlyingData.ItemType.Name;
  }
}
