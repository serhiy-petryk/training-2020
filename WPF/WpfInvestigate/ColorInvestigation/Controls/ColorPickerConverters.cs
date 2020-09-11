using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ColorInvestigation.Controls
{
    public class BrushToHexConverter : IValueConverter
    {
        public static BrushToHexConverter Instance = new BrushToHexConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = value as SolidColorBrush;
            return $"#{brush.Color.A:X2}{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
