using System.Diagnostics;
using System.Windows;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ChromeTests.xaml
    /// </summary>
    public partial class ChromeTests : Window
    {
        public ChromeTests()
        {
            InitializeComponent();
        }

        private void TestButton_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void Element_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            Debug.Print($"GotFocus: {fe.Name}");
        }

        private void Element_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            Debug.Print($"LostFocus: {fe.Name}");
        }
    }
}
