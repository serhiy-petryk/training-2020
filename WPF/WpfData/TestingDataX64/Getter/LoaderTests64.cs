using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
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
  public class LoaderTests64
  {
    private const string sql = "SELECT top 1000000 * from [GlDocLine]";

    [TestMethod]
    public void ClearSqlCache()
    {
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        conn.Open();
        cmd.CommandText = "CHECKPOINT";
        cmd.ExecuteNonQuery();
        cmd.CommandText = "DBCC DROPCLEANBUFFERS";
        cmd.ExecuteNonQuery();
      }
    }

    [TestMethod]
    public void SqlSimplestWithRefTables()
    {
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();

      GlDocLine.InitParallel();
      sw.Start();
      var tasks = new List<Task>
      {
        Task.Factory.StartNew(MastCoAAlt.Init),
        Task.Factory.StartNew(MastCoA.Init),
        // Task.Factory.StartNew(GlDocList.Init),
        Task.Factory.StartNew(GlDocLine.Init)
      };

      Task.WaitAll(tasks.ToArray());

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;

      var r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
      var data = GlDocLine.GetData();
    }

    [TestMethod]
    public void SqlSimplest()
    {
      const int threads = 3;
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      var aa1 = new List<double>();

      GlDocLine.InitParallel();
      sw.Start();

      GlDocLine.Init();

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;

      var r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
      var data = GlDocLine.GetData();
    }

    [TestMethod]
    public void SqlParallel8()
    {
      const int threads = 3;
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      var aa1 = new List<double>();

      GlDocLine.InitParallel();
      sw.Start();
      // var tasks = new List<Task>();
      var tasks = new List<Task>
      {
        Task.Factory.StartNew(MastCoAAlt.Init),
        Task.Factory.StartNew(MastCoA.Init),
        Task.Factory.StartNew(GlDocList.Init),
        // Task.Factory.StartNew(() => GlDocLine.InitParallel(4, 0)),
        // Task.Factory.StartNew(() => GlDocLine.InitParallel(4, 1)),
        // Task.Factory.StartNew(() => GlDocLine.InitParallel(4, 2)),
        // Task.Factory.StartNew(() => GlDocLine.InitParallel(4, 3))
      };

      for (var k2 = 0; k2 < threads; k2++)
      {
        var k11 = threads;
        var k21 = k2;
        tasks.Add(Task.Factory.StartNew(() => GlDocLine.LoadParallel(k11, k21)));
      }

      Task.WaitAll(tasks.ToArray());
      // await Task.WhenAll(tasks.ToArray());

      sw.Stop();
      aa1.Add(sw.Elapsed.TotalMilliseconds);
      sw.Reset();

      var r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
      var data = GlDocLine.GetData();
    }

    [TestMethod]
    public void SqlParallelWithRefTables()
    {
      var r1 = Common.MemoryUsedInKB;
      var r2 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      var aa1 = new List<double>();

      for (var k1 = 1; k1 < 16; k1++)
      {
        GlDocLine.InitParallel();
        sw.Start();
        // var tasks = new List<Task>();
        var tasks = new List<Task>
        {
          Task.Factory.StartNew(MastCoAAlt.Init),
          Task.Factory.StartNew(MastCoA.Init),
          Task.Factory.StartNew(GlDocList.Init),
        };

        for (var k2 = 0; k2 < k1; k2++)
        {
          var k11 = k1;
          var k21 = k2;
          tasks.Add(Task.Factory.StartNew(() => GlDocLine.LoadParallel(k11, k21)));
        }

        Task.WaitAll(tasks.ToArray());

        sw.Stop();
        aa1.Add(sw.Elapsed.TotalMilliseconds);
        r2 = Common.MemoryUsedInKB;
        sw.Reset();
      }

      r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
      var data = GlDocLine.GetData();
    }

    [TestMethod]
    public void SqlParallel()
    {
      var r1 = Common.MemoryUsedInKB;
      var r2 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      var aa1 = new List<double>();

      for (var k1 = 1; k1 < 16; k1++)
      {
        GlDocLine.InitParallel();
        sw.Start();
        var tasks = new List<Task>();
        for (var k2 = 0; k2 < k1; k2++)
        {
          var k11 = k1;
          var k21 = k2;
          tasks.Add(Task.Factory.StartNew(() => GlDocLine.LoadParallel(k11, k21)));
        }

        Task.WaitAll(tasks.ToArray());

        sw.Stop();
        aa1.Add(sw.Elapsed.TotalMilliseconds);
        r2 = Common.MemoryUsedInKB;
        sw.Reset();
      }

      r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
      var data = GlDocLine.GetData();
    }

    //=============================
    [TestMethod]
    public void Simplest()
    {
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      sw.Start();

      var Data = new List<GlDocLine>();
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        cmd.CommandText = sql;
        conn.Open();
        using (var rdr = cmd.ExecuteReader())
        {
          var oo = new object[rdr.FieldCount];
          while (rdr.Read())
          {
            rdr.GetValues(oo);
            Data.Add(new GlDocLine(oo));
          }
        }
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void OnlySqlRead()
    {
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      sw.Start();

      var Data = new List<GlDocLine>();
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        cmd.CommandText = sql;
        conn.Open();
        using (var rdr = cmd.ExecuteReader())
        {
          var oo = new object[rdr.FieldCount];
          while (rdr.Read())
          {
            // rdr.GetValues(oo);
            // Data.Add(new GlDocLine(oo));
          }
        }
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
    }

    //=================================
    [TestMethod]
    public void Simple()
    {
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      sw.Start();
      DataCommon.Models.GlDocLine.Init();
      DataCommon.Models.GlDocList.Init();
      DataCommon.Models.MastCoA.Init();
      DataCommon.Models.MastCoAAlt.Init();
      // DataCommon.Models.GlDocLine.RefreshDataParallel(recs);
      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void ParallelWithCancel()
    {
      var r1 = Common.MemoryUsedInKB;

      var cts = new CancellationTokenSource();
      /*var timer = new System.Timers.Timer() { Interval = 30000 };
      timer.Elapsed += (sender, args) =>
      {
        cts.Cancel();
        // var lk = 0;
      };
      timer.Start();*/

      var sw = new Stopwatch();
      sw.Start();

      var tasks = new List<Task>
      {
        Task.Factory.StartNew(MastCoAAlt.Init),
        Task.Factory.StartNew(MastCoA.Init),
        Task.Factory.StartNew(GlDocList.Init),
        Task.Factory.StartNew(GlDocLine.Init)
      };

      // cts.CancelAfter(1000);

      var result = "";

      Task.Factory.StartNew(() =>
      {
        System.Threading.Thread.Sleep(100);
        cts.Cancel();
      });

      try
      {
        Task.WaitAll(tasks.ToArray(), cts.Token);
        // Task.WaitAll(tasks.ToArray());
      }
      catch (OperationCanceledException)
      {
        result += "\r\nDownload canceled.\r\n";
      }
      catch (Exception ex)
      {
        result += "\r\n"+ ex.ToString()+".\r\n";
      }
      // await Task.WhenAll(tasks.ToArray());

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void Parallel3()
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
      // await Task.WhenAll(tasks.ToArray());

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public async Task Parallel()
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

      // Task.WaitAll(tasks.ToArray());
      await Task.WhenAll(tasks.ToArray());

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public async Task Parallel2()
    {
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      sw.Start();

      var tasks = new List<Task>
      {
        Task.Factory.StartNew(GlDocLine.Init),
        Task.Factory.StartNew(GlDocList.Init),
        Task.Factory.StartNew(MastCoA.Init),
        Task.Factory.StartNew(MastCoAAlt.Init)
      };

      // Task.WaitAll(tasks.ToArray());
      await Task.WhenAll(tasks.ToArray());

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r2 = Common.MemoryUsedInKB;
      var r21 = r2 - r1;
    }
  }
}
