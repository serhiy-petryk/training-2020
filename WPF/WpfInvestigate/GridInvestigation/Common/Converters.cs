using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GridInvestigation.Common
{
    public class SuppressXamlErrorConverter : DependencyObject, IValueConverter
    {
        // Dummy convertor to suppress output errors for TimePickerBase: Cannot find governing FrameworkElement or FrameworkContentElement for target element...
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class DummyConverter : DependencyObject, IValueConverter
    {
        public static DummyConverter Instance = new DummyConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var aa1 = Validation.GetErrors(value as DependencyObject).Select(e=> e.ErrorContent.ToString()).ToList();
            return string.Join(Environment.NewLine, aa1);
            return "AAA";
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
