using System;
using System.Collections.Generic;
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
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

    // is using in ToggleGridVisibilityButton (DG3.1 project)
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

    /// <summary>
    /// Math converter. Support polish notation (https://en.wikipedia.org/wiki/Polish_notation). Math expressions are stored in ConverterParameter.
    /// Supported operators: '+,-,*,/,%' for double, '!' for boolean. Supported types of return value: double, bool, Thickness, GridLength.
    /// </summary>
    public class MathConverter : IValueConverter
    {
        /// <summary>
        /// Instance of Math converter. Support polish notation (https://en.wikipedia.org/wiki/Polish_notation). Math expressions are stored in ConverterParameter.
        /// Supported operators: '+,-,*,/,%' for double, '!' for boolean. Supported types of return value: double, bool, Thickness, GridLength.
        /// </summary>
        public static MathConverter Instance = new MathConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var operand = "";
            var operands = new List<object> {value};

            var s = (parameter as string ?? "").Trim();

            foreach (var c in s)
            {
                if (c == '.' || char.IsDigit(c))
                {
                    operand += c;
                    continue;
                }

                if (!string.IsNullOrEmpty(operand))
                    operands.Add(operand);
                operand = "";

                if (c != ' ') Calculate(c);
            }

            if (operands.Count !=1)
                throw new Exception($"MathConverter error. Invalid number of operands at the end of calculation. Should be 1, but is {operands.Count}");

            if (targetType == typeof(Thickness))
                return new Thickness(GetDouble(operands[0]));
            if (targetType == typeof(GridLength))
                return new GridLength(GetDouble(operands[0]));
            if (targetType == typeof(double))
                return GetDouble(operands[0]);
            if (targetType == typeof(bool))
                return GetBool(operands[0]);
            throw new Exception($"MathConverter error. Unsupported target type of return value: {targetType.Name}");

            void Calculate(char _operator)
            {
                if (_operator == '+' && operands.Count >= 2)
                    operands[operands.Count - 2] = GetDouble(operands[operands.Count - 2]) + GetDouble(operands[operands.Count - 1]);
                else if (_operator == '-' && operands.Count >= 2)
                    operands[operands.Count - 2] = GetDouble(operands[operands.Count - 2]) - GetDouble(operands[operands.Count - 1]);
                else if (_operator == '*' && operands.Count >= 2)
                    operands[operands.Count - 2] = GetDouble(operands[operands.Count - 2]) * GetDouble(operands[operands.Count - 1]);
                else if (_operator == '/' && operands.Count >= 2)
                    operands[operands.Count - 2] = GetDouble(operands[operands.Count - 2]) / GetDouble(operands[operands.Count - 1]);
                else if (_operator == '%' && operands.Count >= 2)
                    operands[operands.Count - 2] = GetDouble(operands[operands.Count - 2]) * (GetDouble(operands[operands.Count - 1]) / 100.0);
                else if (_operator == '!' && operands.Count >= 1)
                    operands[operands.Count - 1] = !GetBool(operands[operands.Count - 1]);
                else
                {
                    if (operands.Count < 1 && _operator == '!')
                        throw new Exception($"MathConverter error. There are no operand for '{_operator}' operator");
                    if (operands.Count < 2)
                        throw new Exception($"MathConverter error. Not enough operands '{_operator}' operator. Should be 2, but are {operands.Count}");
                    throw new Exception($"MathConverter error. Undefined operator: {_operator}");
                }

                if (_operator != '!')
                    operands.RemoveAt(operands.Count - 1);
            }

            double GetDouble(object o)
            {
                if (o is double d) return d;
                if (o is string s1) return double.Parse(s1);
                throw new Exception($"MathConverter error. Can't convert {o} to double data type");
            }

            bool GetBool(object o) => o is bool b ? b : o != null && !Equals(o, false) && !Equals(o, 0) && !Equals(o, "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class LeftRightButtonConverter : IValueConverter
    {
        public static LeftRightButtonConverter LeftUpPolygonPoints = new LeftRightButtonConverter { _isLeftUpPolygonPoints = true };
        public static LeftRightButtonConverter RightDownPolygonPoints = new LeftRightButtonConverter();
        public static LeftRightButtonConverter BorderWidth = new LeftRightButtonConverter{_isBorderWidth = true};

        private bool _isLeftUpPolygonPoints = false;
        private bool _isBorderWidth = false;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is FrameworkElement fe)
            {
                double.TryParse(fe.Tag as string, out var temp);
                if (_isBorderWidth)
                    return temp;

                var border = temp / 2;
                var height = fe.ActualHeight - border;
                var width = fe.ActualWidth - border;

                if (_isLeftUpPolygonPoints)
                    return new PointCollection(new[] { new Point(border, border), new Point(width, border), new Point(border, height) });
                
                return new PointCollection(new[] { new Point(border, height), new Point(width, border), new Point(width, height) });
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // Converter for Controls.DoubleButton.xaml
    public class ObsoleteDoubleButtonConverter : IValueConverter
    {
        public static ObsoleteDoubleButtonConverter LeftUpPolygonPoints = new ObsoleteDoubleButtonConverter { _isLeftUpPolygonPoints = true };
        public static ObsoleteDoubleButtonConverter RightDownPolygonPoints = new ObsoleteDoubleButtonConverter();
        public static ObsoleteDoubleButtonConverter RightDownMargin = new ObsoleteDoubleButtonConverter { _isRightDownMargin = true };

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
