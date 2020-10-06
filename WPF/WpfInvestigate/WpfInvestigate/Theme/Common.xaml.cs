using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfInvestigate.Controls.Helpers;

namespace WpfInvestigate.Theme
{
    public partial class Common
    {
        public Common()
        {
            InitializeComponent();
        }

        private void OnDatePickerTextBoxLoaded(object sender, RoutedEventArgs e) =>
            ControlHelper.HideBorderOfDatePickerTextBox(sender as DatePickerTextBox);

        private void OnToolBarLoaded(object sender, RoutedEventArgs e) =>
            ControlHelper.SetBorderOfToolbarComboBoxes(sender as ToolBar);
    }
}
