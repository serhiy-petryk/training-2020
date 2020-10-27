using System.Windows;
using ControlInvestigate.Tests;

namespace ControlInvestigate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TempControl_OnClick(object sender, RoutedEventArgs e) => new TempControl().Show();
    }
}
