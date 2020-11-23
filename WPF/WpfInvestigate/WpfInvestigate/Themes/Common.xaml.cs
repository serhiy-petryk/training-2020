using System.Windows;
using System.Windows.Controls;
using WpfInvestigate.Controls.Helpers;

namespace WpfInvestigate.Themes
{
    public partial class Common
    {
        public Common()
        {
            InitializeComponent();
        }

        private void OnToolBarLoaded(object sender, RoutedEventArgs e) =>
            ControlHelper.SetBorderOfToolbarComboBoxes(sender as ToolBar);
    }
}
