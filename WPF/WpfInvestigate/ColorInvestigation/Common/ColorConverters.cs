using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ColorInvestigation.Lib;

namespace ColorInvestigation.Common
{

    #region ==========  BindingProxy  ==============
    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore() => new BindingProxy();
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(object), typeof(BindingProxy), new FrameworkPropertyMetadata(default));
        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
    }

    public class HslProxy : Freezable, INotifyPropertyChanged
    {
        protected override Freezable CreateInstanceCore() => new HslProxy();
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(ColorHSL), typeof(HslProxy), new FrameworkPropertyMetadata(default));

        public SolidColorBrush Brush => Value == null ? new SolidColorBrush(Colors.Transparent) : ((ColorHSL)Value).Brush;
        private static void AAA(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var proxy = (HslProxy)d;
            proxy.OnPropertiesChanged(new [] {nameof(Brush)});
        }

        public ColorHSL Value
        {
            get => (ColorHSL)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertiesChanged(string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    #endregion BindingProxy

    public class LabToneOfColor : IValueConverter
    {
        public static LabToneOfColor Instance = new LabToneOfColor();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                var color = ((SolidColorBrush)value).Color;
                var lab = ColorUtilities.ColorToLab(color);
                var l = lab.Item1 / 100;
                var isDelta = true;
                var k = 0.3;
                if (Equals(parameter, "+75"))
                {

                }
                if (parameter is string sParam && !string.IsNullOrEmpty(sParam))
                {
                    isDelta = sParam.StartsWith("+");
                    k = (isDelta ? double.Parse(sParam.Substring(1)) : double.Parse(sParam)) / 100.0;
                }
                var level = isDelta ? l : 0.55;
                var newL = level < 0.5 ? (1.0 - level) * k + level : level * (1.0 - k);
                var newColor = ColorUtilities.LabToColor(newL * 100.0, lab.Item2, lab.Item3);
                return new SolidColorBrush(newColor);
            }
            return new SolidColorBrush(Colors.White);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BlackAndWhite : IValueConverter
    {
        public static BlackAndWhite Instance = new BlackAndWhite();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                var color = ((SolidColorBrush)value).Color;
                var isDelta = false;
                var k = 1.0;
                if (parameter is string sParam && !string.IsNullOrEmpty(sParam))
                {
                    isDelta = sParam.StartsWith("+");
                    k = (isDelta ? double.Parse(sParam.Substring(1)) : double.Parse(sParam)) / 100.0;
                }

                var grayLevel = (isDelta ? ColorUtilities.ColorToGrayLevel(color) : ColorUtilities.DarkSplit) / 255.0;
                var newGrayLevel = ColorUtilities.IsDarkColor(color) ? (1.0 - grayLevel) * k + grayLevel : grayLevel * (1.0 - k);
                var newGrayValue = System.Convert.ToByte(newGrayLevel * 255.0);
                Debug.Print($"B&W. IsDelta: {isDelta}, grayLevel: {grayLevel}, newGray: {newGrayLevel}");
                return new SolidColorBrush(Color.FromRgb(newGrayValue, newGrayValue, newGrayValue));
            }
            return new SolidColorBrush(Colors.White);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ToneOfColor : IValueConverter
    {
        public static ToneOfColor Instance = new ToneOfColor();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                var color = ((SolidColorBrush)value).Color;
                var hsl = ColorUtilities.ColorToHsl(color);
                var l = hsl.Item3;
                var isDelta = true;
                var k = 0.3;
                if (parameter is string sParam && !string.IsNullOrEmpty(sParam))
                {
                    isDelta = sParam.StartsWith("+");
                    k = (isDelta ? double.Parse(sParam.Substring(1)) : double.Parse(sParam)) / 100.0;
                }
                var level = isDelta ? l : 0.5;
                var newL = level < 0.5 ? (1.0 - level) * k + level : level * (1.0 - k);
                var newColor = ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL);
                // Debug.Print($"ToneOfColor. isDelta: {isDelta}, Level: {level}, HSL: {hsl}. NewL: {newL}");
                return new SolidColorBrush(newColor);
            }
            return new SolidColorBrush(Colors.White);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ToneOfColor2 : IValueConverter
    {
        public static ToneOfColor2 Instance = new ToneOfColor2();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                var color = ((SolidColorBrush)value).Color;
                var hsl = ColorUtilities.ColorToHsl(color);
                var l = hsl.Item3;
                var isDelta = true;
                var k = 0.3;
                if (parameter is string sParam && !string.IsNullOrEmpty(sParam))
                {
                    isDelta = sParam.StartsWith("+");
                    k = (isDelta ? double.Parse(sParam.Substring(1)) : double.Parse(sParam)) / 100.0;
                }
                var level = isDelta ? l : 0.5;
                var newL = level < 0.5 ? (1.0 - level) * k + level : level * (1.0 - k);
                var newColor = ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL);
                Debug.Print($"ToneOfColor. isDelta: {isDelta}, Level: {level}, HSL: {hsl}. NewL: {newL}");
                return new SolidColorBrush(newColor);
            }
            return new SolidColorBrush(Colors.White);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GetBrushHsl2 : IValueConverter
    {
        public static GetBrushHsl2 Instance = new GetBrushHsl2();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                var color = ((SolidColorBrush)value).Color;
                var hsl = ColorUtilities.ColorToHsl(color);
                var l = hsl.Item3;
                var newL = l >= 0.5 ? l - 0.3 : l + 0.3;
                if (Equals(parameter, "5"))
                    newL = l >= 0.5 ? l - 0.5 : l + 0.5;
                var newColor = ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL);
                Debug.Print($"GetBrushHsl2. HSL: {hsl}. NewL: {newL}");
                return new SolidColorBrush(newColor);
            }
            Debug.Print($"GetBrushHsl2. Transparent");
            return new SolidColorBrush(Colors.Transparent);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class TestBrush : IValueConverter
    {
        public static TestBrush Instance = new TestBrush();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                var color = ((SolidColorBrush)value).Color;
                var hsl = ColorUtilities.ColorToHsl(color);
                var newL = ColorConverterHelper.ConvertValue(hsl.Item3, parameter, null);
                if (newL.HasValue)
                {
                    var newColor = ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL.Value);
                    Debug.Print($"TestBrush: hsl={hsl}, p={parameter}, newColor={ColorUtilities.ColorToHsl(newColor)}");
                    return new SolidColorBrush(newColor);
                }
            }
            return new SolidColorBrush(Colors.White);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class TestBrushWithSplit : IValueConverter
    {
        public static TestBrushWithSplit Instance = new TestBrushWithSplit();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                var color = ((SolidColorBrush)value).Color;
                var hsl = ColorUtilities.ColorToHsl(color);
                var newL = ColorConverterHelper.ConvertValue(hsl.Item3, parameter, 0.5);
                if (newL.HasValue)
                {
                    var newColor = ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL.Value);
                    Debug.Print($"TestBrushWithSplit: hsl={hsl}, p={parameter}, newColor={ColorUtilities.ColorToHsl(newColor)}");
                    return new SolidColorBrush(newColor);
                }
            }
            return new SolidColorBrush(Colors.White);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ColorConverterHelper
    {
        public static double? ConvertValue(double value, object parameter, double? split)
        {
            /*
             value is double in range [0-1.0]
             format of parameter: [+ or -]NN[%] where NN is double between 0-100
             split is double in range [0-1.0] or null

            1. split = null ( no split):
             =NN[%] return = NN / 100
             [+]NN  return = value + NN / 100
             -NN    return = value - NN / 100
             [+]NN% return = value + (1 - value) * NN/100
             -NN%:  return = value - value * NN/100

            2. split has value:
             =NN[%] return = NN / 100

             if (value < split)
             [+]NN  return = value + NN / 100
             -NN    return = value - NN / 100
             [+]NN% return = value + (1 - value) * NN/100
             -NN%:  return = value - value * NN/100

             if (value >= split)
             [+]NN  return = value - NN / 100
             -NN    return = value + NN / 100
             [+]NN% return = value - value * NN/100
             -NN%:  return = value + (1 - value) * NN/100

             return: double in [0-1.0]  or null(if bad parameter)

             */
            if (parameter is string sParameter)
            {
                var isEqual = false;
                var isPlus = true;
                var isPercent = false;

                if (sParameter.StartsWith("="))
                    isEqual = true;
                else if (sParameter.StartsWith("+"))
                    sParameter = sParameter.Substring(1);
                else if (sParameter.StartsWith("-"))
                {
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

                    if (split.HasValue)
                    {
                        var isValueLess = value < split.Value;
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
                    ss = ((string) ((BindingProxy) value).Value).Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

                if (ss != null && ss.Length == 2)
                {
                    var h = double.Parse(ss[0], Tips.InvariantCulture);
                    var s = double.Parse(ss[1], Tips.InvariantCulture);
                    return new SolidColorBrush(ColorUtilities.HslToColor(h / 360, s / 100, newL / 100.0));
                }

                if (value is Brush brush)
                {
                    var hsl = ColorUtilities.ColorToHsl(Tips.GetColorFromBrush(brush));
                    return new SolidColorBrush(ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL / 100.0));
                }

                if (value is DependencyObject d)
                {
                    var color = Tips.GetActualBackgroundColor(d);
                    var hsl = ColorUtilities.ColorToHsl(color);
                    return new SolidColorBrush(ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL / 100.0));
                }
            }

            return new SolidColorBrush(Colors.Transparent);
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

            if (ss != null && ss.Length == 2)
                hsl = new Tuple<double, double, double>(double.Parse(ss[0], Tips.InvariantCulture), double.Parse(ss[1], Tips.InvariantCulture), 0);
            else if (value is Brush brush)
                hsl = ColorUtilities.ColorToHsl(Tips.GetColorFromBrush(brush));
            else if (value is DependencyObject d)
                hsl = ColorUtilities.ColorToHsl(Tips.GetActualBackgroundColor(d));

            if (hsl != null)
            {
                var newL = ColorConverterHelper.ConvertValue(hsl.Item3, parameter, _isSplit ? 0.5 : (double?)null);
                if (newL.HasValue)
                {
                    Debug.Print($"HslBrush. HSL: {hsl}. Parameter: {parameter}. NewL: {newL}");
                    return new SolidColorBrush(ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, newL.Value));
                }
            }

            Debug.Print($"HslBrush. HSL: {hsl}. Parameter: {parameter}. Transparent.");
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ColorBlackAndWhiteBrush : IValueConverter
    {
        public static ColorBlackAndWhiteBrush Instance = new ColorBlackAndWhiteBrush();
        public static ColorBlackAndWhiteBrush InstanceWithSplit = new ColorBlackAndWhiteBrush { _isSplit = true };

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
                var newGrayLevel = ColorConverterHelper.ConvertValue(oldGrayLevel, parameter, _isSplit ? ColorUtilities.DarkSplit/255.0 : (double?)null);
                if (newGrayLevel.HasValue)
                {
                    Debug.Print($"BlackAndWhite. GrayLevel: {oldGrayLevel}. Parameter: {parameter}. NewL: {newGrayLevel}");
                    var newGrayValue = System.Convert.ToByte(newGrayLevel * 255.0);
                    return new SolidColorBrush(Color.FromRgb(newGrayValue, newGrayValue, newGrayValue));
                }
            }

            Debug.Print($"BlackAndWhite. Transparent");
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class TestConverter : IValueConverter
    {
        public static TestConverter Instance = new TestConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // return value;
            return new SolidColorBrush(Colors.Red);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
