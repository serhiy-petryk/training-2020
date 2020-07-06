using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DataCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestingDataX64.Getter
{
  [TestClass]
  public class OrderTests64
  {
    public IEnumerable<GlDocLine> GetData()
    {
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      sw.Start();

      var tasks = new List<Task>
      {
        Task.Factory.StartNew(MastCoAAlt.Init),
        Task.Factory.StartNew(MastCoA.Init),
        Task.Factory.StartNew(GlDocList.Init),
        Task.Factory.StartNew(GlDocLine.Init)
      };

      Task.WaitAll(tasks.ToArray());

      foreach (var x in GlDocLine.GetData())
        x.ooACCOUNT = MastCoA.Data[x.ACCOUNT];

      foreach (var x in MastCoA.Data.Values) 
        if (x.ALTACC != null)
          x.ooALTACC = MastCoAAlt.Data[x.ALTACC];

      // await Task.WhenAll(tasks.ToArray());

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;

      return GlDocLine.GetData();
    }

    [TestMethod]
    public void OrderByTest()
    {
      var data = GetData();

      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      sw.Start();
      // var x1 = data.OrderBy((item) => item.DOCKEY).ToArray();
      // var x1 = data.OrderBy((item) => item.oACCOUNT?.oALTACC.LONGNAME, StringComparer.CurrentCultureIgnoreCase).ToArray();
      // var x1 = data.OrderBy((item) => item.oACCOUNT?.oALTACC.LONGNAME, StringComparer.Ordinal).ToArray();
      // var x1 = data.GroupBy((item) => item.DOCKEY).OrderBy((item) => item.Key).ToArray();
      // var x1 = data.GroupBy((item) => item.oACCOUNT?.oALTACC.LONGNAME, StringComparer.Ordinal)
      //.OrderBy((item) => item.Key, StringComparer.Ordinal).ToArray();
      //var x1 = data.GroupBy((item) => Equals(item.oACCOUNT.oALTACC, null)? null : item.oACCOUNT.oALTACC.LONGNAME, StringComparer.Ordinal)
      //.OrderBy((item) => item.Key, StringComparer.Ordinal).ToArray();
      var x1 = data.GroupBy((item) => item.oACCOUNT.oALTACC.LONGNAME)
      .OrderBy((item) => item.Key).ToArray();

      // var x1 = data.GroupBy((item) => item.ooACCOUNT?.ooALTACC.LONGNAME, StringComparer.Ordinal)
      // .OrderBy((item) => item.Key, StringComparer.Ordinal).ToArray();
      //var x1 = data.GroupBy((item) => Equals(item.ooACCOUNT.ooALTACC, null) ? null : item.ooACCOUNT.ooALTACC.LONGNAME)
      //.OrderBy((item) => item.Key).ToArray();
      // var x1 = data.GroupBy((item) => item.ooACCOUNT.ooALTACC.LONGNAME)
        //.OrderBy((item) => item.Key).ToArray();

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;
    }

    [TestMethod]
    public void Simple()
    {
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      sw.Start();

      var oo = new OGlDocLine();
      var data = oo.GetData(new List<(string, ListSortDirection)>{("DOCKEY", ListSortDirection.Descending)});

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;
    }

  }
}
