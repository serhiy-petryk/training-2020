using System.Windows;
using System.Windows.Media;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for FlatButton.xaml
    /// </summary>
    public partial class FlatButtonTests
    {
        public FlatButtonTests()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Click");
        }

        private void OnChangeBackgroundClick(object sender, RoutedEventArgs e)
        {
            if (dp.Background is SolidColorBrush brush && brush.Color == Colors.Blue)
                dp.Background = new SolidColorBrush(Colors.Yellow);
            else
                dp.Background = new SolidColorBrush(Colors.Blue);
        }
    }
}
