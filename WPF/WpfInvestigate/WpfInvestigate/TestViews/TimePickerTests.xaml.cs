using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfInvestigate.TestViews
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

        private void ChangeDate_OnClick(object sender, RoutedEventArgs e) => dtp.SelectedDate = DateTime.Now;
        private void ChangeDate1_OnClick(object sender, RoutedEventArgs e) => dtp1.SelectedDate = DateTime.Now;
        private void ChangeDate2_OnClick(object sender, RoutedEventArgs e) => dtp2.SelectedDate = DateTime.Now;
        private void ChangeDate3_OnClick(object sender, RoutedEventArgs e) => dtp3.SelectedDate = DateTime.Now;
        private void DatePickerMode_OnClick(object sender, RoutedEventArgs e) => dtp.DatePickerMode = !dtp.DatePickerMode;
        private void DatePickerMode1_OnClick(object sender, RoutedEventArgs e) => dtp1.DatePickerMode = !dtp1.DatePickerMode;
        private void DatePickerMode2_OnClick(object sender, RoutedEventArgs e) => dtp2.DatePickerMode = !dtp2.DatePickerMode;
        private void DatePickerMode3_OnClick(object sender, RoutedEventArgs e) => dtp3.DatePickerMode = !dtp3.DatePickerMode;

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
    }
}
