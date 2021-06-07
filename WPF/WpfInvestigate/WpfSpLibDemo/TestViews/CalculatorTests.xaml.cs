using System.Windows;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for CalculatorTests.xaml
    /// </summary>
    public partial class CalculatorTests : Window
    {
        public CalculatorTests()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OnSetValueButtonClick(object sender, RoutedEventArgs e)
        {
            DarkCalc.Value = (DarkCalc.Value ?? 100M) + 10;
        }
    }
}
