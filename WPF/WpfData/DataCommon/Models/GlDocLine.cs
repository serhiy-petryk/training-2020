using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;

namespace DataCommon.Models
{
  public class GlDocLine
  {
    private const string sql = "SELECT top 100000 * from [GlDocLine]";
    // private const string sql2 = "SELECT top 3000000 *, ROW_NUMBER() OVER(ORDER BY (select 1)) as __rn from [GlDocLine]";
    private static CultureInfo ci = CultureInfo.InvariantCulture;
    // private static List<GlDocLine> Data;
    private static List<GlDocLine> Data;
    // private static ConcurrentBag<GlDocLine> Data;
    private static string _status = null;

    public static readonly Dictionary<string, object> Getters = new Dictionary<string, object>
    {
      { "DOCKEY", (Func<GlDocLine, long>)((item) => item.DOCKEY) },
      { "LINENO", (Func<GlDocLine, short>)((item) => item.LINENO) },
      { "ACCOUNT", (Func<GlDocLine, string>)((item) => item.ACCOUNT) },
      { "ALTACC", (Func<GlDocLine, string>)((item) => item.ALTACC) },
      { "AMT", (Func<GlDocLine, decimal>)((item) => item.AMT) }
    };
    // public static readonly List<Type> DependedTypes = new List<Type> { typeof(GlDocList), typeof(MastCoA), typeof(MastCoAAlt) };
    // public static object GetGetter(string elementName) => _getters[elementName];

    public static IEnumerable<GlDocLine> GetData() => Data;

