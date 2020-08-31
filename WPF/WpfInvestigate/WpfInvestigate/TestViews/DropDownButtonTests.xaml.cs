using System.Linq;
using System.Windows;
using WpfInvestigate.Common;
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            // var a1 = T1;
            // var a2 = Tips.GetVisualChildren(T1).ToArray();
            var a12 = T2;
            var a22 = Tips.GetVisualChildren(T2).ToArray();
        }
    }
}
