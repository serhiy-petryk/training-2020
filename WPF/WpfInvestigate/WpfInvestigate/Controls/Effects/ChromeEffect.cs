using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls.Effects
{
    /// <summary>
    /// </summary>
    public class ChromeEffect
    {
        #region ================  Chrome Settings  ======================
        public static readonly DependencyProperty ChromeMatrixProperty = DependencyProperty.RegisterAttached(
            "ChromeMatrix", typeof(string), typeof(ChromeEffect), new UIPropertyMetadata(null, OnChromeChanged));
        public static string GetChromeMatrix(DependencyObject obj) => (string)obj.GetValue(ChromeMatrixProperty);
        public static void SetChromeMatrix(DependencyObject obj, string value) => obj.SetValue(ChromeMatrixProperty, value);
        #endregion

        #region ================  Monochrome  ======================
        public static readonly DependencyProperty MonochromeProperty = DependencyProperty.RegisterAttached(
            "Monochrome", typeof(Color?), typeof(ChromeEffect), new UIPropertyMetadata(null, OnChromeChanged));
        public static Color? GetMonochrome(DependencyObject obj) => (Color?)obj.GetValue(MonochromeProperty);
        public static void SetMonochrome(DependencyObject obj, Color? value) => obj.SetValue(MonochromeProperty, value);
        #endregion

        #region ================  Monochrome Animated ======================
        public static readonly DependencyProperty MonochromeAnimatedProperty = DependencyProperty.RegisterAttached(
            "MonochromeAnimated", typeof(Color?), typeof(ChromeEffect), new UIPropertyMetadata(null, OnChromeChanged));
        public static Color? GetMonochromeAnimated(DependencyObject obj) => (Color?)obj.GetValue(MonochromeAnimatedProperty);
        public static void SetMonochromeAnimated(DependencyObject obj, Color? value) => obj.SetValue(MonochromeAnimatedProperty, value);
        #endregion

        #region =============================  Bichrome  ===========================
        public static readonly DependencyProperty BichromeBackgroundProperty = DependencyProperty.RegisterAttached(
            "BichromeBackground", typeof(Color?), typeof(ChromeEffect), new UIPropertyMetadata(null, OnChromeChanged));
        public static Color? GetBichromeBackground(DependencyObject obj) => (Color?)obj.GetValue(BichromeBackgroundProperty);
        public static void SetBichromeBackground(DependencyObject obj, Color? value) => obj.SetValue(BichromeBackgroundProperty, value);

        public static readonly DependencyProperty BichromeForegroundProperty = DependencyProperty.RegisterAttached(
            "BichromeForeground", typeof(Color?), typeof(ChromeEffect), new FrameworkPropertyMetadata(null, OnChromeChanged));
        public static Color? GetBichromeForeground(DependencyObject obj) => (Color?)obj.GetValue(BichromeForegroundProperty);
        public static void SetBichromeForeground(DependencyObject obj, Color? value) => obj.SetValue(BichromeForegroundProperty, value);
        #endregion

        #region =============================  Bichrome Animated ===========================
        public static readonly DependencyProperty BichromeAnimatedBackgroundProperty = DependencyProperty.RegisterAttached(
            "BichromeAnimatedBackground", typeof(Color?), typeof(ChromeEffect), new UIPropertyMetadata(null, OnChromeChanged));
        public static Color? GetBichromeAnimatedBackground(DependencyObject obj) => (Color?)obj.GetValue(BichromeAnimatedBackgroundProperty);
        public static void SetBichromeAnimatedBackground(DependencyObject obj, Color? value) => obj.SetValue(BichromeAnimatedBackgroundProperty, value);

        public static readonly DependencyProperty BichromeAnimatedForegroundProperty = DependencyProperty.RegisterAttached(
            "BichromeAnimatedForeground", typeof(Color?), typeof(ChromeEffect), new FrameworkPropertyMetadata(null, OnChromeChanged));
        public static Color? GetBichromeAnimatedForeground(DependencyObject obj) => (Color?)obj.GetValue(BichromeAnimatedForegroundProperty);
        public static void SetBichromeAnimatedForeground(DependencyObject obj, Color? value) => obj.SetValue(BichromeAnimatedForegroundProperty, value);
        #endregion

        #region ====================  Chrome common methods  ======================
        private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Control control)) return;

            OnChromeUnloaded(control, null);

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                if (control.Style == null)
                {
                    var style = Application.Current.TryFindResource("DefaultButtonBaseStyle") as Style;
                    if (style != null && style.TargetType.IsInstanceOfType(control))
                        control.Style = style;
                }
                control.Unloaded += OnChromeUnloaded;
                control.PreviewMouseLeftButtonDown += ChromeUpdate;
                control.PreviewMouseLeftButtonUp += ChromeUpdate;
                var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
                dpd.AddValueChanged(control, ChromeUpdate);
                dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
                dpd.AddValueChanged(control, ChromeUpdate);

                ChromeUpdate(control, null);
            }));
        }

        private static void OnChromeUnloaded(object sender, RoutedEventArgs e)
        {
            var control = (Control)sender;
            control.Unloaded -= OnChromeUnloaded;
            control.PreviewMouseLeftButtonDown -= ChromeUpdate;
            control.PreviewMouseLeftButtonUp -= ChromeUpdate;
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, ChromeUpdate);
            dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, ChromeUpdate);
        }

        private static Tuple<Color?, Color?, Color?, Color?, Color?, Color?, Tuple<bool, bool, bool>> Chrome_GetState(Control control)
        {
            return new Tuple<Color?, Color?, Color?, Color?, Color?, Color?, Tuple<bool, bool, bool>>(GetMonochrome(control),
                GetMonochromeAnimated(control), GetBichromeBackground(control), GetBichromeForeground(control),
                GetBichromeAnimatedBackground(control), GetBichromeAnimatedForeground(control),
                new Tuple<bool, bool, bool>(control.IsMouseOver, control.IsEnabled, Mouse.LeftButton == MouseButtonState.Pressed));
        }

        private static void ChromeUpdate(object sender, EventArgs e)
        {
            if (!(sender is Control control)) return;

            // To prevent: the same property changes multiple times
            var oldState = control.Resources["state"];
            var newState = Chrome_GetState(control);
            if (Equals(oldState, newState)) return;
            control.Resources["state"] = newState;

            var getBackgroundMethod = GetBackgroundMethod(control);
            var newValues = Chrome_GetNewColors(control, getBackgroundMethod);

            if (!(control.Background is SolidColorBrush && !((SolidColorBrush)control.Background).IsSealed))
                control.Background = new SolidColorBrush(newValues.Item1);
            if (!(control.Foreground is SolidColorBrush && !((SolidColorBrush)control.Foreground).IsSealed))
                control.Foreground = new SolidColorBrush(newValues.Item2);
            if (!(control.BorderBrush is SolidColorBrush && !((SolidColorBrush)control.BorderBrush).IsSealed))
                control.BorderBrush = new SolidColorBrush(newValues.Item3);

            var noAnimate = getBackgroundMethod == GetMonochrome || getBackgroundMethod == GetBichromeBackground;
            if (noAnimate)
            {
                if (((SolidColorBrush)control.Background).Color != newValues.Item1)
                    ((SolidColorBrush)control.Background).Color = newValues.Item1;
                if (((SolidColorBrush)control.Foreground).Color != newValues.Item2)
                    ((SolidColorBrush)control.Foreground).Color = newValues.Item2;
                if (((SolidColorBrush)control.BorderBrush).Color != newValues.Item3)
                    ((SolidColorBrush)control.BorderBrush).Color = newValues.Item3;
                if (!Tips.AreEqual(control.Opacity, newValues.Item4))
                    control.Opacity = newValues.Item4;
            }
            else
            {
                var sb = control.Resources["animation"] as Storyboard;
                if (sb == null)
                {
                    sb = new Storyboard();
                    control.Resources["animation"] = sb;
                    sb.Children.Add(control.CreateAnimation("(Border.Background).(SolidColorBrush.Color)", typeof(Color), AnimationHelper.SlowAnimationDuration));
                    sb.Children.Add(control.CreateAnimation("(Control.Foreground).(SolidColorBrush.Color)", typeof(Color), AnimationHelper.SlowAnimationDuration));
                    sb.Children.Add(control.CreateAnimation("(Border.BorderBrush).(SolidColorBrush.Color)", typeof(Color), AnimationHelper.SlowAnimationDuration));
                    sb.Children.Add(control.CreateAnimation(UIElement.OpacityProperty));
                }

                sb.Children[0].SetFromToValues(((SolidColorBrush)control.Background).Color, newValues.Item1);
                sb.Children[1].SetFromToValues(((SolidColorBrush)control.Foreground).Color, newValues.Item2);
                sb.Children[2].SetFromToValues(((SolidColorBrush)control.BorderBrush).Color, newValues.Item3);
                sb.Children[3].SetFromToValues(control.Opacity, newValues.Item4);

                sb.Begin();
            }
        }

        private static Tuple<Color, Color, Color, double> GetNewColors(Control control, Func<DependencyObject, Color?> getBackgroundMethod)
        {
            var isMouseOver = control.IsMouseOver;
            var isPressed = Mouse.LeftButton == MouseButtonState.Pressed;
            var backColor = getBackgroundMethod(control) ?? Colors.Transparent;

            var a1 = GetChromeMatrix(control);
            var matrix = a1.Split(',');

            var opacity = control.IsEnabled ? 1.0 : ColorConverterHelper.ConvertValue(1.0, matrix[9].Trim());
            var startIndex = isPressed ? 6 : (isMouseOver? 3 : 0);
            var newBackColor = (Color)ColorHslBrush.NoSplit.Convert(backColor, typeof(Color), matrix[startIndex].Trim(), null);
            var foreColor = (Color)ColorHslBrush.NoSplit.Convert(backColor, typeof(Color), matrix[startIndex + 1].Trim(), null);
            var borderColor = (Color)ColorHslBrush.NoSplit.Convert(backColor, typeof(Color), matrix[startIndex + 2].Trim(), null);

            return new Tuple<Color, Color, Color, double>(newBackColor, foreColor, borderColor, opacity);
        }
        private static Tuple<Color, Color, Color, double> Chrome_GetNewColors(Control control, Func<DependencyObject, Color?> getBackgroundMethod)
        {
            var isMouseOver = control.IsMouseOver;
            var isPressed = Mouse.LeftButton == MouseButtonState.Pressed;
            var backColor = getBackgroundMethod(control) ?? Colors.Transparent;
            Color foreColor, borderColor;
            double opacity;

            var a1 = GetChromeMatrix(control);
            if (a1 != null)
                return GetNewColors(control, getBackgroundMethod);

            if (getBackgroundMethod == GetMonochrome || getBackgroundMethod == GetMonochromeAnimated)
            {
                if (isPressed)
                    backColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+60%", null);
                else if (isMouseOver)
                    backColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+20%", null);

                foreColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+75%", null);
                borderColor = isPressed || isMouseOver
                    ? (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+30%", null)
                    : backColor;
                opacity = control.IsEnabled ? 1.0 : 0.4;
            }
            else
            {
                var biChromeBackColor = backColor;
                var biChromeForeColor = (getBackgroundMethod == GetBichromeBackground ? GetBichromeForeground(control) : GetBichromeAnimatedForeground(control)) ?? Colors.Transparent;

                opacity = control.IsEnabled ? (isMouseOver || isPressed ? 1.0 : 0.7) : 0.35;
                if (isPressed)
                {
                    backColor = ColorMix(biChromeBackColor, biChromeForeColor, 0.5);
                    foreColor = biChromeForeColor;
                    borderColor = ColorMix(biChromeBackColor, biChromeForeColor, 0.5);
                }
                else if (isMouseOver)
                {
                    backColor = ColorMix(biChromeBackColor, biChromeForeColor, 0.9);
                    foreColor = biChromeForeColor;
                    borderColor = ColorMix(biChromeBackColor, biChromeForeColor, 0.5);
                }
                else
                {
                    backColor = biChromeBackColor;
                    foreColor = biChromeForeColor;
                    borderColor = biChromeBackColor;
                }
            }
            return new Tuple<Color, Color, Color, double>(backColor, foreColor, borderColor, opacity);
        }

        private static Color ColorMix(Color color1, Color color2, double multiplier)
        {
            var multiplier2 = 1.0 - multiplier;
            return Color.FromArgb(Convert.ToByte(color1.A * multiplier + color2.A * multiplier2),
                Convert.ToByte(color1.R * multiplier + color2.R * multiplier2),
                Convert.ToByte(color1.G * multiplier + color2.G * multiplier2),
                Convert.ToByte(color1.B * multiplier + color2.B * multiplier2));
        }

        private static Func<DependencyObject, Color?> GetBackgroundMethod(Control control)
        {
            if (GetMonochrome(control) != null) return GetMonochrome;
            if (GetMonochromeAnimated(control) != null) return GetMonochromeAnimated;
            if (GetBichromeBackground(control) != null) return GetBichromeBackground;
            return GetBichromeAnimatedBackground;
        }
        #endregion

    }
}
