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
  public class MastCoAAlt
  {
    private static CultureInfo ci = CultureInfo.InvariantCulture;
    public static Dictionary<string, MastCoAAlt> Data;

    public static void RefreshData()
    {
      Data = new Dictionary<string, MastCoAAlt>();
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        cmd.CommandText = "SELECT * from [Mast_CoA_alt]";
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
