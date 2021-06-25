using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for TimePickerTest.xaml
    /// </summary>
    public partial class TimePickerTests
    {
        public TimePickerTests()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ChangeNullable_OnClick(object sender, RoutedEventArgs e)
        {
            var x = NNN;
            x.IsNullable = !x.IsNullable;
        }

        private void ChangeDate_OnClick(object sender, RoutedEventArgs e) => dtp.SelectedDateTime = DateTime.Now;
        private void ChangeDate1_OnClick(object sender, RoutedEventArgs e) => dtp1.SelectedDateTime = DateTime.Now;
        private void ChangeDate2_OnClick(object sender, RoutedEventArgs e) => dtp2.SelectedDateTime = DateTime.Now;
        private void ChangeDate3_OnClick(object sender, RoutedEventArgs e) => dtp3.SelectedDateTime = DateTime.Now;
        private void DatePickerMode_OnClick(object sender, RoutedEventArgs e) => dtp.IsDateOnlyMode = !dtp.IsDateOnlyMode;
        private void DatePickerMode1_OnClick(object sender, RoutedEventArgs e) => dtp1.IsDateOnlyMode = !dtp1.IsDateOnlyMode;
        private void DatePickerMode2_OnClick(object sender, RoutedEventArgs e) => dtp2.IsDateOnlyMode = !dtp2.IsDateOnlyMode;
        private void DatePickerMode3_OnClick(object sender, RoutedEventArgs e) => dtp3.IsDateOnlyMode = !dtp3.IsDateOnlyMode;

        private void TimeFormat_OnClick(object sender, RoutedEventArgs e)
        {
            dtp.SelectedTimeFormat = dtp.SelectedTimeFormat == DatePickerFormat.Long ? DatePickerFormat.Short : DatePickerFormat.Long;
        }
        private void DateFormat_OnClick(object sender, RoutedEventArgs e)
        {
            dtp.SelectedDateFormat = dtp.SelectedDateFormat == DatePickerFormat.Long ? DatePickerFormat.Short : DatePickerFormat.Long;
        }

        private void ReadOnly_OnClick(object sender, RoutedEventArgs e)
        {
            dtp.IsReadOnly = !dtp.IsReadOnly;
        }

        private void IsEnabled_OnClick(object sender, RoutedEventArgs e)
        {
            dtp.IsEnabled = !dtp.IsEnabled;
        }

        private void ChangeBackground_OnClick(object sender, RoutedEventArgs e)
        {
            if (dtp.Background == Brushes.Yellow)
                dtp.Background = Brushes.YellowGreen;
            else
                dtp.Background = Brushes.Yellow;
        }

        private void ChangeForeground_OnClick(object sender, RoutedEventArgs e)
        {
            if (dtp.Foreground == Brushes.Blue)
                dtp.Foreground = Brushes.Violet;
            else
                dtp.Foreground = Brushes.Blue;
        }
    }
}
