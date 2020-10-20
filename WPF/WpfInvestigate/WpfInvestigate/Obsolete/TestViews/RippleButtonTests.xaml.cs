using System.Windows;
using System.Windows.Controls;

namespace WpfInvestigate.Obsolete.TestViews
{
    /// <summary>
    /// Interaction logic for RippleButtonTests.xaml
    /// </summary>
    public partial class RippleButtonTests
    {
        public RippleButtonTests()
        {
            InitializeComponent();
        }

        private void OpenSettingButton_OnChecked(object sender, RoutedEventArgs e) => ToggleButtonHelper.OpenMenu_OnCheck(sender);

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("ButtonBase_OnClick");
        }

        private void Dropdown_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var b = new Button();
            var tt = b.ToolTip;
            //throw new System.NotImplementedException();
        }
    }
}
