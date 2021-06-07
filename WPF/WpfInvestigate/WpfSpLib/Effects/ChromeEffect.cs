using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Helpers;

namespace WpfSpLib.Effects
{
    /// <summary>
    /// </summary>
    public class ChromeEffect
    {
        #region ================  Chrome Settings  ======================
        public static readonly DependencyProperty ChromeMatrixProperty = DependencyProperty.RegisterAttached(
            "ChromeMatrix", typeof(string), typeof(ChromeEffect), new FrameworkPropertyMetadata(null, OnChromeChanged));
        public static string GetChromeMatrix(DependencyObject obj) => (string)obj.GetValue(ChromeMatrixProperty);
        public static void SetChromeMatrix(DependencyObject obj, string value) => obj.SetValue(ChromeMatrixProperty, value);
        #endregion

        #region ================  Monochrome Animated ======================
        public static readonly DependencyProperty MonochromeProperty = DependencyProperty.RegisterAttached(
            "Monochrome", typeof(Color?), typeof(ChromeEffect), new FrameworkPropertyMetadata(null, OnChromeChanged));
        public static Color? GetMonochrome(DependencyObject obj) => (Color?)obj.GetValue(MonochromeProperty);
        public static void SetMonochrome(DependencyObject obj, Color? value) => obj.SetValue(MonochromeProperty, value);
        #endregion

        #region =============================  Bichrome Animated ===========================
        public static readonly DependencyProperty BichromeBackgroundProperty = DependencyProperty.RegisterAttached(
            "BichromeBackground", typeof(Color?), typeof(ChromeEffect), new FrameworkPropertyMetadata(null, OnChromeChanged));
        public static Color? GetBichromeBackground(DependencyObject obj) => (Color?)obj.GetValue(BichromeBackgroundProperty);
        public static void SetBichromeBackground(DependencyObject obj, Color? value) => obj.SetValue(BichromeBackgroundProperty, value);

        public static readonly DependencyProperty BichromeForegroundProperty = DependencyProperty.RegisterAttached(
            "BichromeForeground", typeof(Color?), typeof(ChromeEffect), new FrameworkPropertyMetadata(null, OnChromeChanged));
        public static Color? GetBichromeForeground(DependencyObject obj) => (Color?)obj.GetValue(BichromeForegroundProperty);
        public static void SetBichromeForeground(DependencyObject obj, Color? value) => obj.SetValue(BichromeForegroundProperty, value);
        #endregion

