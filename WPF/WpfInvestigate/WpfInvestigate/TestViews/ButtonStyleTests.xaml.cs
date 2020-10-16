using System.Windows;
using WpfInvestigate.Controls.Helpers;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ButtonStyleTests.xaml
    /// </summary>
    public partial class ButtonStyleTests : Window
    {
        public ButtonStyleTests()
        {
            InitializeComponent();
        }

        private void OpenDropDownMenu(object sender, RoutedEventArgs e) => DropDownButtonHelper.OpenDropDownMenu(sender);
    }
}
