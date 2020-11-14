using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WpfInvestigate.Common.ColorSpaces;

namespace WpfInvestigate.Common
{
    internal static class ColorConverterHelper
    {
        internal static double ConvertValue(double value, object parameter, bool? isUp = null)
        {
            /* Process parameter of ColorHslBrush/ColorLabBrush/ColorGrayScaleBrush
             value is double in range [0-1.0]
             format of parameter: [+ or -]NN[%] where NN is double between 0-100
             split is double in range [0-1.0] or null

            1. split = null ( no split):
             NN[%] return = NN / 100
             +NN  return = value + NN / 100
             -NN    return = value - NN / 100
             +NN% return = value + (1 - value) * NN/100
             -NN%:  return = value - value * NN/100

            2. split has value:
             NN[%] return = NN / 100

             if (value < split)
             +NN  return = value + NN / 100
             -NN    return = value - NN / 100
             +NN% return = value + (1 - value) * NN/100
             -NN%:  return = value - value * NN/100

             if (value >= split)
             +NN  return = value - NN / 100
             -NN    return = value + NN / 100
             +NN% return = value - value * NN/100
             -NN%:  return = value + (1 - value) * NN/100

             return: double in [0-1.0]  or null(if bad parameter)
             */

            if (parameter is string sParameter)
            {
                var isEqual = true;
                var isPlus = true;
                var isPercent = false;

                if (sParameter.StartsWith("+"))
                {
                    isEqual = false;
                    sParameter = sParameter.Substring(1);
                }
                else if (sParameter.StartsWith("-"))
                {
                    isEqual = false;
                    isPlus = false;
                    sParameter = sParameter.Substring(1);
                }

                if (sParameter.EndsWith("%"))
                {
                    isPercent = true;
                    sParameter = sParameter.Substring(0, sParameter.Length - 1);
                }

                double temp;
                if (double.TryParse(sParameter, NumberStyles.Any, Tips.InvariantCulture, out temp))
                {
                    double result;
                    var dParameter = temp / 100.0;

                    if (isEqual)
                        return dParameter;

                    if (isUp.HasValue)
                    {
                        var isValueLess = isUp.Value;
                        if ((isPercent && isPlus && isValueLess) || isPercent && !isPlus && !isValueLess)
                            result = value + (1.0 - value) * dParameter;
                        else if (isPercent)
                            result = value - value * dParameter;
                        else if (isPlus && isValueLess || !isPlus && !isValueLess)
                            result = value + dParameter;
                        else
                            result = value - dParameter;
                    }
                    else
                    {
                        if (isPercent && isPlus)
                            result = value + (1.0 - value) * dParameter;
                        else if (isPercent)
                            result = value - value * dParameter;
                        else if (isPlus)
                            result = value + dParameter;
                        else
                            result = value - dParameter;
                    }

                    result = Math.Round(result, 9);
                    if (result > 1.0)
                        result -= 1.0;
                    else if (result < 0.0)
                        result += 1.0;
                    return result;
                }
            }
            return value;
        }
    }

