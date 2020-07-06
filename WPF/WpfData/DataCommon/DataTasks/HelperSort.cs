using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataCommon.DataTasks
{
  public class HelperSort<TItem>
  {
    static Dictionary<Type, ISortHelper> _helpers = new Dictionary<Type, ISortHelper>();

    interface ISortHelper
    {
      // IEnumerable SortItems(IEnumerable<TItem> nonSortedGroups, ListSortDirection sortOrder);
      IEnumerable SortIGroupingItems(IEnumerable nonSortedGroups, ListSortDirection sortOrder);
      // IOrderedEnumerable<DGV_GroupItem<TItem>> SortItems(IEnumerable nonSortedGroups, ListSortDirection sortOrder, bool isFirstLevel);
      object GetGroupingKey(object o);
    }

    private static ISortHelper GetHelper(Type type)
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

    class SortHelper<TKeyType> : ISortHelper
    {
      static Func<IGrouping<TKeyType, TItem>, TKeyType> func = delegate (IGrouping<TKeyType, TItem> item) { return item.Key; };
      // static Func<DGV_GroupItem<TItem>, TKeyType> funcReoder = delegate (DGV_GroupItem<TItem> item) { return (TKeyType)item._propertyValue; };

      /*public IEnumerable SortItems(IEnumerable<TItem> data, ListSortDirection sortOrder)
      {
        if (sortOrder == ListSortDirection.Ascending)
          return data.OrderBy(func);
          // return Enumerable.OrderBy((IEnumerable<TItem>)data, func);
        else
        {
          return Enumerable.OrderByDescending((IEnumerable<IGrouping<TKeyType, TItem>>)data, func);
        }
      }*/
      public IEnumerable SortIGroupingItems(IEnumerable nonSortedGroups, ListSortDirection sortOrder)
      {
        if (sortOrder == ListSortDirection.Ascending)
          return Enumerable.OrderBy((IEnumerable<IGrouping<TKeyType, TItem>>)nonSortedGroups, func);
        else
        {
          return Enumerable.OrderByDescending((IEnumerable<IGrouping<TKeyType, TItem>>)nonSortedGroups, func);
        }
      }
      /*public IOrderedEnumerable<DGV_GroupItem<TItem>> SortItems(IEnumerable nonSortedGroups, ListSortDirection sortOrder, bool isFirstLevel)
      {
        if (isFirstLevel)
        {
          if (sortOrder == ListSortDirection.Ascending)
          {
            return Enumerable.OrderBy((IEnumerable<DGV_GroupItem<TItem>>)nonSortedGroups, funcReoder);
          }
          else
          {
            return Enumerable.OrderByDescending((IEnumerable<DGV_GroupItem<TItem>>)nonSortedGroups, funcReoder);
          }
        }
        else
        {
          if (sortOrder == ListSortDirection.Ascending)
          {
            return Enumerable.ThenBy((IOrderedEnumerable<DGV_GroupItem<TItem>>)nonSortedGroups, funcReoder);
          }
          else
          {
            return Enumerable.ThenByDescending((IOrderedEnumerable<DGV_GroupItem<TItem>>)nonSortedGroups, funcReoder);
          }
        }
      }*/

      public object GetGroupingKey(object o)
      {
        return func((IGrouping<TKeyType, TItem>)o);
      }
    }

    //========================================
    //========================================

    // Recursive functions: in case of RequeryData
    // public static void SortRecursive(List<ListSortDescription> _sorts, IEnumerable<TItem> data, int level, IList destination)
    public static double SortRecursive(List<(string, ListSortDirection)> _sorts, IEnumerable<TItem> source, int level, IList destination)
    {
      double d1 = 0;
      if (_sorts.Count == 0)
      {// no sorting
        foreach (TItem o in source) destination.Add(o);
        return d1;
      }

      // Check, if the number of items > 1
      int cnt = 0;
      foreach (TItem x in source)
      {
        if ((cnt++) > 0) break;
      }

      if (cnt > 1)
      {// Number of items >1 : need to sort
        var sw = new Stopwatch();
        sw.Start();
        // IEnumerable groupedData = ((PD.MemberDescriptor<TItem>)_sorts[level].PropertyDescriptor).GroupBy(data);

        var gettersFI = typeof(TItem).GetField("Getters", BindingFlags.Public | BindingFlags.Static);
        var getters = (Dictionary<string, object>)gettersFI.GetValue(null);
        var getter = getters[_sorts[level].Item1];
        var returnType = getter.GetType().GenericTypeArguments[1];

        var groupByMI = typeof(HelperSort<TItem>).GetMethod("GroupBy", BindingFlags.Public | BindingFlags.Static);
        var groupByMI2 = groupByMI.MakeGenericMethod(returnType);
        IEnumerable groupedData = (IEnumerable)groupByMI2.Invoke(null, new [] {source, getter});

        // Sort groups (sortedGroup)
        ISortHelper helper = GetHelper(groupedData.GetType().GetGenericArguments()[1]);
        IEnumerable sortedGroups = helper.SortIGroupingItems(groupedData, _sorts[level].Item2);

        if (level < (_sorts.Count - 1))
        {
          // Call next group
          foreach (object o in sortedGroups)
            SortRecursive(_sorts, (IEnumerable<TItem>)o, level + 1, destination);
        }
        else
        {
          foreach (IEnumerable oo in sortedGroups)
            foreach (object o in oo) destination.Add(o);
          // foreach (object o in sortedGroups) destination.Add(o);
        }

        sw.Stop();
        d1 = sw.Elapsed.TotalMilliseconds;
      }
      else
      {// Number of items <=1: Add item to destination object
        foreach (object o in source) destination.Add(o);
      }

      return d1;
    }

    public static IEnumerable GroupBy<TReturn>(IEnumerable<TItem> source, Func<TItem, TReturn> getter)
    {
      MethodInfo _miGeneric_GroupBy = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "GroupBy" && mi.GetParameters().Length == 2).ToArray()[0];
      MethodInfo groupByMI = _miGeneric_GroupBy.MakeGenericMethod(new Type[] { typeof(TItem), typeof(TReturn) });
      return (IEnumerable)groupByMI.Invoke(null, new object[] { source, getter });
    }

  }
}
