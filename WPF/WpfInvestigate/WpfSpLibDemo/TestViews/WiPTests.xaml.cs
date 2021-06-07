using System.Windows;

namespace WpfSpLibDemo.TestViews
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

        private void OnChangeSizeClick(object sender, RoutedEventArgs e)
        {
            // AA.Width = AA.ActualWidth * 1.2;
        }
    }
}