    public static void Init()
    {
      _status = "init";
      Data = new List<GlDocLine>();
      // Data = new ConcurrentBag<GlDocLine>();
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
            Data.Add(new GlDocLine(oo));
          }
        }
      }
      _status = "finished";
    }

    public static void InitParallel()
    {
      // Data = new ConcurrentBag<GlDocLine>();
      Data = new List<GlDocLine>();
    }
    public static void LoadParallel(int numberOfThread, int thisThreadNo)
    {
      // Data = new ConcurrentBag<GlDocLine>();
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        cmd.CommandText = $"select * from (select *, ROW_NUMBER() OVER(ORDER BY (select 1)) as __rn from ({sql}) a) a1 where __rn % {numberOfThread} = {thisThreadNo};";
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
    }

    public static (string, int) GetStatus() => (_status, Data?.Count ?? 0);
    public static void Clear() => Data = null;

    public static void LoadData()
    {
      var tasks = new List<Task>
      {
        Task.Factory.StartNew(MastCoAAlt.Init),
        Task.Factory.StartNew(MastCoA.Init),
        // Task.Factory.StartNew(GlDocList.Init),
        Task.Factory.StartNew(GlDocLine.Init)
      };

      Task.WaitAll(tasks.ToArray());
      // await Task.WhenAll(tasks.ToArray());
    }

    //=======================================
    public long DOCKEY { get; set; }
    public short LINENO { get; protected set; }
    public string ACCOUNT { get; protected set; }
    public string ALTACC { get; protected set; }
    public decimal DOCAMT { get; protected set; }
    public decimal AMT { get; protected set; }
    public string ASSIGNMENT { get; protected set; }
    public string PROFITCENTER { get; protected set; }
    public string BUSA { get; protected set; }
    public string CC { get; protected set; }
    public string FMAREA { get; set; }
    public string GRANT { get; set; }
    public string FUNCA { get; set; }
    public long? PO { get; set; }
    public short? POITEM { get; set; }
    public long? SO { get; set; }
    public int? SOITEM { get; set; }
    public string LID { get; set; }
    public string MATERIAL { get; set; }
    public bool STORNO { get; set; }
    public long? ORDERNO { get; set; }
    public string PARTNER { get; set; }
    public string PLANGRP { get; set; }
    public string PLANLEVEL { get; set; }
    public string PLANT { get; set; }
    public string PK { get; set; }
    public decimal? QTY { get; set; }
    public short? SORTKEY { get; set; }
    public string SP_TRAN_TYPE { get; set; }
    public string SP_IND { get; set; }
    public string TAX { get; set; }
    public string TEXT { get; set; }
    public string ASSET { get; set; }
    public string ASSET_TRAN_TYPE { get; set; }
    public string ASSET_SNO { get; set; }
    public string TRAN_KEY { get; set; }
    public string TRAN_TYPE { get; set; }
    public string WBS { get; set; }
    public short? MAT_LINE { get; set; }
    public string order_type;
    public short orig_lineno;
    public string orig_CC;
    public string orig_FMAREA;
    public string orig_GRANT;
    public string orig_FUNCA;
    public long? orig_ORDERNO;
    public string orig_WBS;
    public string orig_tax;
    public string orig_altacc;
    //===================
    // public GlDocList oDOCKEY => GlDocList.Data[DOCKEY];
    public MastCoA oACCOUNT => MastCoA.Data[ACCOUNT];
    public MastCoA ooACCOUNT;
    // public MastCoAAlt oALTACC => MastCoAAlt.Data.ContainsKey(ALTACC) ? MastCoAAlt.Data[ALTACC] : null;
    //============================
    public GlDocLine(object[] oo)
    {
      DOCKEY = (long)oo[0];
      LINENO = (short)oo[1];
      ACCOUNT = (string)oo[2];
      ALTACC = oo[3] == DBNull.Value ? null : (string)oo[3];
      DOCAMT = (decimal)oo[4];
      AMT = (decimal)oo[4];
      ASSIGNMENT = oo[6] == DBNull.Value ? null : (string)oo[6];
      PROFITCENTER = (string)oo[7];
      BUSA = oo[8] == DBNull.Value ? null : (string)oo[8];
      CC = oo[9] == DBNull.Value ? null : (string)oo[9];
      FMAREA = oo[10] == DBNull.Value ? null : (string)oo[10];
      GRANT = oo[11] == DBNull.Value ? null : (string)oo[11];
      FUNCA = oo[12] == DBNull.Value ? null : (string)oo[12];
      PO = oo[13] == DBNull.Value ? (long?)null : (long)oo[13];
      POITEM = oo[14] == DBNull.Value ? (short?)null : (short)oo[14];
      SO = oo[15] == DBNull.Value ? (long?)null : (long)oo[15];
      SOITEM = oo[16] == DBNull.Value ? (int?)null : (int)oo[16];
      LID = oo[17] == DBNull.Value ? null : (string)oo[17];
      MATERIAL = oo[18] == DBNull.Value ? null : (string)oo[18];
      STORNO = (bool)oo[19];
      ORDERNO = oo[20] == DBNull.Value ? (long?)null : (long)oo[20];
      PARTNER = oo[21] == DBNull.Value ? null : (string)oo[21];
      PLANGRP = oo[22] == DBNull.Value ? null : (string)oo[22];
      PLANLEVEL = oo[23] == DBNull.Value ? null : (string)oo[23];
      PLANT = oo[24] == DBNull.Value ? null : (string)oo[24];
      PK = oo[25] == DBNull.Value ? null : (string)oo[25];
      QTY = oo[26] == DBNull.Value ? (decimal?)null : (decimal)oo[26];
      SORTKEY = oo[27] == DBNull.Value ? (short?)null : (short)oo[27];
      SP_TRAN_TYPE = oo[28] == DBNull.Value ? null : (string)oo[28];
      SP_IND = oo[29] == DBNull.Value ? null : (string)oo[29];
      TAX = oo[30] == DBNull.Value ? null : (string)oo[30];
      TEXT = oo[31] == DBNull.Value ? null : (string)oo[31];
      ASSET = oo[32] == DBNull.Value ? null : (string)oo[32];
      ASSET_TRAN_TYPE = oo[33] == DBNull.Value ? null : (string)oo[33];
      ASSET_SNO = oo[34] == DBNull.Value ? null : (string)oo[34];
      TRAN_KEY = oo[35] == DBNull.Value ? null : (string)oo[35];
      TRAN_TYPE = oo[36] == DBNull.Value ? null : (string)oo[36];
      WBS = oo[37] == DBNull.Value ? null : (string)oo[37];
      MAT_LINE = oo[38] == DBNull.Value ? (short?)null : (short)oo[38];
      order_type = oo[39] == DBNull.Value ? null : (string)oo[39];
      orig_lineno = (short)oo[40];
      orig_CC = oo[41] == DBNull.Value ? null : (string)oo[41];
      orig_FMAREA = oo[42] == DBNull.Value ? null : (string)oo[42];
      orig_GRANT = oo[43] == DBNull.Value ? null : (string)oo[43];
      orig_FUNCA = oo[44] == DBNull.Value ? null : (string)oo[44];
      orig_ORDERNO = oo[45] == DBNull.Value ? (long?)null : (long)oo[45];
      orig_WBS = oo[46] == DBNull.Value ? null : (string)oo[46];
      orig_tax = oo[47] == DBNull.Value ? null : (string)oo[47];
      orig_altacc = oo[48] == DBNull.Value ? null : (string)oo[48];
    }

  }
}
