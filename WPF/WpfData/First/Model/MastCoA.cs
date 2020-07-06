using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First.Model
{
  public class MastCoA
  {
    private static CultureInfo ci = CultureInfo.InvariantCulture;
    private static int cnt = 0;
    public static Dictionary<string, MastCoA> Data;

    public static void RefreshData()
    {
      if (MastCoAAlt.Data == null)
        MastCoAAlt.RefreshData();

      Data = new Dictionary<string, MastCoA>();
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        // cmd.CommandText = "SELECT * from [Mast_CoA] where account like N'1%'";
        cmd.CommandText = "SELECT * from [Mast_CoA]";
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
    public int _ID_ { get; protected set; } = cnt++;

    public string ACCOUNT { get; set; }
    public string ALTACC { get; set; }
    public string SHORT_TEXT { get; set; }
    public string LONG_TEXT { get; set; }
    public string ACCOUNT_GROUP { get; set; }
    public DateTime? DATED { get; set; }
    public MastCoA(object[] oo)
    {
      ACCOUNT = (string)oo[0];
      ALTACC = oo[1] == DBNull.Value ? null : (string)oo[1];
      SHORT_TEXT = oo[2] == DBNull.Value ? null : (string)oo[2];
      LONG_TEXT = oo[3] == DBNull.Value ? null : (string)oo[3];
      ACCOUNT_GROUP = oo[4] == DBNull.Value ? null : (string)oo[4];
      DATED = oo[29] == DBNull.Value ? (DateTime?)null : (DateTime)oo[29];
    }
  }
}
