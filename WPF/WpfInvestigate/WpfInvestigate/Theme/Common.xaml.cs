using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfInvestigate.Theme
{
    public partial class Common
    {
        public Common()
        {
            InitializeComponent();
        }

        private void OnDatePickerTextBoxLoaded(object sender, RoutedEventArgs e) =>
            Controls.ControlHelper.HideBorderOfTextBox(sender as DatePickerTextBox);

        private void OnToolBarLoaded(object sender, RoutedEventArgs e) =>
            Controls.ControlHelper.SetBorderOfToolbarComboBoxes(sender as ToolBar);
    }
}
