using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;
using MyWpfMwi.Common;
using MyWpfMwi.Examples;

namespace MyWpfMwi.Controls
{
    /// <summary>
    /// Interaction logic for DGView.xaml
    /// </summary>
    public partial class DGView : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleControl"/> class.
        /// </summary>
        public DGView()
        {
            InitializeComponent();
        }

        private DGListComponent _dGListComponent;
        public void Bind(DGCore.Misc.DataDefiniton dd, string layoutID, string startUpParameters,
            string startUpLayoutName, DGCore.UserSettings.DGV settings)
        {
            DgClear();
            _dGListComponent = new DGListComponent();

            Task.Factory.StartNew(() =>
            {
                var sw = new Stopwatch();
                sw.Start();

                var ds = dd.GetDataSource(_dGListComponent);
                Type listType = typeof(DGCore.DGVList.DGVList<>).MakeGenericType(ds.ItemType);
                // var dataSource = Activator.CreateInstance(listType, ds, (Func<Utils.DGVColumnHelper[]>)GetColumnHelpers);
                var dataSource = (DGCore.DGVList.IDGVList)Activator.CreateInstance(listType, ds, null);
                _dGListComponent.Data = dataSource;
                dataSource.RefreshData();

                sw.Stop();
                var d1 = sw.Elapsed.TotalMilliseconds;
                sw.Reset();
                sw.Start();
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)delegate ()
                    {
                        DataGrid.ItemsSource = (IEnumerable)dataSource;
                        sw.Stop();
                        var d2 = sw.Elapsed.TotalMilliseconds;
                        // LogData.Add("Load data time: " + d1);
                        // LogData.Add("Get data time: " + d2);
                        //if (!AUTOGENERATE_COLUMNS)
                          //  CreateColumns();
                        // IsCommandBarEnabled = true;
                    }
                );
            });
        }

        private void DgClear()
        {
            _dGListComponent?.Dispose();
            DataGrid.ItemsSource = null;
            // _dg.Items.Clear();
            DataGrid.Columns.Clear();
        }

        public void Dispose()
        {
            DgClear();
        }
    }
}
