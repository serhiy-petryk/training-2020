using System.Windows;
using WpfInvestigate.Controls.Effects;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for DatePickerEffectTests.xaml
    /// </summary>
    public partial class DatePickerEffectTests : Window
    {
        public DatePickerEffectTests()
        {
            InitializeComponent();
        }

        private void Debug1_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = DatePickerEffect.GetClearButton(dp1);
            DatePickerEffect.SetClearButton(dp1, !a1);
        }

        private void Debug2_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = DatePickerEffect.GetClearButton(dp2);
            DatePickerEffect.SetClearButton(dp2, !a1);
        }
    }
}
