using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ColorInvestigation.Common
{
    public class InverseBoolConverter : IValueConverter
    {
        public static InverseBoolConverter Instance = new InverseBoolConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || Equals(value, false) || Equals(value, 0) || Equals(value, "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class DummyConverter : DependencyObject, IValueConverter
    {
        public static DummyConverter Instance = new DummyConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
                return brush.Color.ToString();
            if (value is string)
                return value;
            return "NaN";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
