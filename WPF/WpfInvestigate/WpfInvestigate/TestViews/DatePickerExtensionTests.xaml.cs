using System.Windows;
using WpfInvestigate.Controls;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for DatePickerExtensionTests.xaml
    /// </summary>
    public partial class DatePickerExtensionTests : Window
    {
        public DatePickerExtensionTests()
        {
            InitializeComponent();
        }

        private void Debug1_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = DatePickerExtension.GetClearButton(dp1);
            DatePickerExtension.SetClearButton(dp1, !a1);
        }

        private void Debug2_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = DatePickerExtension.GetClearButton(dp2);
            DatePickerExtension.SetClearButton(dp2, !a1);
        }
    }
}
