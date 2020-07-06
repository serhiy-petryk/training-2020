using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace DataCommon.Models
{
  public class MastCoA
  {
    private const string sql = "SELECT * from [Mast_CoA]";
    private static CultureInfo ci = CultureInfo.InvariantCulture;
    public static Dictionary<string, MastCoA> Data;
    private static string _status = null;

    public static void Init()
    {
      _status = "init";
      Data = new Dictionary<string, MastCoA>();
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
            var o = new MastCoA(oo);
            Data.Add(o.ACCOUNT, o);
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
      if (MastCoAAlt.Data == null) 
        MastCoAAlt.RefreshData();

      Data = new Dictionary<string, MastCoA>();
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        // cmd.CommandText = "SELECT * from [Mast_CoA] where account like N'1%'";
        cmd.CommandText = sql;
        conn.Open();
        using (var rdr = cmd.ExecuteReader())
        {
          var oo = new object[rdr.FieldCount];
          while (rdr.Read())
          {
            rdr.GetValues(oo);
            var o = new MastCoA(oo);
            Data.Add(o.ACCOUNT, o);
          }
        }
      }
    }


    //==================================================
    public MastCoAAlt oALTACC => MastCoAAlt.Data.ContainsKey(ALTACC) ? MastCoAAlt.Data[ALTACC] : null;
    public MastCoAAlt ooALTACC ;
    public MastCoAAlt oALTACC2 => MastCoAAlt.Data[ALTACC];
    public string ACCOUNT { get; set; }
    public string ALTACC { get; set; }
    public string SHORT_TEXT { get; set; }
    public string LONG_TEXT { get; set; }
    public string ACCOUNT_GROUP { get; set; }
    public bool? BALANCE_SHEET { get; set; }
    public bool? LINE_ITEMS_DISPLAY { get; set; }
    public string TAX { get; set; }
    public string CURR { get; set; }
    public string SORT { get; set; }
    public bool? WITHOUT_TAX { get; set; }
    public bool? OPEN_ITEM { get; set; }
    public bool? LC_BALS { get; set; }
    public bool? AT { get; set; }
    public string TRPRT { get; set; }
    public bool? DELFLAG { get; set; }
    public bool? CRTNBLOCK { get; set; }
    public bool? PSTGBLOCK { get; set; }
    public bool? PSTGBLOCK1 { get; set; }
    public bool? PLANBLOCK { get; set; }
    public string FUNCAREA { get; set; }
    public string LEVEL { get; set; }
    public string FBI { get; set; }
    public string FSTGROUP { get; set; }
    public string RECONID { get; set; }
    public bool? C { get; set; }
    public bool? AUTOPOSTING { get; set; }
    public bool? DELETIONFLAG { get; set; }
    public string INTCAL { get; set; }
    public DateTime? DATED { get; set; }
    public string CREATEDBY { get; set; }
    public DateTime? DATED1 { get; set; }
    public string CREATEDBY1 { get; set; }
    public DateTime? UPDATED { get; set; }


    public MastCoA(object[] oo)
    {
      ACCOUNT = (string)oo[0];
      ALTACC = oo[1] == DBNull.Value ? null : (string)oo[1];
      SHORT_TEXT = oo[2] == DBNull.Value ? null : (string)oo[2];
      LONG_TEXT = oo[3] == DBNull.Value ? null : (string)oo[3];
      ACCOUNT_GROUP = oo[4] == DBNull.Value ? null : (string)oo[4];
      BALANCE_SHEET = oo[5] == DBNull.Value ? (bool?)null : (bool)oo[5];
      LINE_ITEMS_DISPLAY = oo[6] == DBNull.Value ? (bool?)null : (bool)oo[6];
      TAX = oo[7] == DBNull.Value ? null : (string)oo[7];
      CURR = oo[8] == DBNull.Value ? null : (string)oo[8];
      SORT = oo[9] == DBNull.Value ? null : (string)oo[9];
      WITHOUT_TAX = oo[10] == DBNull.Value ? (bool?)null : (bool)oo[10];
      OPEN_ITEM = oo[11] == DBNull.Value ? (bool?)null : (bool)oo[11];
      LC_BALS = oo[12] == DBNull.Value ? (bool?)null : (bool)oo[12];
      AT = oo[13] == DBNull.Value ? (bool?)null : (bool)oo[13];
      TRPRT = oo[14] == DBNull.Value ? null : (string)oo[14];
      DELFLAG = oo[15] == DBNull.Value ? (bool?)null : (bool)oo[15];
      CRTNBLOCK = oo[16] == DBNull.Value ? (bool?)null : (bool)oo[16];
      PSTGBLOCK = oo[17] == DBNull.Value ? (bool?)null : (bool)oo[17];
      PSTGBLOCK1 = oo[18] == DBNull.Value ? (bool?)null : (bool)oo[18];
      PLANBLOCK = oo[19] == DBNull.Value ? (bool?)null : (bool)oo[19];
      FUNCAREA = oo[20] == DBNull.Value ? null : (string)oo[20];
      LEVEL = oo[21] == DBNull.Value ? null : (string)oo[21];
      FBI = oo[22] == DBNull.Value ? null : (string)oo[22];
      FSTGROUP = oo[23] == DBNull.Value ? null : (string)oo[23];
      RECONID = oo[24] == DBNull.Value ? null : (string)oo[24];
      C = oo[25] == DBNull.Value ? (bool?)null : (bool)oo[25];
      AUTOPOSTING = oo[26] == DBNull.Value ? (bool?)null : (bool)oo[26];
      DELETIONFLAG = oo[27] == DBNull.Value ? (bool?)null : (bool)oo[27];
      INTCAL = oo[28] == DBNull.Value ? null : (string)oo[28];
      DATED = oo[29] == DBNull.Value ? (DateTime?)null : (DateTime)oo[29];
      CREATEDBY = oo[30] == DBNull.Value ? null : (string)oo[30];
      DATED1 = oo[31] == DBNull.Value ? (DateTime?)null : (DateTime)oo[31];
      CREATEDBY1 = oo[32] == DBNull.Value ? null : (string)oo[32];
      UPDATED = oo[33] == DBNull.Value ? (DateTime?)null : (DateTime)oo[33];
    }
  }
}
