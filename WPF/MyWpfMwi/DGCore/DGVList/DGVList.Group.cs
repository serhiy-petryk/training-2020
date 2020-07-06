// сортировку лучше делать используя GroupBy + Order полученного объекта, чем прямой OrderBy
// Особенно она работает быстрее для строк (в 3 раза)

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DGCore.DGVList
{
  public partial class DGVList<TItem>
  {// : BindingList<object>, IIDGVList {

    static Dictionary<Type, ISortHelper> _helpers = new Dictionary<Type, ISortHelper>();

    static ISortHelper GetHelper(Type type)
    {
      if (!_helpers.ContainsKey(type))
      {
        //          Type typeHelper = typeof(GroupHelper<>).MakeGenericType(new Type[] { typeof(ItemWrapper<TItem>), typeof(TItem), type });
        Type typeHelper = typeof(SortHelper<>).MakeGenericType(new Type[] { typeof(TItem), type });
        ISortHelper helper = (ISortHelper)Activator.CreateInstance(typeHelper);
        _helpers.Add(type, helper);
        return helper;
      }
      return _helpers[type];
    }

    static IEnumerable<TItem> ApplyWherePredicates(IEnumerable<TItem> data, Delegate[] predicates)
    {
      foreach (Delegate d in predicates)
      {
        data = Enumerable.Where<TItem>(data, (Func<TItem, bool>)d);
      }
      return data;
    }

    // Recursive functions: in case of RequeryData
    void SortRecursive(IEnumerable<TItem> data, int level, IList destination)
    {

      if (Sorts.Count == 0)
      {// no sorting
        foreach (TItem o in data) destination.Add(o);
        return;
      }

      // Check, if the number of items > 1
      int cnt = 0;
      foreach (TItem x in data)
        if ((cnt++) > 0)
          break;

      if (cnt > 1)
      {// Number of items >1 : need to sort
        IEnumerable groupedData = ((PD.MemberDescriptor<TItem>)Sorts[level].PropertyDescriptor).GroupBy(data);

        // Sort groups (sortedGroup)
        ISortHelper helper = GetHelper(groupedData.GetType().GetGenericArguments()[1]);
        IEnumerable sortedGroups = helper.SortIGroupingItems(groupedData, Sorts[level].SortDirection);

        if (level < (Sorts.Count - 1))
        {
          // Call next group
          foreach (object o in sortedGroups)
          {
            this.SortRecursive((IEnumerable<TItem>)o, level + 1, destination);
          }
        }
        else
        {
          foreach (IEnumerable oo in sortedGroups)
          {
            foreach (object o in oo) destination.Add(o);
          }
        }
      }
      else
      {// Number of items <=1: Add item to destination object
        foreach (object o in data) destination.Add(o);
      }
    }

    void NewGroupRecursive(IEnumerable<TItem> data, int level, DGVList_GroupItem<TItem> parent)
    {
      if (level < Groups.Count)
      {// Add subgroups to group
        IEnumerable groupedData = ((PD.MemberDescriptor<TItem>)Groups[level].PropertyDescriptor).GroupBy(data);
        ISortHelper helper = this._helpersGroup[level];
        string keyName = Groups[level].PropertyDescriptor.Name;
        // Prepare Properties for objects
        PropertyDescriptorCollection pdc = null;
        Type pt = Groups[level].PropertyDescriptor.PropertyType;
        if (pt != typeof(string) && pt.IsClass)
        {
          pdc = PD.MemberDescriptorUtils.GetTypeMembers(pt);
        }
        // Call next group
        bool expanded = (level + 1) < _expandedGroupLevel;
        foreach (object o in groupedData)
        {
          object keyValue = helper.GetGroupingKey(o);
          if (keyValue != null)
          {
          }
          //            DGVList_GroupItem<TItem> newParent = parent.CreateChildGroup(keyName, keyValue, this._owner._isInitiallyExpanded ?? true, pdc);
          DGVList_GroupItem<TItem> newParent = parent.CreateChildGroup(keyName, keyValue, expanded, pdc);
          NewGroupRecursive((IEnumerable<TItem>)o, level + 1, newParent);// Proccess next group level
        }
      }
      else
      {// Add items to group
        if (level > 0)
        {
          //            bool? b1 = this._owner._isInitiallyExpanded;
          //          if (!b1.HasValue) parent._isExpanded = false;// Last level is not expanded when show structure mode
          //            if (this._owner._expandedGroupLevel.HasValue) parent._isExpanded = true;
        }
        parent._childItems = new List<TItem>(data);
        FilteredRowCount += parent._childItems.Count;
      }
    }

    void SetFilterByValueInGroupMode(Delegate del, DGVList_GroupItem<TItem> parent)
    {
      if (parent._childItems != null)
      {
        IEnumerable<TItem> data = Enumerable.Where<TItem>(parent._childItems, (Func<TItem, bool>)del);
        parent._childItems = new List<TItem>(data);
        //          this._owner._filteredRows += parent._childItems.Count;
      }
      else
      {
        foreach (DGVList_GroupItem<TItem> item in parent._childGroups)
        {
          SetFilterByValueInGroupMode(del, item);
        }
      }
    }
    void RemoveBlankGroups(DGVList_GroupItem<TItem> currentGroup)
    {
      currentGroup.ResetTotals(); // reset totals
      bool flagAddToDGV = (currentGroup.IsVisible && currentGroup.IsExpanded);
      if (currentGroup._childGroups != null)
      {
        for (int i = 0; i < currentGroup._childGroups.Count; i++)
        {
          if (currentGroup._childGroups[i].IsEmpty) currentGroup._childGroups.RemoveAt(i--);
          else
          {
            //              if (flagAddToDGV) this.Add(currentGroup._childGroups[i]);
            RemoveBlankGroups(currentGroup._childGroups[i]);
          }
        }
      }
      else
      {
        FilteredRowCount += currentGroup._childItems.Count;
      }
    }


    void SortGroups(DGVList_GroupItem<TItem> parent, IList destination, int level)
    {

      if (this._resetTotalFlag) parent.ResetTotals();// Reset totals
      if (parent._childGroups == null)
      {// item lines
       //          if (parent._isExpanded && parent.IsVisible) {
        CurrentExpandedGroupLevel = int.MaxValue;
        if (parent._childItems.Count < 2)
        {
          if (parent._childItems.Count > 0) destination.Add(parent._childItems[0]);
        }
        else
        {
          this.SortRecursive(parent._childItems, 0, destination);
        }
        //        }
      }
      else
      {
        if (CurrentExpandedGroupLevel < (level + 1))
          CurrentExpandedGroupLevel = level + 1;
        ISortHelper helper = this._helpersGroup[level];
        IEnumerable<DGVList_GroupItem<TItem>> sortedGroups = parent._childGroups;
        if (parent._childGroups.Count > 1)
        {
          for (int i = 0; i < SortsOfGroups[level].Count; i++)
          {
            ListSortDirection sortDirection = SortsOfGroups[level][i].SortDirection;
            PropertyDescriptor pd = SortsOfGroups[level][i].PropertyDescriptor;
            sortedGroups = GetSortedGroup_OneLevel(sortedGroups, sortDirection, i == 0, pd);
          }
          sortedGroups = helper.SortItems(sortedGroups, Groups[level].SortDirection, SortsOfGroups[level].Count == 0);
        }
        // Call next group
        foreach (DGVList_GroupItem<TItem> item in sortedGroups)
        {
          if (parent.IsVisible)
          {
            if (ShowGroupsOfUpperLevels || item.Level >= _expandedGroupLevel) destination.Add(item);
            if (item._isExpanded)
            {
              SortGroups(item, destination, level + 1);// Proccess next group level
            }
          }
        }//foreach (DGVList_GroupItem<TItem>
      }//if (parent._childGroups
    }

    IOrderedEnumerable<DGVList_GroupItem<TItem>> GetSortedGroup_OneLevel(IEnumerable<DGVList_GroupItem<TItem>> data, ListSortDirection sortOrder, bool isFirstLevel, PropertyDescriptor pd)
    {
      if (isFirstLevel)
      {
        if (sortOrder == ListSortDirection.Ascending)
          return Enumerable.OrderBy<DGVList_GroupItem<TItem>, object>(data,
            delegate (DGVList_GroupItem<TItem> item) { return pd.GetValue(item); });
        else
          return Enumerable.OrderByDescending<DGVList_GroupItem<TItem>, object>(data,
            delegate (DGVList_GroupItem<TItem> item) { return pd.GetValue(item); });
      }
      else
      {
        if (sortOrder == ListSortDirection.Ascending)
          return Enumerable.ThenBy<DGVList_GroupItem<TItem>, object>((IOrderedEnumerable<DGVList_GroupItem<TItem>>)data,
            delegate (DGVList_GroupItem<TItem> item) { return pd.GetValue(item); });
        else
          return Enumerable.ThenByDescending<DGVList_GroupItem<TItem>, object>((IOrderedEnumerable<DGVList_GroupItem<TItem>>)data,
            delegate (DGVList_GroupItem<TItem> item) { return pd.GetValue(item); });
      }
    }

    ISortHelper[] _helpersSort;
    ISortHelper[] _helpersGroup;
    Stopwatch _timer = new Stopwatch();

    void PrepareHelpers()
    {
      _helpersSort = new ISortHelper[Sorts.Count];
      for (int i = 0; i < _helpersSort.Length; i++)
      {
        this._helpersSort[i] = GetHelper(Sorts[i].PropertyDescriptor.PropertyType);
      }
      this._helpersGroup = new ISortHelper[Groups.Count];
      for (int i = 0; i < this._helpersGroup.Length; i++)
      {
        this._helpersGroup[i] = GetHelper(Groups[i].PropertyDescriptor.PropertyType);
      }//for (int i = 0;
    }

    // ============   Refresh  =====================
    enum RefreshMode { Common, AfterCommonColumnSortChanged, AfterGroupColumnSortChanged, AfterTotalGroupSortChanged, AfterFastFilterChanged, AfterFilterByValueChanged };

    public void RefreshData() => RefreshDataInternal(RefreshMode.Common);

    private void RefreshDataAfterCommonColumnSortChanged()
    {
      if (IsGroupMode)
        RefreshDataInternal(RefreshMode.AfterCommonColumnSortChanged);
      else
        RefreshDataInternal(RefreshMode.Common);
    }

    private void PrepareLiveTotalLines()
    {
      LiveTotalLines.Clear();
      if (IsGroupMode)
        foreach (var line in TotalLines.Where(tl => tl.TotalFunction != Common.Enums.TotalFunction.None))
        {
          line.PropertyDescriptor = Properties[line.Id];
          LiveTotalLines.Add(line);
        }
    }

    private DGVList_GroupItem<TItem> _rootGroup;
    private bool _resetTotalFlag = false;// Reset total: sorting is changing in group mode

    // see https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/
    SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
    private async void RefreshDataInternal(RefreshMode mode, params object[] parameters)
    {
      await _refreshLock.WaitAsync();
      try
      {
        if (_isDisposing)
          return;

        PrepareLiveTotalLines();
        if (UnderlyingData.IsDataReady)
           DataStateChanged?.Invoke(this, new Sql.DataSourceBase.SqlDataEventArgs(Sql.DataSourceBase.DataEventKind.BeforeRefresh));

        IEnumerable<TItem> data = (IEnumerable<TItem>) UnderlyingData.GetData(false);

        _timer.Reset();
        _timer.Start();

        RaiseListChangedEvents = false;
        Clear();
        var oldFilteredRows = FilteredRowCount;
        FilteredRowCount = 0;

        if (IsGroupMode)
        {
          var requeryFlag = false;
          if (mode == RefreshMode.AfterTotalGroupSortChanged)
          {
            FilteredRowCount = oldFilteredRows;
          }
          else if (mode == RefreshMode.AfterGroupColumnSortChanged)
          {
            // Sorting of Group column: // nothing to do
            FilteredRowCount = oldFilteredRows;
          }
          else if (mode == RefreshMode.AfterCommonColumnSortChanged && IsGroupMode)
          {
            // Sorting in Group mode
            FilteredRowCount = oldFilteredRows;
            _resetTotalFlag = TotalLines.Any(tl =>
              tl.TotalFunction == Common.Enums.TotalFunction.First ||
              tl.TotalFunction == Common.Enums.TotalFunction.Last);
          }
          else if (mode == RefreshMode.AfterFastFilterChanged)
          {
            // FastFilter changed in group mode
            PrepareFastFilter();
            SetFastFilterInGroupMode(this._rootGroup); // Set filter
            RemoveBlankGroups(this._rootGroup);
            /*              this.Items.Add(this._rootGroup);
                          PrepareHelpers();
                          this.SortGroups(this._rootGroup, this);*/
          }
          else if (mode == RefreshMode.AfterFilterByValueChanged)
          {
            // FilterByValue changed in group mode
            Delegate filterByValuePredicate = (Delegate)parameters[0];
            SetFilterByValueInGroupMode(filterByValuePredicate, this._rootGroup);
            RemoveBlankGroups(this._rootGroup);
            /*              this.Items.Add(this._rootGroup);
                          PrepareHelpers();
                          this.SortGroups(this._rootGroup, this);*/
          }
          else
          {
            // common
            data = SetFiltersWhileRefresh(data);
            this._rootGroup = new DGVList_GroupItem<TItem>();
            if (Groups.Count > 0)
              this._rootGroup._childGroups = new List<DGVList_GroupItem<TItem>>();
            if (LiveTotalLines.Count > 0)
              this._rootGroup.SetTotalsProperties(LiveTotalLines.ToArray());
            requeryFlag = true;
          }

          if (ShowTotalRow)
            Items.Add(this._rootGroup);
          PrepareHelpers();

          //Changed at 2015-04-02            if (this._owner._currentExpandedGroupLevel > this._helpersGroup.Length) this._owner._currentExpandedGroupLevel = -1;
          CurrentExpandedGroupLevel = -1;

          if (requeryFlag) this.NewGroupRecursive(data, 0, this._rootGroup); // requery data from original source
          this.SortGroups(this._rootGroup, this, 0);
          this._resetTotalFlag = false;
        }
        else
        {
          // not group mode
          data = SetFiltersWhileRefresh(data);
          CurrentExpandedGroupLevel = int.MaxValue;
          this.SortRecursive(data, 0, this);
          FilteredRowCount = this.Count;
        }

        RaiseListChangedEvents = true;
        ResetBindings(); // Need for sorting visualiztion
      }
      // ??? catch (Exception ex)
      finally
      {
        _refreshLock.Release();
      }
    }

    private IEnumerable<TItem> SetFiltersWhileRefresh(IEnumerable<TItem> data)
    {
      // Apply ByValue Filter
      if (FilterByValue != null)
      {
        Delegate[] byValuePredicates = FilterByValue.GetWherePredicates();
        if (byValuePredicates.Length > 0)
        {
          data = ApplyWherePredicates(data, byValuePredicates);
        }
      }
      // Apply where filter
      Delegate[] predicates = WhereFilter.GetWherePredicates();
      if (predicates.Length > 0)
      {
        data = ApplyWherePredicates(data, predicates);
      }
      int recs = Enumerable.Count(data);
      // Fast filter
      PrepareFastFilter();
      if (this._formattedValueObjects != null)
      {
        data = Enumerable.Where<TItem>(data, ApplyFastFilterPredicate);
      }
      return data;
    }

    // ======  Fast Filter
    // Using of native getter and typed DGV_FormattedValueToString increases speed only on 1-3%
    private Utils.DGVColumnHelper[] _formattedValueObjects;
    private string[] _txtFastFilters;

    private void PrepareFastFilter()
    {
      _formattedValueObjects = null;// reset
      _txtFastFilters = null;
      if (string.IsNullOrEmpty(TextFastFilter) || _getColumnHelpers == null)
        return;

      _formattedValueObjects = _getColumnHelpers();
      if (_formattedValueObjects.Length == 0)
        return;

      _txtFastFilters = TextFastFilter.ToLowerInvariant().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
      for (int i = 0; i < _txtFastFilters.Length; i++)
        _txtFastFilters[i] = _txtFastFilters[i].Trim();
    }

    private bool ApplyFastFilterPredicate(TItem o)
    {
      // 10% more slowly: _txtFastFilters.All(s => _formattedValueObjects.Any(x => x.Contains(o, s)));
      foreach (var s in _txtFastFilters)
      {
        var flag = false;
        foreach (Utils.DGVColumnHelper x in _formattedValueObjects)
          if (x.Contains(o, s))
          {
            flag = true;
            break;
          }
        if (!flag) return false;
      }
      return true;
    }

    void SetFastFilterInGroupMode(DGVList_GroupItem<TItem> parent)
    {
      if (parent._childItems != null)
      {
        //          IEnumerable<TItem> data = Enumerable.Where<TItem>(parent._childItems, (Func<TItem, bool>)del);
        IEnumerable<TItem> data = Enumerable.Where<TItem>(parent._childItems, ApplyFastFilterPredicate);
        parent._childItems = new List<TItem>(data);
      }
      else
      {
        foreach (DGVList_GroupItem<TItem> item in parent._childGroups)
        {
          SetFastFilterInGroupMode(item);
        }
      }
    }

    // ===== Expand/collapse items 
    public void ItemExpandedChanged(int rowIndex)
    {
      DGVList_GroupItem<TItem> item = this[rowIndex] as DGVList_GroupItem<TItem>;
      if (item != null)
      {
        _timer.Reset();
        _timer.Start();
        PrepareLiveTotalLines();

        DataStateChanged?.Invoke(this, new Sql.DataSourceBase.SqlDataEventArgs(Sql.DataSourceBase.DataEventKind.BeforeRefresh));

        // Do action
        if (item.IsExpanded)
        {
          int recs = item.ExpandedItemCount - 1;
          //            List<ItemWrapper<TItem>> oo = (List<ItemWrapper<TItem>>)this.Items;
          this.RaiseListChangedEvents = false;
          ((List<object>)this.Items).RemoveRange(rowIndex + 1, recs);
          item.IsExpanded = false;
          RefreshExpandedGroupLevel();
          this.RaiseListChangedEvents = true;
          this.ResetBindings();
        }
        else
        {
          item.IsExpanded = true;
          List<object> items = new List<object>();
          //            FillChildList(item, items);
          //            item.FillChildList(items);
          PrepareHelpers();
          this.SortGroups(item, items, item.Level);
          this.RaiseListChangedEvents = false;
          ((List<object>)this.Items).InsertRange(rowIndex + 1, items);
          this.RaiseListChangedEvents = true;
          this.ResetBindings();
        }
      }
    }

    void RefreshExpandedGroupLevel()
    {
      var maxGroupNo = -1;
      foreach (var o in this)
        if (o is IDGVList_GroupItem)
        {
          if (((IDGVList_GroupItem)o).Level > maxGroupNo)
            maxGroupNo = ((IDGVList_GroupItem)o).Level;
        }
        else
        {
          maxGroupNo = int.MaxValue;
          break;
        }
      CurrentExpandedGroupLevel = maxGroupNo;
    }

    private new void ResetBindings()
    {
      if (_isDisposing)
        return;

      base.ResetBindings(); // Need for sorting visualiztion

      _timer.Stop();
      LastRefreshedTimeInMsecs = Convert.ToInt32(_timer.Elapsed.TotalMilliseconds);
      if (UnderlyingData.IsDataReady)
        DataStateChanged?.Invoke(this,
          new Sql.DataSourceBase.SqlDataEventArgs(Sql.DataSourceBase.DataEventKind.Refreshed));
    }
  }

}
