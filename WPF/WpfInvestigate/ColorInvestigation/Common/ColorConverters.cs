using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ColorInvestigation.Lib;

namespace ColorInvestigation.Common
{

    internal static class ColorConverterHelper
    {
        internal static double? ConvertValue(double value, object parameter, bool? isUp = null)
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
            return null;
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
            if (parameter is string && double.TryParse((string) parameter, NumberStyles.Any, Tips.InvariantCulture, out var newL))
            {
                string[] ss = null;
                if (value is string)
                    ss = ((string) value).Split(new [] {",", " "}, StringSplitOptions.RemoveEmptyEntries);
                else if ((value as BindingProxy)?.Value is string)
                    ss = ((string)((BindingProxy)value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
                else if ((value as DynamicBinding)?.Value is string)
                    ss = ((string)((DynamicBinding)value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

                if (ss != null && ss.Length == 2)
                {
                    var h = double.Parse(ss[0], Tips.InvariantCulture);
                    var s = double.Parse(ss[1], Tips.InvariantCulture);
                    if (Tips.GetNotNullableType(targetType) == typeof(Color))
                        ColorUtilities.HslToColor(h / 360, s / 100, newL / 100.0);
                    return new SolidColorBrush(ColorUtilities.HslToColor(h / 360, s / 100, newL / 100.0));
                }

                if (value is Brush brush)
                {
                    var hsl = ColorUtilities.ColorToHsl(Tips.GetColorFromBrush(brush));
                    if (Tips.GetNotNullableType(targetType) == typeof(Color))
                        return ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL / 100.0);
                    return new SolidColorBrush(ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL / 100.0));
                }

                if (value is DependencyObject d)
                {
                    var color = Tips.GetActualBackgroundColor(d);
                    var hsl = ColorUtilities.ColorToHsl(color);
                    if (Tips.GetNotNullableType(targetType) == typeof(Color))
                        return ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL / 100.0);
                    return new SolidColorBrush(ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL / 100.0));
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
        public static ColorHslBrush InstanceWithSplit = new ColorHslBrush { _isSplit = true };

        private bool _isSplit = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Tuple<double, double, double> hsl = null;
            string[] ss = null;

            if (value is string)
                ss = ((string)value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            else if ((value as BindingProxy)?.Value is string)
                ss = ((string)((BindingProxy)value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            else if ((value as DynamicBinding)?.Value is string)
                ss = ((string)((DynamicBinding)value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

            if (ss != null && ss.Length == 2)
                hsl = new Tuple<double, double, double>(double.Parse(ss[0], Tips.InvariantCulture), double.Parse(ss[1], Tips.InvariantCulture), 0);
            else if (value is Brush brush)
                hsl = ColorUtilities.ColorToHsl(Tips.GetColorFromBrush(brush));
            else if (value is DependencyObject d)
                hsl = ColorUtilities.ColorToHsl(Tips.GetActualBackgroundColor(d));

            if (hsl != null)
            {
                var color = ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, hsl.Item3);
                var isDarkColor = ColorUtilities.IsDarkColor(color);
                var newL = ColorConverterHelper.ConvertValue(hsl.Item3, parameter, _isSplit ? isDarkColor : (bool?) null);
                if (newL.HasValue)
                {
                    if (Tips.GetNotNullableType(targetType) == typeof(Color))
                        return ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL.Value);
                    return new SolidColorBrush(ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL.Value));
                }
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
        public static ColorLabBrush InstanceWithSplit = new ColorLabBrush { _isSplit = true };

        private bool _isSplit = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Tuple<double, double, double> lab = null;
            string[] ss = null;

            if (value is string)
                ss = ((string)value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            else if ((value as BindingProxy)?.Value is string)
                ss = ((string)((BindingProxy)value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

            if (ss != null && ss.Length == 2)
                lab = new Tuple<double, double, double>(double.Parse(ss[0], Tips.InvariantCulture), double.Parse(ss[1], Tips.InvariantCulture), 0);
            else if (value is Brush brush)
                lab = ColorUtilities.ColorToLab(Tips.GetColorFromBrush(brush));
            else if (value is DependencyObject d)
                lab = ColorUtilities.ColorToLab(Tips.GetActualBackgroundColor(d));

            if (lab != null)
            {
                var color = ColorUtilities.LabToColor(lab.Item1, lab.Item2, lab.Item3);
                var isDarkColor = ColorUtilities.IsDarkColor(color);
                var newL = ColorConverterHelper.ConvertValue(lab.Item1 / 100.0, parameter, _isSplit ? isDarkColor : (bool?)null);
                if (newL.HasValue)
                {
                    if (Tips.GetNotNullableType(targetType) == typeof(Color))
                        return ColorUtilities.LabToColor(newL.Value * 100.0, lab.Item2, lab.Item3);
                    return new SolidColorBrush(ColorUtilities.LabToColor( newL.Value * 100.0,  lab.Item2, lab.Item3));
                }
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
        public static ColorGrayScaleBrush InstanceWithSplit = new ColorGrayScaleBrush { _isSplit = true };

        private bool _isSplit = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color? color = null;
            if (value is Brush brush)
                color = Tips.GetColorFromBrush(brush);
            else if (value is DependencyObject d)
                color = Tips.GetActualBackgroundColor(d);

            if (color.HasValue)
            {
                var oldGrayLevel = ColorUtilities.ColorToGrayLevel(color.Value) / 255.0;
                var isDarkColor = ColorUtilities.IsDarkColor(color.Value);
                var newGrayLevel = ColorConverterHelper.ConvertValue(oldGrayLevel, parameter, _isSplit ? isDarkColor : (bool?)null);
                if (newGrayLevel.HasValue)
                {
                    var newGrayValue = System.Convert.ToByte(newGrayLevel * 255.0);
                    return new SolidColorBrush(Color.FromRgb(newGrayValue, newGrayValue, newGrayValue));
                }
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class TestConverter : IValueConverter
    {
        public static TestConverter Instance = new TestConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
                return "X" + (string) value;
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
