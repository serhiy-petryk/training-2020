using System.Windows;
using WpfInvestigate.Controls;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ToggleButtonAndPopup.xaml
    /// </summary>
    public partial class DropDownButtonTests : Window
    {
        public DropDownButtonTests()
        {
            InitializeComponent();
        }
        private void OpenDropDownMenu(object sender, RoutedEventArgs e) => DropDownButton.OpenDropDownMenu(sender);

    }
}
