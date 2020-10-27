using System.Windows;
using WpfInvestigate.Controls.Helpers;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ControlDemo.xaml
    /// </summary>
    public partial class ControlDemo : Window
    {
        public ControlDemo()
        {
            InitializeComponent();
        }

        private void OpenDropDownMenu(object sender, RoutedEventArgs e) => DropDownButtonHelper.OpenDropDownMenu(sender);
    }
}
