using System.Windows;

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
    }
}
