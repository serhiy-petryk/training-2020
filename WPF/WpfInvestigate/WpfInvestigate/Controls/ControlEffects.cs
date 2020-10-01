using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// </summary>
    public class ControlEffects
    {
        #region ================  PathData  ==========================
        public static readonly DependencyProperty PathDataProperty = DependencyProperty.RegisterAttached(
            "PathData", typeof(Geometry), typeof(ControlEffects), new UIPropertyMetadata(null, OnPathDataChanged));
        public static Geometry GetPathData(DependencyObject obj) => (Geometry)obj.GetValue(PathDataProperty);
        public static void SetPathData(DependencyObject obj, Geometry value) => obj.SetValue(PathDataProperty, value);

        private static void OnPathDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ContentControl control)
            {
                if (e.NewValue is Geometry geometry)
                {
                    var path = new Path { Stretch = Stretch.Uniform, Data = geometry };
                    var b = new Binding("Foreground")
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Control), 1)
                    };
                    path.SetBinding(Shape.FillProperty, b);
                    control.Content = new Viewbox { Child = path };
                }
                else
                    control.Content = null;
            }
        }
        #endregion

        #region ================  CornerRadius  =======================
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius", typeof(double), typeof(ControlEffects), new UIPropertyMetadata(double.NaN, OnCornerRadiusChanged));
        public static double GetCornerRadius(DependencyObject obj) => (double)obj.GetValue(CornerRadiusProperty);
        public static void SetCornerRadius(DependencyObject obj, double value) => obj.SetValue(CornerRadiusProperty, value);
        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newRadius = (double)e.NewValue;
            if (!double.IsNaN(newRadius) && newRadius >= -0.0001)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    var border = Tips.GetVisualChildren(d).OfType<Border>().FirstOrDefault();
                    if (border != null)
                        border.CornerRadius = new CornerRadius(newRadius);
                }));
            }
        }
        #endregion

        #region ================  Monochrome  ======================
        public static readonly DependencyProperty MonochromeProperty = DependencyProperty.RegisterAttached(
            "Monochrome", typeof(Brush), typeof(ControlEffects), new UIPropertyMetadata(null, OnMonochromeChanged));
        public static Brush GetMonochrome(DependencyObject obj) => (Brush)obj.GetValue(MonochromeProperty);
        public static void SetMonochrome(DependencyObject obj, Brush value) => obj.SetValue(MonochromeProperty, value);
        private static void OnMonochromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                MonochromeRemoveEvents(control);

                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty,
                        typeof(UIElement));
                    dpd.AddValueChanged(control, MonochromeUpdate);
                    dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
                    dpd.AddValueChanged(control, MonochromeUpdate);
                    dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                    dpd.AddValueChanged(control, MonochromeUpdate);
                }));

                MonochromeUpdate(control, null);
            }
        }

        private static void MonochromeRemoveEvents(Control control)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, MonochromeUpdate);
            dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, MonochromeUpdate);
            dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
            dpd.RemoveValueChanged(control, MonochromeUpdate);
        }
        private static void MonochromeUpdate(object sender, EventArgs e)
        {
            if (sender is Control control)
            {
                var newValues = Monochrome_GetNewColors(control, false);
                control.Background = new SolidColorBrush(newValues.Item1);
                control.Foreground = new SolidColorBrush(newValues.Item2);
                control.BorderBrush = new SolidColorBrush(newValues.Item3);
                control.Opacity = newValues.Item4;
            }
        }

        private static Tuple<Color, Color, Color, double> Monochrome_GetNewColors(Control control, bool isAnimated)
        {
            var isMouseOver = control.IsMouseOver;
            var isPressed = (control as ButtonBase)?.IsPressed ?? false;
            var backColor = Tips.GetColorFromBrush(isAnimated ? GetMonochromeAnimated(control) : GetMonochrome(control));
            if (isPressed)
                backColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+60%", null);
            else if (isMouseOver)
                backColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+20%", null);

            var foreColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+75%", null);
            var borderColor = isPressed || isMouseOver ? (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+30%", null) : backColor;

            var opacity = control.IsEnabled ? 1.0 : 0.4;

            if (isAnimated)
            {
                if (!(control.Background is SolidColorBrush && !((SolidColorBrush)control.Background).IsSealed))
                    control.Background = new SolidColorBrush(backColor);
                if (!(control.Foreground is SolidColorBrush && !((SolidColorBrush)control.Foreground).IsSealed))
                    control.Foreground = new SolidColorBrush(foreColor);
                if (!(control.BorderBrush is SolidColorBrush && !((SolidColorBrush)control.BorderBrush).IsSealed))
                    control.BorderBrush = new SolidColorBrush(borderColor);
            }

            return new Tuple<Color, Color, Color, double>(backColor, foreColor, borderColor, opacity);
        }

        #endregion

        #region ================  Monochrome Animated ======================
        public static readonly DependencyProperty MonochromeAnimatedProperty = DependencyProperty.RegisterAttached(
            "MonochromeAnimated", typeof(Brush), typeof(ControlEffects), new UIPropertyMetadata(null, OnMonochromeAnimatedChanged));
        public static Brush GetMonochromeAnimated(DependencyObject obj) => (Brush)obj.GetValue(MonochromeAnimatedProperty);
        public static void SetMonochromeAnimated(DependencyObject obj, Brush value) => obj.SetValue(MonochromeAnimatedProperty, value);
        private static void OnMonochromeAnimatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                MonochromeAnimatedRemoveEvents(control);

                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty,
                        typeof(UIElement));
                    dpd.AddValueChanged(control, MonochromeAnimatedUpdate);
                    dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
                    dpd.AddValueChanged(control, MonochromeAnimatedUpdate);
                    dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                    dpd.AddValueChanged(control, MonochromeAnimatedUpdate);
                }));

                MonochromeAnimatedUpdate(control, null);
            }
        }
        private static void MonochromeAnimatedRemoveEvents(Control control)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, MonochromeAnimatedUpdate);
            dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, MonochromeAnimatedUpdate);
            dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
            dpd.RemoveValueChanged(control, MonochromeAnimatedUpdate);
        }
        private static void MonochromeAnimatedUpdate(object sender, EventArgs e)
        {
            if (sender is Control control)
            {
                var newValues = Monochrome_GetNewColors(control, true);
                AnimateSolidColorBrush((SolidColorBrush)control.Background, newValues.Item1);
                AnimateSolidColorBrush((SolidColorBrush)control.Foreground, newValues.Item2);
                AnimateSolidColorBrush((SolidColorBrush)control.BorderBrush, newValues.Item3);

                if (!Tips.AreEqual(control.Opacity, newValues.Item4))
                {
                    var animation = new DoubleAnimation { From = control.Opacity, To = newValues.Item4, Duration = AnimationHelper.SlowAnimationDuration };
                    control.BeginAnimation(UIElement.OpacityProperty, animation);
                }
            }
        }
        private static void AnimateSolidColorBrush(SolidColorBrush brush, Color newColor)
        {
            if (brush.Color == newColor)
                return;
            var animation = new ColorAnimation { From = brush.Color, To = newColor, Duration = Common.AnimationHelper.SlowAnimationDuration };
            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }
        #endregion

        // =============================  Bichrome  ===========================
        public static readonly DependencyProperty BichromeBackgroundProperty = DependencyProperty.RegisterAttached(
            "BichromeBackground", typeof(Brush), typeof(ControlEffects), new UIPropertyMetadata(null, OnBichromeChanged));
        public static Brush GetBichromeBackground(DependencyObject obj) => (Brush)obj.GetValue(BichromeBackgroundProperty);
        public static void SetBichromeBackground(DependencyObject obj, Brush value) => obj.SetValue(BichromeBackgroundProperty, value);

        public static readonly DependencyProperty BichromeForegroundProperty = DependencyProperty.RegisterAttached(
            "BichromeForeground", typeof(Brush), typeof(ControlEffects), new FrameworkPropertyMetadata(null, OnBichromeChanged));
        public static Brush GetBichromeForeground(DependencyObject obj) => (Brush)obj.GetValue(BichromeForegroundProperty);
        public static void SetBichromeForeground(DependencyObject obj, Brush value) => obj.SetValue(BichromeForegroundProperty, value);
        private static void OnBichromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                BichromeRemoveEvents(control);

                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty,
                        typeof(UIElement));
                    dpd.AddValueChanged(control, BichromeUpdate);
                    dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
                    dpd.AddValueChanged(control, BichromeUpdate);
                    dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                    dpd.AddValueChanged(control, BichromeUpdate);
                }));

                BichromeUpdate(control, null);
            }
        }
        private static void BichromeRemoveEvents(Control control)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, BichromeUpdate);
            dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, BichromeUpdate);
            dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
            dpd.RemoveValueChanged(control, BichromeUpdate);
        }

        private static void BichromeUpdate(object sender, EventArgs e)
        {
            if (sender is Control control)
            {
                var newValues = Bichrome_GetNewColors(control);
                control.Background = new SolidColorBrush(newValues.Item1);
                control.Foreground = new SolidColorBrush(newValues.Item2);
                control.BorderBrush = new SolidColorBrush(newValues.Item3);
                control.Opacity = newValues.Item4;
            }
        }

        private static Tuple<Color, Color, Color, double> Bichrome_GetNewColors(Control control)
        {
            var isMouseOver = control.IsMouseOver;
            var isPressed = (control as ButtonBase)?.IsPressed ?? false;
            var biChromeBackColor = Tips.GetColorFromBrush(GetBichromeBackground(control));
            var biChromeForeColor = Tips.GetColorFromBrush(GetBichromeForeground(control));

            Color backColor, foreColor, borderColor;
            var opacity = 0.7;

            if (isPressed)
            {
                backColor = biChromeForeColor;
                foreColor = biChromeBackColor;
                borderColor = biChromeForeColor;
            }
            else if (isMouseOver)
            {
                backColor = ColorMix(biChromeBackColor, biChromeForeColor, 0.75);
                foreColor = biChromeForeColor;
                borderColor = ColorMix(biChromeBackColor, biChromeForeColor, 0.5);
                opacity = 1.0;
            }
            else
            {
                backColor = biChromeBackColor;
                foreColor = biChromeForeColor;
                borderColor = biChromeBackColor;
            }
            return new Tuple<Color, Color, Color, double>(backColor, foreColor, borderColor, control.IsEnabled ? opacity : 0.35);
        }

        private static Color ColorMix(Color color1, Color color2, double multiplier)
        {
            var multiplier2 = 1.0 - multiplier;
            return Color.FromArgb(Convert.ToByte(color1.A * multiplier + color2.A * multiplier2),
                Convert.ToByte(color1.R * multiplier + color2.R * multiplier2),
                Convert.ToByte(color1.G * multiplier + color2.G * multiplier2),
                Convert.ToByte(color1.B * multiplier + color2.B * multiplier2));
        }
    }
}
