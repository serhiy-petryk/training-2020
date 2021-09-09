using System;
using System.Data.Common;
using System.Text;

namespace GridInvestigation.Models
{
    public class Categories
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }
        public string Icon_17 { get; set; }
        public string Icon_25 { get; set; }

        public Categories(DbDataReader reader)
        {
            CategoryId = (int)reader["CategoryId"];
            CategoryName = (string)reader["CategoryName"];
            Description = (string)reader["CategoryName"];
            Picture = (byte[])reader["Picture"];
            Icon_17 = ConvertFrom((byte[])reader["Icon_17"]);
            Icon_25 = ConvertFrom((byte[])reader["Icon_25"]);
        }

        public string ConvertFrom(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            var bb = (byte[])value;
            var hex = new StringBuilder("0x", bb.Length * 2 + 2);
            foreach (var b in bb)
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }

    }
}
