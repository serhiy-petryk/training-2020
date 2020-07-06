using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCommon.Models
{
  public interface IMasterHeader<T>
  {
    List<Type> DependedTypes { get; }
    Dictionary<string, object> Getters { get; }
    IEnumerable<T> GetData(List<(string, ListSortDirection)> sortList);
  }
  public class MasterHeader<T>
  {
    /*public IEnumerable<T> GetData(List<(string, ListSortDirection)> sortList)
    {
      if (GlDocLine.GetData() == null)
        RefreshData();

      if (sortList == null || sortList.Count == 0)
        return GlDocLine.GetData();

      var data = GlDocLine.GetData();
      IOrderedEnumerable<GlDocLine> data1 = null;
      try
      {
        data1 = sortList[0].Item2 == ListSortDirection.Ascending
          ? data.OrderBy((Func<GlDocLine, object>)_getters[sortList[0].Item1])
          : data.OrderByDescending((Func<GlDocLine, object>)_getters[sortList[0].Item1]);
      }
      catch (Exception ex)
      {

      }

      for (var k = 1; k < sortList.Count; k++)
        data1 = sortList[k].Item2 == ListSortDirection.Ascending ? data1.ThenBy((Func<GlDocLine, object>)_getters[sortList[k].Item1]) : data1.ThenByDescending((Func<GlDocLine, object>)_getters[sortList[k].Item1]);

      return data1;
    }*/

  }

  public class OGlDocLine: IMasterHeader<GlDocLine>
  {
    private List<Type> _dependedTypes = new List<Type> {typeof(GlDocList), typeof(MastCoA), typeof(MastCoAAlt)};
    private Dictionary<string, object> _getters = new Dictionary<string, object>{{"DOCKEY", (Func<GlDocLine, object>) (item => (object)item.DOCKEY) }};
    public List<Type> DependedTypes => _dependedTypes;
    public Dictionary<string, object> Getters => _getters;
    public IEnumerable<GlDocLine> GetData(List<(string, ListSortDirection)> sortList)
    {
      if (GlDocLine.GetData() == null)
        RefreshData();

      if (sortList == null || sortList.Count == 0)
        return GlDocLine.GetData();

      var data = GlDocLine.GetData();
      IOrderedEnumerable<GlDocLine> data1 = null;
      try
      {
        data1 = sortList[0].Item2 == ListSortDirection.Ascending
          ? data.OrderBy((Func<GlDocLine, object>) _getters[sortList[0].Item1])
          : data.OrderByDescending((Func<GlDocLine, object>) _getters[sortList[0].Item1]);
      }
      catch (Exception ex)
      {

      }

      for (var k=1; k<sortList.Count; k++)
        data1 = sortList[k].Item2 == ListSortDirection.Ascending ? data1.ThenBy((Func<GlDocLine, object>)_getters[sortList[k].Item1]) : data1.ThenByDescending((Func<GlDocLine, object>)_getters[sortList[k].Item1]);

      return data1;
    }

    private void RefreshData()
    {
      var tasks = new List<Task>
      {
        Task.Factory.StartNew(MastCoAAlt.Init),
        Task.Factory.StartNew(MastCoA.Init),
        Task.Factory.StartNew(GlDocList.Init),
        Task.Factory.StartNew(GlDocLine.Init)
      };

      Task.WaitAll(tasks.ToArray());
      // await Task.WhenAll(tasks.ToArray());
    }
  }
}
