using System.Windows;

namespace WpfInvestigate.TestViews
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
            Calc.Value = (Calc.Value ?? 100M) + 10;
        }
    }
}
