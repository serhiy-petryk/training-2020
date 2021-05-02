using System.Linq;
using System.Windows;
using WpfInvestigate.Common;
using WpfInvestigate.Temp;

namespace WpfInvestigate.Samples
{
    /// <summary>
    /// Interaction logic for CommandBarExample.xaml
    /// </summary>
    public partial class MwiCommandBarSample
    {
        public MwiCommandBarSample()
        {
            InitializeComponent();
        }

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            var a1 = this.GetVisualParents().OfType<MwiStartup>().FirstOrDefault();
            PropertyInvestigation.UpdateProperties3(a1);
        }
    }
}
