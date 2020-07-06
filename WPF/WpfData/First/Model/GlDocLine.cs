using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;

namespace First.Model
{
  public class GlDocLine
  {

    private static CultureInfo ci = CultureInfo.InvariantCulture;
    private static int cnt = 0;
    public static Dictionary<int, GlDocLine> Data;

    public static void RefreshData(int records = 100)
    {
      if (MastCoA.Data == null)
        MastCoA.RefreshData();
      if (GlDocList.Data == null)
        GlDocList.RefreshData();

      Data = new Dictionary<int, GlDocLine>();
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
            Data.Add(o._ID_, o);
          }
        }
      }
    }

    public int _ID_ { get; protected set; } = cnt++;
    public long DOCKEY { get; set; }
    public short LINENO { get; protected set; }

    public GlDocList oGlDocList => GlDocList.Data[DOCKEY];
    public MastCoA oACCOUNT => MastCoA.Data.ContainsKey(ACCOUNT) ? MastCoA.Data[ACCOUNT] : null;
    public MastCoAAlt oALTACC => MastCoAAlt.Data.ContainsKey(ALTACC) ? MastCoAAlt.Data[ALTACC] : null;

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

    public GlDocLine(DbDataReader r)
    {
      DOCKEY = r.GetInt64(0);
      LINENO = r.GetInt16(1);
      ACCOUNT = r.GetString(2);
      ALTACC = r.IsDBNull(3) ? null : r.GetString(3);
      DOCAMT = r.GetDecimal(4);
      AMT = r.GetDecimal(5);
      ASSIGNMENT = r.IsDBNull(6) ? null : r.GetString(6);
      PROFITCENTER = r.GetString(7);
      BUSA = r.IsDBNull(8) ? null : r.GetString(8);
      CC = r.IsDBNull(9) ? null : r.GetString(9);
      FMAREA = r.IsDBNull(10) ? null : r.GetString(10);
      GRANT = r.IsDBNull(11) ? null : r.GetString(11);
      FUNCA = r.IsDBNull(12) ? null : r.GetString(12);
      PO = r.IsDBNull(13) ? (long?)null : r.GetInt64(13);
      POITEM = r.IsDBNull(14) ? (short?)null : r.GetInt16(14);
      SO = r.IsDBNull(15) ? (long?)null : r.GetInt64(15);
      SOITEM = r.IsDBNull(16) ? (int?)null : r.GetInt32(16);
      LID = r.IsDBNull(17) ? null : r.GetString(17);
      MATERIAL = r.IsDBNull(18) ? null : r.GetString(18);
      STORNO = r.GetBoolean(19);
      ORDERNO = r.IsDBNull(20) ? (long?)null : r.GetInt64(20);
      PARTNER = r.IsDBNull(21) ? null : r.GetString(21);
      PLANGRP = r.IsDBNull(22) ? null : r.GetString(22);
      PLANLEVEL = r.IsDBNull(23) ? null : r.GetString(23);
      PLANT = r.IsDBNull(24) ? null : r.GetString(24);
      PK = r.IsDBNull(25) ? null : r.GetString(25);
      QTY = r.IsDBNull(26) ? (decimal?)null : r.GetDecimal(26);
      SORTKEY = r.IsDBNull(27) ? (short?)null : r.GetInt16(27);
      SP_TRAN_TYPE = r.IsDBNull(28) ? null : r.GetString(28);
      SP_IND = r.IsDBNull(29) ? null : r.GetString(29);
      TAX = r.IsDBNull(30) ? null : r.GetString(30);
      TEXT = r.IsDBNull(31) ? null : r.GetString(31);
      ASSET = r.IsDBNull(32) ? null : r.GetString(32);
      ASSET_TRAN_TYPE = r.IsDBNull(33) ? null : r.GetString(33);
      ASSET_SNO = r.IsDBNull(34) ? null : r.GetString(34);
      TRAN_KEY = r.IsDBNull(35) ? null : r.GetString(35);
      TRAN_TYPE = r.IsDBNull(36) ? null : r.GetString(36);
      WBS = r.IsDBNull(37) ? null : r.GetString(37);
      MAT_LINE = r.IsDBNull(38) ? (short?)null : r.GetInt16(38);
      order_type = r.IsDBNull(39) ? null : r.GetString(39);
      orig_lineno = r.GetInt16(40);
      orig_CC = r.IsDBNull(41) ? null : r.GetString(41);
      orig_FMAREA = r.IsDBNull(42) ? null : r.GetString(42);
      orig_GRANT = r.IsDBNull(43) ? null : r.GetString(43);
      orig_FUNCA = r.IsDBNull(44) ? null : r.GetString(44);
      orig_ORDERNO = r.IsDBNull(45) ? (long?)null : r.GetInt64(45);
      orig_WBS = r.IsDBNull(46) ? null : r.GetString(46);
      orig_tax = r.IsDBNull(47) ? null : r.GetString(47);
      orig_altacc = r.IsDBNull(48) ? null : r.GetString(48);
    }

  }
}
