using System.Windows;
using System.Windows.Media;
using WpfSpLib.Effects;
using WpfSpLib.Helpers;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for DatePickerEffectTests.xaml
    /// </summary>
    public partial class DatePickerEffectTests : Window
    {
        public DatePickerEffectTests()
        {
            InitializeComponent();
            ControlHelper.HideInnerBorderOfDatePickerTextBox(this, true);
        }

        private void ToggleButtonVisibility1_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = DatePickerEffect.GetClearButton(dp1);
            DatePickerEffect.SetClearButton(dp1, !a1);
        }

        private void ToggleButtonVisibility2_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = DatePickerEffect.GetClearButton(dp2);
            DatePickerEffect.SetClearButton(dp2, !a1);
        }

        private void ChangeBackground1_OnClick(object sender, RoutedEventArgs e)
        {
            if (dp1.Background == Brushes.Yellow)
                dp1.Background = Brushes.YellowGreen;
            else
                dp1.Background = Brushes.Yellow;
        }

        private void ChangeForeground1_OnClick(object sender, RoutedEventArgs e)
        {
            if (dp1.Foreground == Brushes.Blue)
                dp1.Foreground = Brushes.Violet;
            else
                dp1.Foreground = Brushes.Blue;
        }
    }
}
