using System.Windows;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for WiPTests.xaml
    /// </summary>
    public partial class WiPTests
    {

        public WiPTests()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OpenDropDownMenu(object sender, RoutedEventArgs e) => DropDownButtonHelper.OpenDropDownMenu(sender);

        private void OnChangeSizeClick(object sender, RoutedEventArgs e)
        {
            // AA.Width = AA.ActualWidth * 1.2;
        }
    }
}
