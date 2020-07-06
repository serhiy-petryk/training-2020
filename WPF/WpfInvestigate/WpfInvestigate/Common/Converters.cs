using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfInvestigate.Common
{
    public class SuppressXamlErrorConverter : DependencyObject, IValueConverter
    {
        // Dummy convertor to suppress output errors for TimePickerBase: Cannot find governing FrameworkElement or FrameworkContentElement for target element...
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class PercentageConverter : DependencyObject, IValueConverter
    { // return = (double)ConverterValue * ConverterParameter /100;
        public static PercentageConverter Instance = new PercentageConverter();
        public static PercentageConverter InstanceForPadding = new PercentageConverter{_isPadding = true};
        private bool _isPadding = false;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double percent;
            if (value is double && parameter is string && double.TryParse((string)parameter, NumberStyles.Any, CultureInfo.InvariantCulture, out percent))
            {
                var v = (double) value * percent / 100;
                if (_isPadding)
                    return new Thickness(2 * v, v, 2 * v, v);
                return v;
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class DummyConverter : DependencyObject, IValueConverter
    {
        public static DummyConverter Instance = new DummyConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ChangeTypeConverter : DependencyObject, IValueConverter
    {
        public static ChangeTypeConverter Instance = new ChangeTypeConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Tips.GetDefaultOfType(targetType);
            
            var type = Tips.GetNotNullableType(targetType);
            if (value.GetType() == type)
                return value;

            if (value is IConvertible && type == typeof(string))
                return ((IConvertible)value).ToString(culture);
            if (value is IFormattable && type == typeof(string))
                return ((IFormattable)value).ToString(null, culture);

            try
            {
                return System.Convert.ChangeType(value, type, culture);
            }
            catch { }

            throw new NotImplementedException($"Type converter for {targetType.Name} isn't implemented");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    public class DoubleMultiplyConverter : DependencyObject, IValueConverter
    {
        public static DoubleMultiplyConverter Instance = new DoubleMultiplyConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d1 = double.Parse(parameter.ToString(), CultureInfo.InvariantCulture);
            return (double) value * d1;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GridLinesVisibilityConverter : IValueConverter
    {
        public static GridLinesVisibilityConverter Instance = new GridLinesVisibilityConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, DataGridGridLinesVisibility.None);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, false) ? DataGridGridLinesVisibility.All : DataGridGridLinesVisibility.None;
    }

    public class TestConverter : IValueConverter
    {
        public static TestConverter Instance = new TestConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || Equals(value, false) || Equals(value, 0)
                ? (Equals(parameter, "Hide") ? Visibility.Hidden : Visibility.Collapsed)
                : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

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

    public class InverseVisibilityConverter : IValueConverter
    {
        public static InverseVisibilityConverter Instance = new InverseVisibilityConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || Equals(value, false)
                ? Visibility.Visible
                : (Equals(parameter, "Hide") ? Visibility.Hidden : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class InverseBoolConverter : IValueConverter
    {
        public static InverseBoolConverter Instance = new InverseBoolConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || Equals(value, false) || Equals(value, 0) || Equals(value, "");
        }

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
