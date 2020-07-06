using System;
using System.Data.SqlClient;

namespace DataCommon.Models
{
  public class Common
  {
    public const string csDbOneSap = "Data Source=localhost;Initial Catalog=dbOneSAP_DW;Integrated Security=True";
    // public const string csDbOneSap = "Data Source=localhost;Initial Catalog=dbSAP_DW;Integrated Security=True";

    public const string sqlGLDoclineWithRecs = "SELECT top {0} * from [Gldocline]";
    public const string sqlGLDoclistWithRecs = "SELECT top {0} * from [Gldoclist]";
    public const string sqlGLDocline = "SELECT top 200000 * from [Gldocline]";
    public const string sqlGLDoclist = "SELECT top 200000 * from [Gldoclist]";
    public const string sqlMastCoA = "SELECT * from [MAST_COA]";
    // public const string sqlGLDocline = "SELECT top 20000 * from gldocline";
    public static void ClearSqlCache()
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

    public static long MemoryUsedInKB
    {
      get
      {
        // clear memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        //
        return GC.GetTotalMemory(true) / 1000;
      }
    }
  }
}
