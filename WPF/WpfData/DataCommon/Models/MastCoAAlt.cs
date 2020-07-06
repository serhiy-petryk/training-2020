using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace DataCommon.Models
{
  public class MastCoAAlt
  {
    private const string sql = "SELECT * from [Mast_CoA_ALT]";
    private static CultureInfo ci = CultureInfo.InvariantCulture;
    public static Dictionary<string, MastCoAAlt> Data;
    private static string _status = null;

    public static void Init()
    {
      _status = "init";
      Data = new Dictionary<string, MastCoAAlt>();
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        cmd.CommandText = sql;
        conn.Open();
        using (var rdr = cmd.ExecuteReader())
        {
          _status = "loading";
          var oo = new object[rdr.FieldCount];
          while (rdr.Read())
          {
            rdr.GetValues(oo);
            var o = new MastCoAAlt(oo);
            Data.Add(o.ALTACC, o);
          }
        }
      }
      _status = "finished";
    }

    public static (string, int) GetStatus() => (_status, Data?.Count ?? 0);
    public static void Clear() => Data = null;

    //==========   old   ===========
    public static void RefreshData()
    {
      Data = new Dictionary<string, MastCoAAlt>();
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
            var o = new MastCoAAlt(oo);
            Data.Add(o.ALTACC, o);
          }
        }
      }
    }


    //==================================================
    public string ALTACC { get; set; }
    public string SHORTNAME { get; set; }
    public string LONGNAME { get; set; }
    public DateTime? UPDATED { get; set; }
    public MastCoAAlt(object[] oo)
    {
      ALTACC = (string)oo[0];
      SHORTNAME = oo[1] == DBNull.Value ? null : (string)oo[1];
      LONGNAME = oo[2] == DBNull.Value ? null : (string)oo[2];
      UPDATED = oo[3] == DBNull.Value ? (DateTime?)null : (DateTime)oo[3];
    }
  }
}
