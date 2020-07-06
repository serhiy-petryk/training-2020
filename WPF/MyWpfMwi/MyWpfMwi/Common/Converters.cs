using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MyWpfMwi.Common
{
    public class VisibilityConverter : IValueConverter
    {
        public static VisibilityConverter Instance = new VisibilityConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || Equals(value, false) || Equals(value, 0)
                ? (Equals(parameter, "Hide") ? Visibility.Hidden : Visibility.Collapsed)
                : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class InverseVisibilityConverter : IValueConverter
    {
        public static InverseVisibilityConverter Instance = new InverseVisibilityConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value == null || Equals(value, false)
                ? Visibility.Visible
                : (Equals(parameter, "Hide") ? Visibility.Hidden : Visibility.Collapsed);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class StringToGeometryConverter : IValueConverter
    {
        public static StringToGeometryConverter Instance = new StringToGeometryConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Geometry.Parse((string)value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // Converter for Controls.DoubleButton.xaml
    public class DoubleButtonConverter : IValueConverter
    {
        public static DoubleButtonConverter Instance = new DoubleButtonConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var templateName = (string)parameter;
                var size = (double)value;
                if (templateName == "RightDownButton")
                    return PointCollection.Parse($"0, {size}, {size}, 0, {size}, {size}");
                if (templateName == "LeftUpButton")
                    return PointCollection.Parse($"0, 0, {size}, 0, 0, {size}");
            }
            catch { }
            return $"0, 0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }


}
