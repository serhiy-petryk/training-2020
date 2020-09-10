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

    public class HueConverter : IValueConverter
    {
        public static HueConverter Instance = new HueConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double hue = (((double)value) / 100) * 360;
            byte max = 255;
            byte min = 0;
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (hue <= 60)
            {
                r = max;
                g = (byte)((hue / 60) * max);
                b = min;
            }
            else if (hue <= 120)
            {
                r = (byte)(((120 - hue) / 60) * max);
                g = max;
                b = min;
            }
            else if (hue <= 180)
            {
                r = min;
                g = max;
                b = (byte)(((hue - 120) / 60) * max);
            }
            else if (hue <= 240)
            {
                r = min;
                g = (byte)(((240 - hue) / 60) * max);
                b = max;
            }
            else if (hue <= 300)
            {
                r = (byte)(((hue - 240) / 60) * max);
                g = min;
                b = max;
            }
            else
            {
                r = max;
                g = min;
                b = (byte)(((360 - hue) / 60) * max);
            }

            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