        #region ====================  Chrome common methods  ======================
        private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Control control)) return;

            ClearEvents(control);

            var state = GetState(control);
            if (!(state.Item1.HasValue || (state.Item2.HasValue && state.Item3.HasValue)))
                return;

            var isInitialized = control.IsInitialized;
            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                if (!isInitialized || !control.IsElementDisposing())
                {
                    control.PreviewMouseLeftButtonDown += ChromeUpdate;
                    control.PreviewMouseLeftButtonUp += ChromeUpdate;
                    var dpdIsMouseOver = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
                    dpdIsMouseOver.AddValueChanged(control, ChromeUpdate);
                    var dpdIsEnabled = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
                    dpdIsEnabled.AddValueChanged(control, ChromeUpdate);
                    control.Unloaded += OnControlUnloaded;

                    if (control.Style == null && Application.Current.TryFindResource("DefaultButtonBaseStyle") is Style style && style.TargetType.IsInstanceOfType(control))
                        control.Style = style;
                    ChromeUpdate(control, null);
                }
            }, DispatcherPriority.Loaded);
        }

        private static void ClearEvents(Control control)
        {
            control.Unloaded -= OnControlUnloaded;
            control.PreviewMouseLeftButtonDown -= ChromeUpdate;
            control.PreviewMouseLeftButtonUp -= ChromeUpdate;
            var dpdIsMouseOver = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
            dpdIsMouseOver.RemoveValueChanged(control, ChromeUpdate);
            var dpdIsEnabled = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
            dpdIsEnabled.RemoveValueChanged(control, ChromeUpdate);
        }

        private static void OnControlUnloaded(object sender, RoutedEventArgs e)
        {
            var control = (Control)sender;
            if (control.IsElementDisposing())
                ClearEvents(control);
        }

        private static Tuple<Color?, Color?, Color?, bool, bool, bool> GetState(Control control)
        {
            return new Tuple<Color?, Color?, Color?, bool, bool, bool>(
                GetMonochrome(control), GetBichromeBackground(control), GetBichromeForeground(control),
                control.IsMouseOver, control.IsEnabled, IsPressed(control));
        }

        private static readonly FieldInfo _fiSemaphoreLock= typeof(SemaphoreSlim).GetField("m_lockObj", BindingFlags.Instance | BindingFlags.NonPublic);
        private static async void ChromeUpdate(object sender, EventArgs e)
        {
            if (!(sender is Control control)) return;

            if (!(control.Resources["semaphore"] is SemaphoreSlim semaphore))
            {
                semaphore = new SemaphoreSlim(1, 1);
                control.Resources["semaphore"] = semaphore;
            }

            await semaphore.WaitAsync();

            try
            {
                var oldValues = new Tuple<Color?, Color?, Color?, double>((control.Background as SolidColorBrush)?.Color, (control.Foreground as SolidColorBrush)?.Color, (control.BorderBrush as SolidColorBrush)?.Color, control.Opacity);
                var newValues = GetNewColors(control, GetBackgroundMethod(control));
                if (newValues == null || Equals(oldValues, newValues) || !newValues.Item3.HasValue) return;

                if (!(control.Background is SolidColorBrush backgroundBrush && !backgroundBrush.IsSealed))
                    control.SetCurrentValueSmart(Control.BackgroundProperty, new SolidColorBrush(newValues.Item1.Value));
                if (!(control.Foreground is SolidColorBrush foregroundBrush && !foregroundBrush.IsSealed))
                    control.SetCurrentValueSmart(Control.ForegroundProperty, new SolidColorBrush(newValues.Item2.Value));
                if (!(control.BorderBrush is SolidColorBrush borderBrush && !borderBrush.IsSealed))
                    control.SetCurrentValueSmart(Control.BorderBrushProperty, new SolidColorBrush(newValues.Item3.Value));

                await Task.WhenAll(
                    control.Background.BeginAnimationAsync(SolidColorBrush.ColorProperty, ((SolidColorBrush)control.Background).Color, newValues.Item1.Value),
                    control.Foreground.BeginAnimationAsync(SolidColorBrush.ColorProperty, ((SolidColorBrush)control.Foreground).Color, newValues.Item2.Value),
                    control.BorderBrush.BeginAnimationAsync(SolidColorBrush.ColorProperty, ((SolidColorBrush)control.BorderBrush).Color, newValues.Item3.Value),
                    control.BeginAnimationAsync(UIElement.OpacityProperty, control.Opacity, newValues.Item4));
            }
            finally
            {
                if (_fiSemaphoreLock.GetValue(semaphore) != null) // not disposed: can be disposed when element is unloading (see UnloadingHelper)
                    semaphore.Release();
            }
        }

        private static Tuple<Color?, Color?, Color?, double> GetNewColors(Control control, Func<DependencyObject, Color?> getBackgroundMethod)
        {
            if (getBackgroundMethod == null) return null;

            var isPressed = IsPressed(control);
            if (isPressed && ClickEffect.GetRippleColor(control).HasValue)
                return new Tuple<Color?, Color?, Color?, double>(null, null,  null, 1.0);

            var backColor = getBackgroundMethod(control) ?? Colors.Transparent;
            Color? newBackColor = null, foreColor = null, borderColor = null;
            var opacity = 1.0;

            if (getBackgroundMethod == GetMonochrome)
            {   // Monochrome effect
                var matrixDefinition = GetChromeMatrix(control);
                if (string.IsNullOrEmpty(matrixDefinition))
                    matrixDefinition = "+0%,+70%,+0%,40, +0%,+70%,+0%,100, +25%,+25%/+75%,+25%/+50%,100, +60%,+60%/+75%,+60%/+50%,100";

                var matrix = matrixDefinition.Split(',');

                var startIndex = !control.IsEnabled ? 0 : (isPressed ? 12 : (control.IsMouseOver ? 8 : 4));
                if (matrix.Length > startIndex)
                    newBackColor = (Color?) ColorHslBrush.Instance.Convert(backColor, typeof(Color), matrix[startIndex].Trim(), null);
                if (matrix.Length > startIndex + 1)
                    foreColor = (Color?) ColorHslBrush.Instance.Convert(backColor, typeof(Color), matrix[startIndex + 1].Trim(), null);
                if (matrix.Length > startIndex + 2)
                    borderColor = (Color?) ColorHslBrush.Instance.Convert(backColor, typeof(Color), matrix[startIndex + 2].Trim(), null);
                if (matrix.Length > startIndex + 3)
                    if (double.TryParse(matrix[startIndex + 3], out var temp))
                        opacity = temp / 100;
            }
            else
            {   // Bichrome effect
                var biChromeBackColor = backColor;
                var biChromeForeColor = GetBichromeForeground(control) ?? Colors.Transparent;

                opacity = control.IsEnabled ? (control.IsMouseOver || isPressed ? 1.0 : 0.7) : 0.35;
                if (isPressed)
                {
                    newBackColor = ColorMix(biChromeBackColor, biChromeForeColor, 0.6);
                    foreColor = biChromeForeColor;
                    borderColor = ColorMix(biChromeBackColor, biChromeForeColor, 0.6);
                }
                else if (control.IsMouseOver)
                {
                    newBackColor = ColorMix(biChromeBackColor, biChromeForeColor, 0.8);
                    foreColor = biChromeForeColor;
                    borderColor = ColorMix(biChromeBackColor, biChromeForeColor, 0.5);
                }
                else
                {
                    newBackColor = biChromeBackColor;
                    foreColor = biChromeForeColor;
                    borderColor = biChromeBackColor;
                }
            }
            return new Tuple<Color?, Color?, Color?, double>(newBackColor, foreColor, borderColor, opacity);
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
            if (GetBichromeBackground(control) != null && GetBichromeForeground(control) != null) return GetBichromeBackground;
            return null;
        }

        private static bool IsPressed(Control control) => control.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed;

        #endregion
    }
}