    public class HueAndSaturationBrush : IValueConverter
    {
        // Usage:
        // Definitions in global resources:
        // <system:String x:Key="HueAndSaturation">0,100</system:String>
        // <common:BindingProxy x:Key="HueAndSaturationProxy" Value="{DynamicResource HueAndSaturation}"/>
        // Set the control property:
        // <StackPanel Background="{Binding Source={StaticResource HueAndSaturation}, Converter={x:Static converters:HueAndSaturationBrush.Instance}, ConverterParameter=70}">
        // or setter of control property:
        // <Setter Property="Foreground" Value="{Binding Source={StaticResource HueAndSaturationProxy}, Converter={x:Static converters:HueAndSaturationBrush.Instance}, ConverterParameter=35}"/>
        public static HueAndSaturationBrush Instance = new HueAndSaturationBrush();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string && double.TryParse((string)parameter, NumberStyles.Any, Tips.InvariantCulture, out var newL))
            {
                string[] ss = null;
                if (value is string)
                    ss = ((string)value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
                else if ((value as BindingProxy)?.Value is string)
                    ss = ((string)((BindingProxy)value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
                else if ((value as DynamicBinding)?.Value is string)
                    ss = ((string)((DynamicBinding)value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

                HSL hsl = null;
                if (ss != null && ss.Length == 2)
                    hsl = new HSL(double.Parse(ss[0], Tips.InvariantCulture) / 360,
                        double.Parse(ss[1], Tips.InvariantCulture) / 100, newL / 100.0);
                else if (value is Brush brush)
                    hsl = new HSL(new RGB(Tips.GetColorFromBrush(brush)));
                else if (value is DependencyObject d)
                    hsl = new HSL(new RGB(Tips.GetActualBackgroundColor(d)));

                if (hsl != null)
                {
                    if (Tips.GetNotNullableType(targetType) == typeof(Color))
                        return new HSL(hsl.H, hsl.S, newL / 100.0).RGB.Color;
                    return new SolidColorBrush(new HSL(hsl.H, hsl.S, newL / 100.0).RGB.Color);
                }
            }

            if (Tips.GetNotNullableType(targetType) == typeof(Color))
                return Colors.Transparent;
            return Brushes.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ColorHslBrush : IValueConverter
    {
        public static ColorHslBrush Instance = new ColorHslBrush();
        public static ColorHslBrush NoSplit = new ColorHslBrush { _noSplit = true };

        private bool _noSplit = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            HSL hsl = null;
            string[] ss = null;
            Color? oldColor = null;

            if (value is string)
                ss = ((string)value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            else if ((value as BindingProxy)?.Value is string)
                ss = ((string)((BindingProxy)value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            else if ((value as DynamicBinding)?.Value is string)
                ss = ((string)((DynamicBinding)value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

            if (ss != null && ss.Length == 2)
                hsl = new HSL(double.Parse(ss[0], Tips.InvariantCulture) / 360,
                    double.Parse(ss[1], Tips.InvariantCulture) / 100, 0);
            else if (value is Brush brush)
                oldColor = Tips.GetColorFromBrush(brush);
            else if (value is Color color)
                oldColor = color;
            else if (value is DependencyObject d)
                oldColor = Tips.GetActualBackgroundColor(d);

            if (oldColor.HasValue)
                hsl = new HSL(new RGB(oldColor.Value));

            if (hsl != null)
            {
                if (parameter is string)
                    foreach (var p in parameter.ToString().Split('/'))
                    {
                        var isDarkColor = ColorUtils.IsDarkColor(hsl.RGB.Color);
                        var newL = ColorConverterHelper.ConvertValue(hsl.L, p, !_noSplit ? isDarkColor : (bool?)null);
                        hsl = new HSL(hsl.H, hsl.S, newL);
                    }

                if (Tips.GetNotNullableType(targetType) == typeof(Color))
                    return hsl.RGB.GetColor(oldColor?.A/255.0 ?? 1.0);
                return new SolidColorBrush(hsl.RGB.GetColor(oldColor?.A / 255.0 ?? 1.0));
            }

            if (Tips.GetNotNullableType(targetType) == typeof(Color))
                return Colors.Transparent;
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ColorLabBrush : IValueConverter
    {
        public static ColorLabBrush Instance = new ColorLabBrush();
        public static ColorLabBrush NoSplit = new ColorLabBrush { _noSplit = true };

        private bool _noSplit = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LAB lab = null;
            string[] ss = null;

            if (value is string)
                ss = ((string)value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            else if ((value as BindingProxy)?.Value is string)
                ss = ((string)((BindingProxy)value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

            if (ss != null && ss.Length == 2)
                lab = new LAB(double.Parse(ss[0], Tips.InvariantCulture), double.Parse(ss[1], Tips.InvariantCulture), 0);
            else if (value is Brush brush)
                lab = new LAB(new RGB(Tips.GetColorFromBrush(brush)));
            else if (value is DependencyObject d)
                lab = new LAB(new RGB(Tips.GetActualBackgroundColor(d)));

            if (lab != null)
            {
                var color = lab.RGB.Color;
                var isDarkColor = ColorUtils.IsDarkColor(color);
                var newL = ColorConverterHelper.ConvertValue(lab.L / 100.0, parameter, !_noSplit ? isDarkColor : (bool?)null);
                if (Tips.GetNotNullableType(targetType) == typeof(Color))
                    return new LAB(newL * 100.0, lab.A, lab.B).RGB.Color;
                return new SolidColorBrush(new LAB(newL * 100.0, lab.A, lab.B).RGB.Color);

            }

            if (Tips.GetNotNullableType(targetType) == typeof(Color))
                return Colors.Transparent;
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ColorGrayScaleBrush : IValueConverter
    {
        public static ColorGrayScaleBrush Instance = new ColorGrayScaleBrush();
        public static ColorGrayScaleBrush NoSplit = new ColorGrayScaleBrush { _noSplit = true };

        private bool _noSplit = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color? color = null;
            if (value is Brush brush)
                color = Tips.GetColorFromBrush(brush);
            else if (value is DependencyObject d)
                color = Tips.GetActualBackgroundColor(d);

            if (color.HasValue)
            {
                var oldGrayLevel = ColorUtils.GetGrayLevel(new RGB(color.Value)) / 255.0;
                var isDarkColor = ColorUtils.IsDarkColor(color.Value);
                var newGrayLevel = ColorConverterHelper.ConvertValue(oldGrayLevel, parameter, !_noSplit ? isDarkColor : (bool?)null);
                var newGrayValue = System.Convert.ToByte(newGrayLevel * 255.0);
                return new SolidColorBrush(Color.FromRgb(newGrayValue, newGrayValue, newGrayValue));
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GradientMonochromeBrush : IValueConverter
    {
        public static GradientMonochromeBrush Instance = new GradientMonochromeBrush();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && parameter is string)
            {
                var ss = ((string) value).Split(new[] {",", " "}, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length >= 2)
                {
                    var hue = double.Parse(ss[0], Tips.InvariantCulture) / 360;
                    var saturation = double.Parse(ss[1], Tips.InvariantCulture) / 100;
                    var lightness = ss.Length > 2 ? double.Parse(ss[2], Tips.InvariantCulture) / 100 : 0.0;

                    var gradientStops = new GradientStopCollection( );
                    foreach (var p in parameter.ToString().Split('/'))
                    {
                        var pp = p.Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (pp.Length == 2)
                        {
                            var newL = ColorConverterHelper.ConvertValue(lightness, pp[0]);
                            var hsl = new HSL(hue, saturation, newL);
                            var offset = double.Parse(pp[1].Trim(), Tips.InvariantCulture);
                            gradientStops.Add(new GradientStop(hsl.RGB.Color, offset));
                        }
                    }

                    return new LinearGradientBrush(gradientStops, new Point(0, 0), new Point(0, 1));
                }
            }
            return Brushes.Transparent; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
