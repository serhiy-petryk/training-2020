using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfInvestigate.Common
{
    public class DummyConverter : DependencyObject, IValueConverter
    {
        public static DummyConverter Instance = new DummyConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
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
            if (value is double && parameter is string && double.TryParse((string)parameter, NumberStyles.Any, Tips.InvariantCulture, out percent))
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

    public class GridLinesVisibilityConverter : IValueConverter
    {
        public static GridLinesVisibilityConverter Instance = new GridLinesVisibilityConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, DataGridGridLinesVisibility.None);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, false) ? DataGridGridLinesVisibility.All : DataGridGridLinesVisibility.None;
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

    public class InverseBoolConverter : IValueConverter
    {
        public static InverseBoolConverter Instance = new InverseBoolConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || Equals(value, false) || Equals(value, 0) || Equals(value, "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // Converter for Controls.DoubleButton.xaml
    public class DoubleButtonConverter : IValueConverter
    {
        public static DoubleButtonConverter LeftUpPolygonPoints = new DoubleButtonConverter { _isLeftUpPolygonPoints = true };
        public static DoubleButtonConverter RightDownPolygonPoints = new DoubleButtonConverter();
        public static DoubleButtonConverter RightDownMargin = new DoubleButtonConverter { _isRightDownMargin = true };

        private bool _isLeftUpPolygonPoints = false;
        private bool _isRightDownMargin = false;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double dValue)
            {
                if (_isRightDownMargin)
                    return new Thickness(-dValue, 0, 0, 0);
                return _isLeftUpPolygonPoints
                    ? PointCollection.Parse($"0, 0, {dValue}, 0, 0, {dValue}")
                    : PointCollection.Parse($"0, {dValue}, {dValue}, 0, {dValue}, {dValue}");
            }

            if (targetType == typeof(Thickness)) return new Thickness();
            return $"0, 0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class XDoubleButtonConverter : IValueConverter
    {
        public static XDoubleButtonConverter LeftUpPolygonPoints = new XDoubleButtonConverter { _isLeftUpPolygonPoints = true };
        public static XDoubleButtonConverter RightDownPolygonPoints = new XDoubleButtonConverter();
        public static XDoubleButtonConverter RightDownMargin = new XDoubleButtonConverter { _isRightDownMargin = true };

        private bool _isLeftUpPolygonPoints = false;
        private bool _isRightDownMargin = false;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double dValue)
            {
                if (_isRightDownMargin)
                    return new Thickness(-dValue, 0, 0, 0);
                return _isLeftUpPolygonPoints
                    ? PointCollection.Parse($"0, 0, {dValue}, 0, 0, {dValue}")
                    : PointCollection.Parse($"0, {dValue}, {dValue}, 0, {dValue}, {dValue}");
            }

            if (targetType == typeof(Thickness)) return new Thickness();
            return $"0, 0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
