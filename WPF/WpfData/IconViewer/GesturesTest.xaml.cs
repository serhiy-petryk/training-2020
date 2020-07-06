using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IconViewer.Utils;

namespace IconViewer
{
    /// <summary>
    /// Interaction logic for GesturesTest.xaml
    /// </summary>
    public partial class GesturesTest : UserControl
    {
        public RelayCommand StartCommand { get; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand StopCommand { get; }

        public GesturesTest()
        {
            InitializeComponent();
            DataContext = this;
            StartCommand = new RelayCommand((p) => Cmd("Start"));
            CloseCommand = new RelayCommand((p) => Cmd("Close"));
            StopCommand = new RelayCommand((p) => Cmd("Stop"));
        }

        private void Cmd(object p)
        {
            MessageBox.Show(p as string);
        }
    }
}
