using System.Windows;
using WpfInvestigate.Helpers;

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

        private void ChangeContent_OnClick(object sender, RoutedEventArgs e)
        {
            TB1.Content = TB1.Content.ToString() + "X";
        }
    }
}
