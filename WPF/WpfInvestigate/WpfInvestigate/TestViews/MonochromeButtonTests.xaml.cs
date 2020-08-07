using System.Windows;
using System.Windows.Media;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for FlatButton.xaml
    /// </summary>
    public partial class MonochromeButtonTests
    {
        public MonochromeButtonTests()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Click");
        }

        private void OnChangeBackgroundClick(object sender, RoutedEventArgs e)
        {
            if (dp.Background == Brushes.Blue)
                dp.Background = Brushes.Yellow;
            else
                dp.Background = Brushes.Blue;
        }
    }
}
