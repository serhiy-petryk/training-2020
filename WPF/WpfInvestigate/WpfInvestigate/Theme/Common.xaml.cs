using System.Windows;
using System.Windows.Controls.Primitives;

namespace WpfInvestigate.Theme
{
    public partial class Common
    {
        public Common()
        {
            InitializeComponent();
        }

        private void OnDatePickerTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            Controls.DatePickerHelper.HideBorderOfTextBox(sender as DatePickerTextBox);
        }
    }
}
