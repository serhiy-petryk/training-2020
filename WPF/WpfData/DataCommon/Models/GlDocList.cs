using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace DataCommon.Models
{
  public class GlDocList
  {
    // private const string cs = "Data Source=localhost;Initial Catalog=dbOneSAP_DW;Integrated Security=True";
    private const string sql = "SELECT * from [Gldoclist]";
    private static CultureInfo ci = CultureInfo.InvariantCulture;
    public static Dictionary<long, GlDocList> Data;
    private static string _status = null;

    public static void Init()
    {
      _status = "init";
      Data = new Dictionary<long, GlDocList>();
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
            var o = new GlDocList(oo);
            Data.Add(o.DOCKEY, o);
          }
        }
      }
      _status = "finished";
    }

    public static (string, int) GetStatus() => (_status, Data?.Count ?? 0);
    public static void Clear() => Data = null;

    //==========   old   ===========
    public static void RefreshData(int records = 1000000)
    {
      Data = new Dictionary<long, GlDocList>();
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
            Data.Add(o.DOCKEY, o);
          }
        }
      }
    }

    public long DOCKEY { get; set; }
    public string LEDGER { get; set; }
    public string TYPE { get; set; }
    public DateTime POSTED { get; set; }
    public string REFERENCE { get; set; }
    public string REFKEY { get; set; }
    public string REFPROC { get; set; }
    public string DOCHEADER { get; set; }
    public DateTime DOCDATE { get; set; }
    public string STATUS { get; set; }
    public long? STDOC { get; set; }
    public string CURR { get; set; }
    public string USERNAME { get; set; }
    public DateTime CREATED { get; set; }
    public DateTime? CHANGED { get; set; }
    public string TRANCODE { get; set; }
    public string BUSTRAN { get; set; }
    public string DOCTYPE { get; set; }
    public decimal? RATE { get; set; }
    public string FMAREA { get; set; }
    public string SUBSET { get; set; }
    public string DESCRIPTION { get; set; }
    public DateTime? DCREATED { get; set; }
    public string PERIOD { get; set; }
    public byte MYSTORNO { get; set; }
    public long? MYSTDOC { get; set; }
    public bool? ARCHIVE { get; set; }
    public string PARTNER { get; set; }
    public DateTime ORIG_POSTED { get; set; }
    public long? REF_DOCKEY { get; set; }
    public string REF_PARTNER { get; set; }
    public string ORIG_PARTNER { get; set; }
    public string ORIG_PARTNER1 { get; set; }

    public GlDocList(object[] oo)
    {
      DOCKEY = (long)oo[0];
      LEDGER = oo[1] == DBNull.Value ? null : (string)oo[1];
      TYPE = (string)oo[2];
      POSTED = (DateTime)oo[3];
      REFERENCE = oo[4] == DBNull.Value ? null : (string)oo[4];
      REFKEY = oo[5] == DBNull.Value ? null : (string)oo[5];
      REFPROC = oo[6] == DBNull.Value ? null : (string)oo[6];
      DOCHEADER = oo[7] == DBNull.Value ? null : (string)oo[7];
      DOCDATE = (DateTime)oo[8];
      STATUS = oo[9] == DBNull.Value ? null : (string)oo[9];
      STDOC = oo[10] == DBNull.Value ? (long?)null : (long)oo[10];
      CURR = oo[11] == DBNull.Value ? null : (string)oo[11];
      USERNAME = oo[12] == DBNull.Value ? null : (string)oo[12];
      CREATED = (DateTime)oo[13];
      CHANGED = oo[14] == DBNull.Value ? (DateTime?)null : (DateTime)oo[14];
      TRANCODE = oo[15] == DBNull.Value ? null : (string)oo[15];
      BUSTRAN = oo[16] == DBNull.Value ? null : (string)oo[16];
      DOCTYPE = oo[17] == DBNull.Value ? null : (string)oo[17];
      RATE = oo[18] == DBNull.Value ? (decimal?)null : (decimal)oo[18];
      FMAREA = oo[19] == DBNull.Value ? null : (string)oo[19];
      SUBSET = oo[20] == DBNull.Value ? null : (string)oo[20];
      DESCRIPTION = oo[21] == DBNull.Value ? null : (string)oo[21];
      DCREATED = oo[22] == DBNull.Value ? (DateTime?)null : (DateTime)oo[22];
      PERIOD = oo[23] == DBNull.Value ? null : (string)oo[23];
    }
  }
}
