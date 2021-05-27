using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;
using WpfInvestigate.Helpers;

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
            var a1 = Tips.GetVisualParents((DependencyObject) sender).OfType<MwiStartup>().FirstOrDefault();
            var a2 = a1.Content as MwiChild;
            a2.Background = Brushes.Green;

        }
    }
}
