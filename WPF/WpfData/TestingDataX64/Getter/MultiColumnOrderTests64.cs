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
  public class MultiColumnOrderTests64
  {
    [TestMethod]
    public void SimpleTest()
    {
      var r1 = Common.MemoryUsedInKB;

      var tasks = new List<Task>
      {
        Task.Factory.StartNew(MastCoAAlt.Init),
        Task.Factory.StartNew(MastCoA.Init),
        Task.Factory.StartNew(GlDocList.Init),
        Task.Factory.StartNew(GlDocLine.Init),
      };

      Task.WaitAll(tasks.ToArray());
      // await Task.WhenAll(tasks.ToArray());

      var sw = new Stopwatch();
      sw.Start();

      // var dd1 = GlDocLine.GetData().OrderBy((item) => item.oDOCKEY.POSTED).ThenBy((item) => item.LINENO).ToList();

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;
    }

    [TestMethod]
    public void ParallelTest()
    {
      var r1 = Common.MemoryUsedInKB;

      var tasks = new List<Task>
      {
        Task.Factory.StartNew(MastCoAAlt.Init),
        Task.Factory.StartNew(MastCoA.Init),
        Task.Factory.StartNew(GlDocList.Init),
        Task.Factory.StartNew(GlDocLine.Init),
      };

      Task.WaitAll(tasks.ToArray());
      // await Task.WhenAll(tasks.ToArray());

      var sw = new Stopwatch();
      sw.Start();

      // var dd1 = GlDocLine.GetData().AsParallel().OrderBy((item) => item.oDOCKEY.POSTED).ThenBy((item) => item.LINENO).ToList();

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;

    }

    [TestMethod]
    public void ParallelTest2()
    {
      var r1 = Common.MemoryUsedInKB;

      var tasks = new List<Task>
      {
        Task.Factory.StartNew(MastCoAAlt.Init),
        Task.Factory.StartNew(MastCoA.Init),
        Task.Factory.StartNew(GlDocList.Init),
        Task.Factory.StartNew(GlDocLine.Init),
      };

      Task.WaitAll(tasks.ToArray());
      // await Task.WhenAll(tasks.ToArray());

      var sw = new Stopwatch();
      sw.Start();

      // var dd1 = GlDocLine.GetData().AsParallel().OrderBy((item) => item.oDOCKEY.POSTED).ThenBy((item) => item.LINENO).AsParallel().ToList();

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
