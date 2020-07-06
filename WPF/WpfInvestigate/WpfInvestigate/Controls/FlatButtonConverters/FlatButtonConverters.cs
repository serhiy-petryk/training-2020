using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls.FlatButtonConverters
{
    public class GetBorderColorConverter : DependencyObject, IValueConverter
    {
        public static GetBorderColorConverter Instance = new GetBorderColorConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Control c && c.BorderBrush is SolidColorBrush brush)
                return brush.Color;
            return Colors.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GetBackgroundColorConverter : DependencyObject, IValueConverter
    {
        public static GetBackgroundColorConverter Instance = new GetBackgroundColorConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = Tips.GetActualBackgroundColor((DependencyObject)value);
            return color;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GetForegroundColorConverter : DependencyObject, IValueConverter
    {
        public static GetForegroundColorConverter Instance = new GetForegroundColorConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = Tips.GetActualForegroundColor((DependencyObject)value);
            return color;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class GetMouseOverBackgroundColorConverter : DependencyObject, IValueConverter
    {
        public static GetMouseOverBackgroundColorConverter Instance = new GetMouseOverBackgroundColorConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var backColor = Tips.GetActualBackgroundColor((DependencyObject)value);
            var foreColor = Tips.GetActualForegroundColor((DependencyObject)value);
            var color = Color.FromArgb((byte)0xFF, System.Convert.ToByte(0.75 * backColor.R + 0.25 * foreColor.R),
                System.Convert.ToByte(0.75 * backColor.G + 0.25 * foreColor.G),
                System.Convert.ToByte(0.75 * backColor.B + 0.25 * foreColor.B));
            return color;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GetForegroundBrushConverter : DependencyObject, IValueConverter
    {
        public static GetForegroundBrushConverter Instance = new GetForegroundBrushConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = Tips.GetActualForegroundBrush((DependencyObject)value) ?? new SolidColorBrush(Colors.Transparent);
            return brush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }


}
