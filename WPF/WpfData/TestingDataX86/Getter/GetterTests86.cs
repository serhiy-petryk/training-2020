using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using DataCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestingDataX86.Getter
{
  [TestClass]
  public class GetterTests86
  {
    private static int recs = 1000000;

    [TestMethod]
    public void TestMethod1()
    {
      var r1 = Common.MemoryUsedInKB;
      DataCommon.Models.GlDocLineOld.RefreshData(recs);
      var r2 = Common.MemoryUsedInKB;

      // Func<GlDocLine, string> fn = x => x.oACCOUNT?.oALTACC?.LONGNAME;
      // Expression<Func<GlDocLine, string>> fn1 = x => x.oACCOUNT.oALTACC.LONGNAME;

      // var d1 = GlDocLine.Data.Values.Where(x => x.oACCOUNT != null).ToList();

      var sw = new Stopwatch();
      sw.Start();

        // var x1 = DataCommon.Models.GlDocLine.Data.Values.OrderBy(x => x.ACCOUNT);
        // var x2 = new ObservableCollection<GlDocLine>(DataCommon.Models.GlDocLine.Data.Values.OrderBy(x => x.LINENO));
        // var x2 = new ObservableCollection<GlDocLine>(DataCommon.Models.GlDocLine.Data.Values.OrderBy(x => x.ACCOUNT, StringComparer.Ordinal));
        // var x2 = new ObservableCollection<GlDocLine>(DataCommon.Models.GlDocLine.Data.Values.OrderBy(x => x.oACCOUNT?.oALTACC?.LONGNAME, StringComparer.Ordinal));
        // var x2 = new ObservableCollection<GlDocLine>(DataCommon.Models.GlDocLine.Data.Values.OrderBy(fn, StringComparer.Ordinal));
        // var x2 = new ObservableCollection<GlDocLine>(DataCommon.Models.GlDocLine.Data.Values.OrderBy(x => x.oACCOUNT != null && x.oACCOUNT.oALTACC != null ? x.oACCOUNT.oALTACC.LONGNAME : null, StringComparer.Ordinal));
        // var x2 = new ObservableCollection<GlDocLine>(DataCommon.Models.GlDocLine.Data.Values.OrderBy(x => Equals(x.oACCOUNT?.oALTACC, null) ? null : x.oACCOUNT.oALTACC.LONGNAME, StringComparer.Ordinal));
        // var x2 = new ObservableCollection<GlDocLine>(DataCommon.Models.GlDocLine.Data.Values.OrderBy(x => x.oACCOUNT != null ? (x.oACCOUNT.oALTACC != null ? x.oACCOUNT.oALTACC.LONGNAME: null): null, StringComparer.Ordinal));
        // var x2 = new ObservableCollection<GlDocLine>(DataCommon.Models.GlDocLine.Data.Values.OrderBy(x => x.oACCOUNT2.ALTACC, StringComparer.Ordinal));
        var x2 = new ObservableCollection<GlDocLineOld>(DataCommon.Models.GlDocLineOld.Data.Values.OrderBy(x => x.oACCOUNT2.DATED));

      sw.Stop();
      var a2 = sw.Elapsed.TotalMilliseconds;
      var r3 = Common.MemoryUsedInKB;
      var r32 = r3 - r2;
      var r21 = r2 - r1;
    }
  }
}
