using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using Common = First.Model.Common;
using GlDocLine = First.Model.GlDocLine;
using GlDocList = First.Model.GlDocList;

namespace First.Utils
{
  public class Data
  {
    public static IEnumerable<GlDocLine> GetGlDocLine(int records = 100)
    {
      Common.ClearSqlCache();
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      sw.Start();

      var data = new Dictionary<int, GlDocLine>();
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        cmd.CommandText = string.Format(Common.sqlGLDoclineWithRecs, records);
        conn.Open();
        using (var rdr = cmd.ExecuteReader())
        {
          var oo = new object[rdr.FieldCount];
          while (rdr.Read())
          {
            rdr.GetValues(oo);
            var o = new GlDocLine(oo);
            data.Add(o._ID_, o);
          }
        }
      }

      sw.Stop();
      var r2 = Common.MemoryUsedInKB;
      var d1 = sw.Elapsed.TotalMilliseconds;
      var r = r2 - r1;
      MainWindow.LogData.Add("GetGlDocLine: " + $"{r2:n0}");
      MainWindowSort.LogData.Add("GetGlDocLine: " + $"{r2:n0}");
      return data.Values;
    }

    public static IEnumerable<GlDocList> GetGlDocList(int records = 100)
    {
      Common.ClearSqlCache();
      var r1 = Common.MemoryUsedInKB;
      var sw = new Stopwatch();
      sw.Start();

      var data = new Dictionary<long, GlDocList>();
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        cmd.CommandText = string.Format(Common.sqlGLDoclistWithRecs, records);
        conn.Open();
        using (var rdr = cmd.ExecuteReader())
        {
          var oo = new object[rdr.FieldCount];
          while (rdr.Read())
          {
            rdr.GetValues(oo);
            var o = new GlDocList(oo);
            data.Add(o.DOCKEY, o);
          }
        }
      }

      sw.Stop();
      var r2 = Common.MemoryUsedInKB;
      var d1 = sw.Elapsed.TotalMilliseconds;
      var r = r2 - r1;
      MainWindow.LogData.Add("GetGlDocList: " + $"{r2:n0}");
      MainWindowSort.LogData.Add("GetGlDocList: " + $"{r2:n0}");
      return data.Values;
    }
  }
}
