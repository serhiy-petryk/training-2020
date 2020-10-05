using System.Windows;

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
            Controls.DatePickerHelper.HideBorderOfTextBox((FrameworkElement) sender);
        }
    }
}
