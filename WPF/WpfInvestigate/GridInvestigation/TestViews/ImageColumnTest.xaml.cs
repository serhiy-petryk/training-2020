using System.Collections.Generic;
using System.Data.OleDb;
using System.Windows;
using GridInvestigation.Models;

namespace GridInvestigation.TestViews
{
    /// <summary>
    /// Interaction logic for ImageColumnTest.xaml
    /// </summary>
    public partial class ImageColumnTest : Window
    {
        private const string ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=E:\Apps\archive\Northwind\nwind.mdb";
        public ImageColumnTest()
        {
            InitializeComponent();
            DG.ItemsSource = LoadData();
        }

        public List<Categories> LoadData()
        {
            var data = new List<Categories>();
            using (var conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 150;
                    cmd.CommandText = "select * from Categories";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                        {
                            data.Add(new Categories(rdr));
                        }
                }
            }

            return data;
        }
    }
}
