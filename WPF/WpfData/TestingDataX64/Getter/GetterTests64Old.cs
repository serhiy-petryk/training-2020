using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using DataCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestingDataX64.Getter
{
  [TestClass]
  public class GetterTests64Old
  {
    private static int recs = 3000000;

    [TestMethod]
    public void TestMethod1()
    {
      var aa1 = new List<double>();
      var r1 = Common.MemoryUsedInKB;
      DataCommon.Models.GlDocLineOld.RefreshData(recs);
      // DataCommon.Models.GlDocLine.RefreshDataParallel(recs);
      var r2 = Common.MemoryUsedInKB;

      // Func<GlDocLine, string> fn = x => x.oACCOUNT?.oALTACC?.LONGNAME;
      // Expression<Func<GlDocLine, string>> fn1 = x => x.oACCOUNT.oALTACC.LONGNAME;

      // var d1 = GlDocLine.Data.Values.Where(x => x.oACCOUNT != null).ToList();

      var sw = new Stopwatch();
      sw.Start();
      var x22 = new ObservableCollection<GlDocLineOld>(DataCommon.Models.GlDocLineOld.Data.Values.OrderByDescending(x => x.oACCOUNT2?.DATED));
      sw.Stop();
      aa1.Add(sw.Elapsed.TotalMilliseconds);
      x22 = null;

      sw = new Stopwatch();
      sw.Start();
      var x2 = new ObservableCollection<GlDocLineOld>(DataCommon.Models.GlDocLineOld.Data.Values.AsParallel().OrderByDescending(x => x.oACCOUNT2?.DATED));
      sw.Stop();
      aa1.Add(sw.Elapsed.TotalMilliseconds);
      x2 = null;

      for (var i = 1; i <= 16; i++)
      {
        sw = new Stopwatch();
        sw.Start();
        var x21 = new ObservableCollection<GlDocLineOld>(DataCommon.Models.GlDocLineOld.Data.Values.AsParallel().WithDegreeOfParallelism(i).OrderByDescending(x => x.oACCOUNT2?.DATED));
        sw.Stop();
        aa1.Add(sw.Elapsed.TotalMilliseconds);
      }

      var r3 = Common.MemoryUsedInKB;
      var r32 = r3 - r2;
      var r21 = r2 - r1;
    }
  }
}
