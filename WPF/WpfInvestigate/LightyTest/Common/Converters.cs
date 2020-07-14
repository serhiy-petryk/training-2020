using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LightyTest.Common
{
    public class VisibilityConverter : IValueConverter
    {
        public static VisibilityConverter Instance = new VisibilityConverter();
        public static VisibilityConverter InverseInstance = new VisibilityConverter {_inverse = true};
        private bool _inverse;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _inverse ^ (value == null || Equals(value, false) || Equals(value, 0) || Equals(value, ""))
                ? (Equals(parameter, "Hide") ? Visibility.Hidden : Visibility.Collapsed)
                : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
