using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DataCommon.Models
{
  public class ItemHelper
  {
    private static readonly MethodInfo AsParalelMi = typeof(ParallelEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "AsParallel" && mi.IsGenericMethod && mi.GetParameters()[0].ParameterType.Name == "IEnumerable`1").ToArray()[0];
    private static readonly MethodInfo OrderByAscendingMi = typeof(ParallelEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "OrderBy" && mi.GetParameters().Length == 2).ToArray()[0];
    private static readonly MethodInfo OrderByDescendingMi = typeof(ParallelEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "OrderByDescending" && mi.GetParameters().Length == 2).ToArray()[0];
    private static readonly MethodInfo OrderByAscendingStringMi = typeof(ParallelEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "OrderBy" && mi.GetParameters().Length == 3).ToArray()[0];
    private static readonly MethodInfo OrderByDescendingStringMi = typeof(ParallelEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "OrderByDescending" && mi.GetParameters().Length == 3).ToArray()[0];
    private static readonly MethodInfo ThenByAscendingMi = typeof(ParallelEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "ThenBy" && mi.GetParameters().Length == 2).ToArray()[0];
    private static readonly MethodInfo ThenByDescendingMi = typeof(ParallelEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "ThenByDescending" && mi.GetParameters().Length == 2).ToArray()[0];
    private static readonly MethodInfo ThenByAscendingStringMi = typeof(ParallelEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "ThenBy" && mi.GetParameters().Length == 3).ToArray()[0];
    private static readonly MethodInfo ThenByDescendingStringMi = typeof(ParallelEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "ThenByDescending" && mi.GetParameters().Length == 3).ToArray()[0];

    private static readonly MethodInfo GroupByMi = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "GroupBy" && mi.GetParameters().Length == 2).ToArray()[0];
    private static readonly MethodInfo GroupByStringMi = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "GroupBy" && mi.GetParameters().Length == 3).ToArray()[0];

    private static readonly MethodInfo xOrderByAscendingMi = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "OrderBy" && mi.GetParameters().Length == 2).ToArray()[0];
    private static readonly MethodInfo xOrderByDescendingMi = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "OrderByDescending" && mi.GetParameters().Length == 2).ToArray()[0];
    private static readonly MethodInfo xOrderByAscendingStringMi = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "OrderBy" && mi.GetParameters().Length == 3).ToArray()[0];
    private static readonly MethodInfo xOrderByDescendingStringMi = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mi => mi.Name == "OrderByDescending" && mi.GetParameters().Length == 3).ToArray()[0];

    private static readonly Dictionary<Guid, Dictionary<string, object>> Cache = new Dictionary<Guid, Dictionary<string, object>>();

    public static void CreateCache(Type T)
    {
      // Кеш немає виграшу у швидкості виконання в порівнянні з постійною генерацією MethodInfos
      var tCache = new Dictionary<string, object>();
      tCache.Add("LoadData", T.GetMethod("LoadData", BindingFlags.Public | BindingFlags.Static));
      tCache.Add("GetData", T.GetMethod("GetData", BindingFlags.Public | BindingFlags.Static));
      var ienumType = typeof(IEnumerable<>).MakeGenericType(T);
      var ocType = typeof(ObservableCollection<>).MakeGenericType(T);
      tCache.Add("ocConstructor", ocType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { ienumType }, null));
      tCache.Add("asParalelMI", AsParalelMi.MakeGenericMethod(T));

      var gettersFI = T.GetField("Getters", BindingFlags.Public | BindingFlags.Static);
      var getters = (Dictionary<string, object>)gettersFI.GetValue(null);
      tCache.Add("getters", gettersFI.GetValue(null));

      var orderByMethods = new Dictionary<string, Dictionary<ListSortDirection, MethodInfo>>();
      var thenByMethods = new Dictionary<string, Dictionary<ListSortDirection, MethodInfo>>();
      var tempOrderByMethods = new Dictionary<Guid, Dictionary<ListSortDirection, MethodInfo>>();
      var tempThenByMethods = new Dictionary<Guid, Dictionary<ListSortDirection, MethodInfo>>();
      foreach (var getter in getters)
      {
        var returnType = getter.Value.GetType().GenericTypeArguments[1];

        if (!tempOrderByMethods.ContainsKey(returnType.GUID))
        {
          tempOrderByMethods.Add(returnType.GUID, new Dictionary<ListSortDirection, MethodInfo>
          {
            {
              ListSortDirection.Ascending, returnType == typeof(string)
                ? OrderByAscendingStringMi.MakeGenericMethod(T, returnType)
                : OrderByAscendingMi.MakeGenericMethod(T, returnType)
            },
            {
              ListSortDirection.Descending, returnType == typeof(string)
                ? OrderByDescendingStringMi.MakeGenericMethod(T, returnType)
                : OrderByDescendingMi.MakeGenericMethod(T, returnType)
            }
          });
          tempThenByMethods.Add(returnType.GUID, new Dictionary<ListSortDirection, MethodInfo>
          {
            {
              ListSortDirection.Ascending, returnType == typeof(string)
                ? ThenByAscendingStringMi.MakeGenericMethod(T, returnType)
                : ThenByAscendingMi.MakeGenericMethod(T, returnType)
            },
            {
              ListSortDirection.Descending, returnType == typeof(string)
                ? ThenByDescendingStringMi.MakeGenericMethod(T, returnType)
                : ThenByDescendingMi.MakeGenericMethod(T, returnType)
            }
          });
        }

        orderByMethods.Add(getter.Key, tempOrderByMethods[returnType.GUID]);
        thenByMethods.Add(getter.Key, tempThenByMethods[returnType.GUID]);
      }
      tCache.Add("orderByMethods", orderByMethods);
      tCache.Add("thenByMethods", thenByMethods);

      Cache.Add(T.GUID, tCache);
    }

    public static void LoadData(Type T)
    {
      if (!Cache.ContainsKey(T.GUID))
        CreateCache(T);

      ((MethodInfo)Cache[T.GUID]["LoadData"]).Invoke(null, null);
    }

    public static IEnumerable GetData(Type T, List<(string, ListSortDirection)> sortList)
    {
      if (!Cache.ContainsKey(T.GUID))
        CreateCache(T);

      var tCache = Cache[T.GUID];
      var data = ((MethodInfo)tCache["GetData"]).Invoke(null, null);

      if (sortList == null || sortList.Count == 0)
        return (IEnumerable)((ConstructorInfo)tCache["ocConstructor"]).Invoke(new[] { data });

      var data1 = ((MethodInfo)tCache["asParalelMI"]).Invoke(null, new[] { data });

      var getters = (Dictionary<string, object>)tCache["getters"];
      var getter = getters[sortList[0].Item1];
      var orderByMI = ((Dictionary<string, Dictionary<ListSortDirection, MethodInfo>>) tCache["orderByMethods"])[sortList[0].Item1][sortList[0].Item2];
      var data2 = orderByMI.Invoke(null, orderByMI.GetParameters().Length == 2 ? new[] {data1, getter} : new[] { data1, getter, StringComparer.Ordinal });

      for (var k = 1; k < sortList.Count; k++)
      {
        getter = getters[sortList[k].Item1];
        var thenByMI = ((Dictionary<string, Dictionary<ListSortDirection, MethodInfo>>)tCache["thenByMethods"])[sortList[k].Item1][sortList[k].Item2];
        data2 = thenByMI.Invoke(null, thenByMI.GetParameters().Length == 2 ? new[] { data2, getter } : new[] { data2, getter, StringComparer.Ordinal });
      }

      return (IEnumerable)((ConstructorInfo)tCache["ocConstructor"]).Invoke(new[] { data2 });
    }

    public static IEnumerable GetDataNoCache(Type T, List<(string, ListSortDirection)> sortList)
    {
      var getDataMI = T.GetMethod("GetData", BindingFlags.Public | BindingFlags.Static);
      var ienumType = typeof(IEnumerable<>).MakeGenericType(T);
      var ocType = typeof(ObservableCollection<>).MakeGenericType(T);
      var ocType1 = typeof(List<>).MakeGenericType(T);
      // var ocConstructor = ocType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { ienumType }, null);
      var ocConstructor = ocType1.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { ienumType }, null);
      var data = getDataMI.Invoke(null, null);

      if (sortList == null || sortList.Count == 0)
      {
        // return (IEnumerable)ocConstructor.Invoke(new[] { data });
        //var a1 = new List<object>((IEnumerable<object>) data);
        // return new BindingList<object>(a1);
        // var a1 = ((IEnumerable<GlDocLine>)data).ToList();
        // var a1 = new BindingList<object>(((IEnumerable<GlDocLine>)data).Cast<object>().ToList());
        // var a1 = new BindingList<GlDocLine>(((IEnumerable<GlDocLine>)data).ToList());
        var a1 = ((IEnumerable<GlDocLine>)data).ToList();
        // var a1 = new ObservableCollection<GlDocLine>((IEnumerable<GlDocLine>)data);
        return a1;
        // return new List<object>(((IEnumerable<GlDocLine>)data).Cast<object>().ToList());
      }

      var asParalelMI = AsParalelMi.MakeGenericMethod(T);
      var data1 = asParalelMI.Invoke(null, new[] { data });

      var gettersFI = T.GetField("Getters", BindingFlags.Public | BindingFlags.Static);
      var getters = (Dictionary<string, object>)gettersFI.GetValue(null);
      var getter = getters[sortList[0].Item1];
      var returnType = getter.GetType().GenericTypeArguments[1];

      object data2;
      if (returnType == typeof(string))
        data2 = (sortList[0].Item2 == ListSortDirection.Ascending ? OrderByAscendingStringMi : OrderByDescendingStringMi).MakeGenericMethod(T, returnType).Invoke(null, new[] { data1, getter, StringComparer.Ordinal });
      else
        data2 = (sortList[0].Item2 == ListSortDirection.Ascending ? OrderByAscendingMi : OrderByDescendingMi).MakeGenericMethod(T, returnType).Invoke(null, new[] { data1, getter });

      for (var k = 1; k < sortList.Count; k++)
      {
        getter = getters[sortList[k].Item1];
        returnType = getter.GetType().GenericTypeArguments[1];
        if (returnType == typeof(string))
          data2 = (sortList[k].Item2 == ListSortDirection.Ascending ? ThenByAscendingStringMi : ThenByDescendingStringMi).MakeGenericMethod(T, returnType).Invoke(null, new[] { data2, getter, StringComparer.Ordinal });
        else
          data2 = (sortList[k].Item2 == ListSortDirection.Ascending ? ThenByAscendingMi : ThenByDescendingMi).MakeGenericMethod(T, returnType).Invoke(null, new[] { data2, getter });
      }

      // return (IEnumerable)ocConstructor.Invoke(new[] { data2 });
      // return new BindingList<object>(new[] { data2 });
      // var a11 = new BindingList<object>(((IEnumerable<GlDocLine>)data2).Cast<object>().ToList());
      // var a11 = new BindingList<GlDocLine>(((IEnumerable<GlDocLine>)data2).ToList());
      // var a11 = ((IEnumerable<GlDocLine>)data2).ToList();
      // var a11 = new ObservableCollection<GlDocLine>((IEnumerable<GlDocLine>)data2);
      var a11 = (IEnumerable<GlDocLine>)data2;
      return a11;
      // return new List<object>(((IEnumerable<GlDocLine>)data2).Cast<object>().ToList());
    }

    public static IEnumerable GetDataGroupingSortNoCache(Type T, List<(string, ListSortDirection)> sortList)
    {
      var getDataMI = T.GetMethod("GetData", BindingFlags.Public | BindingFlags.Static);
      var ienumType = typeof(IEnumerable<>).MakeGenericType(T);
      var ocType = typeof(ObservableCollection<>).MakeGenericType(T);
      var ocType1 = typeof(List<>).MakeGenericType(T);
      // var ocConstructor = ocType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { ienumType }, null);
      var ocConstructor = ocType1.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { ienumType }, null);
      var data = getDataMI.Invoke(null, null);

      if (sortList == null || sortList.Count == 0)
      {
        // return (IEnumerable)ocConstructor.Invoke(new[] { data });
        var a1 = (IEnumerable<GlDocLine>)data;
        // var a1 = new ObservableCollection<GlDocLine>((IEnumerable<GlDocLine>)data);
        return a1;
      }

      var asParalelMI = AsParalelMi.MakeGenericMethod(T);
      var data1 = asParalelMI.Invoke(null, new[] { data });

      var gettersFI = T.GetField("Getters", BindingFlags.Public | BindingFlags.Static);
      var getters = (Dictionary<string, object>)gettersFI.GetValue(null);
      var getter = getters[sortList[0].Item1];
      var returnType = getter.GetType().GenericTypeArguments[1];

      var groupByMI = GroupByMi.MakeGenericMethod(T, returnType);
      var groupByStringMI = GroupByStringMi.MakeGenericMethod(T, returnType);

      object data2;
      if (returnType == typeof(string))
      {
        data2 = groupByStringMI.Invoke(null, new[] {data1, getter, StringComparer.Ordinal});
        data2 =
          (sortList[0].Item2 == ListSortDirection.Ascending ? OrderByAscendingStringMi : OrderByDescendingStringMi)
          .MakeGenericMethod(T, returnType).Invoke(null, new[] {data1, getter, StringComparer.Ordinal});
      }
      else
      {
        /*data2 = groupByMI.Invoke(null, new[] { data1, getter});
        var t2 = typeof(IGrouping<,>).MakeGenericType(returnType, T);

        var groupFI = typeof(ItemHelper).GetMethod("GroupFunc", BindingFlags.Static | BindingFlags.NonPublic);
        var groupFI2 = groupFI.MakeGenericMethod(T, returnType);
        // var Func<IGrouping<TKeyType, TItem>, TKeyType> func = delegate (IGrouping<TKeyType, TItem> item) { return item.Key; };

        data2 = (sortList[0].Item2 == ListSortDirection.Ascending ? xOrderByAscendingMi : xOrderByDescendingMi)
          .MakeGenericMethod(t2, returnType).Invoke(null, new[] {data2, groupFI2});*/
        var orderByGroupFI = typeof(ItemHelper).GetMethod( sortList[0].Item2 == ListSortDirection.Ascending ? "OrderByGroup" : "OrderByDescendingGroup", BindingFlags.Static | BindingFlags.NonPublic);
        var orderByGroupFI2 = orderByGroupFI.MakeGenericMethod(T, returnType);
        data2 = orderByGroupFI2.Invoke(null, new[] { data, getter });
        var data3 = orderByGroupFI2.Invoke(null, new[] { data, getter });
      }

      /*for (var k = 1; k < sortList.Count; k++)
      {
        getter = getters[sortList[k].Item1];
        returnType = getter.GetType().GenericTypeArguments[1];
        if (returnType == typeof(string))
          data2 = (sortList[k].Item2 == ListSortDirection.Ascending ? ThenByAscendingStringMi : ThenByDescendingStringMi).MakeGenericMethod(T, returnType).Invoke(null, new[] { data2, getter, StringComparer.Ordinal });
        else
          data2 = (sortList[k].Item2 == ListSortDirection.Ascending ? ThenByAscendingMi : ThenByDescendingMi).MakeGenericMethod(T, returnType).Invoke(null, new[] { data2, getter });
      }*/

      // return (IEnumerable)ocConstructor.Invoke(new[] { data2 });
      // return new BindingList<object>(new[] { data2 });
      // var a11 = new BindingList<object>(((IEnumerable<GlDocLine>)data2).Cast<object>().ToList());
      // var a11 = new BindingList<GlDocLine>(((IEnumerable<GlDocLine>)data2).ToList());
      // var a11 = ((IEnumerable<GlDocLine>)data2).ToList();
      // var a11 = new ObservableCollection<GlDocLine>((IEnumerable<GlDocLine>)data2);
      var a11 = (IEnumerable<GlDocLine>)data2;
      return a11;
      // return new List<object>(((IEnumerable<GlDocLine>)data2).Cast<object>().ToList());
    }

    // private static TKeyType GroupFunc<TItem, TKeyType>(IGrouping<TKeyType, TItem> item) => item.Key;
    private static IEnumerable<TItem> OrderByGroup<TItem, TKeyType>(IEnumerable<TItem> items, Func<TItem, TKeyType> getter)
    {
      var aa1 = items.GroupBy(getter).OrderBy(item => item.Key);
      foreach (var a1 in aa1)
      foreach (var a2 in a1)
        yield return a2;
    }

    // private static IOrderedEnumerable<IGrouping<TKeyType, TItem>> OrderByDescendingGroup<TItem, TKeyType>(IEnumerable<TItem> items, Func<TItem, TKeyType> getter)
    private static IEnumerable<TItem> OrderByDescendingGroup<TItem, TKeyType>(IEnumerable<TItem> items, Func<TItem, TKeyType> getter)
    {
      var aa1 = items.GroupBy(getter).OrderByDescending(item => item.Key);
      foreach (var a1 in aa1)
      foreach (var a2 in a1)
        yield return a2;
    }
  }

}
