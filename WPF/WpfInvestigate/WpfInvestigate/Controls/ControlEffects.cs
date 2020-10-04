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

        #region ===============  Chrome effects  ===============

        #region ================  Monochrome  ======================
        public static readonly DependencyProperty MonochromeProperty = DependencyProperty.RegisterAttached(
            "Monochrome", typeof(Brush), typeof(ControlEffects), new UIPropertyMetadata(null, OnChromeChanged));
        public static Brush GetMonochrome(DependencyObject obj) => (Brush)obj.GetValue(MonochromeProperty);
        public static void SetMonochrome(DependencyObject obj, Brush value) => obj.SetValue(MonochromeProperty, value);
        #endregion

        #region ================  Monochrome Animated ======================
        public static readonly DependencyProperty MonochromeAnimatedProperty = DependencyProperty.RegisterAttached(
            "MonochromeAnimated", typeof(Brush), typeof(ControlEffects), new UIPropertyMetadata(null, OnChromeChanged));
        public static Brush GetMonochromeAnimated(DependencyObject obj) => (Brush)obj.GetValue(MonochromeAnimatedProperty);
        public static void SetMonochromeAnimated(DependencyObject obj, Brush value) => obj.SetValue(MonochromeAnimatedProperty, value);
        #endregion

        #region =============================  Bichrome  ===========================
        public static readonly DependencyProperty BichromeBackgroundProperty = DependencyProperty.RegisterAttached(
            "BichromeBackground", typeof(Brush), typeof(ControlEffects), new UIPropertyMetadata(null, OnChromeChanged));
        public static Brush GetBichromeBackground(DependencyObject obj) => (Brush)obj.GetValue(BichromeBackgroundProperty);
        public static void SetBichromeBackground(DependencyObject obj, Brush value) => obj.SetValue(BichromeBackgroundProperty, value);

        public static readonly DependencyProperty BichromeForegroundProperty = DependencyProperty.RegisterAttached(
            "BichromeForeground", typeof(Brush), typeof(ControlEffects), new FrameworkPropertyMetadata(null, OnChromeChanged));
        public static Brush GetBichromeForeground(DependencyObject obj) => (Brush)obj.GetValue(BichromeForegroundProperty);
        public static void SetBichromeForeground(DependencyObject obj, Brush value) => obj.SetValue(BichromeForegroundProperty, value);
        #endregion

        #region =============================  Bichrome Animated ===========================
        public static readonly DependencyProperty BichromeAnimatedBackgroundProperty = DependencyProperty.RegisterAttached(
            "BichromeAnimatedBackground", typeof(Brush), typeof(ControlEffects), new UIPropertyMetadata(null, OnChromeChanged));
        public static Brush GetBichromeAnimatedBackground(DependencyObject obj) => (Brush)obj.GetValue(BichromeAnimatedBackgroundProperty);
        public static void SetBichromeAnimatedBackground(DependencyObject obj, Brush value) => obj.SetValue(BichromeAnimatedBackgroundProperty, value);

        public static readonly DependencyProperty BichromeAnimatedForegroundProperty = DependencyProperty.RegisterAttached(
            "BichromeAnimatedForeground", typeof(Brush), typeof(ControlEffects), new FrameworkPropertyMetadata(null, OnChromeChanged));
        public static Brush GetBichromeAnimatedForeground(DependencyObject obj) => (Brush)obj.GetValue(BichromeAnimatedForegroundProperty);
        public static void SetBichromeAnimatedForeground(DependencyObject obj, Brush value) => obj.SetValue(BichromeAnimatedForegroundProperty, value);
        #endregion

        #region ====================  Chrome common methods  ======================
        private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Control control)) return;

            OnChromeUnloaded(control, null);

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                control.Unloaded += OnChromeUnloaded;
                var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
                dpd.AddValueChanged(control, ChromeUpdate);
                dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
                dpd.AddValueChanged(control, ChromeUpdate);
                dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                dpd.AddValueChanged(control, ChromeUpdate);
            }));

            ChromeUpdate(control, null);
        }

        private static void OnChromeUnloaded(object sender, RoutedEventArgs e)
        {
            var control = (Control)sender;
            control.Unloaded -= OnChromeUnloaded;
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, ChromeUpdate);
            dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, ChromeUpdate);
            dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
            dpd.RemoveValueChanged(control, ChromeUpdate);
        }

        private static Tuple<Brush, Brush, Brush, Brush, Brush, Brush, Tuple<bool, bool, bool>> Chrome_GetState(Control control)
        {
            return new Tuple<Brush, Brush, Brush, Brush, Brush, Brush, Tuple<bool, bool, bool>>(GetMonochrome(control),
                GetMonochromeAnimated(control), GetBichromeBackground(control), GetBichromeForeground(control),
                GetBichromeAnimatedBackground(control), GetBichromeAnimatedForeground(control),
                new Tuple<bool, bool, bool>(control.IsMouseOver, control.IsEnabled,
                    (control as ButtonBase)?.IsPressed ?? false));
        }

        private static void ChromeUpdate(object sender, EventArgs e)
        {
            if (!(sender is Control control)) return;

            // To prevent: the same property changes multiple times
            var oldState = control.Resources["state"];
            var newState = Chrome_GetState(control);
            if (Equals(oldState, newState)) return;
            control.Resources["state"] = newState;

            var getBackgroundMethod = Chrome_GetBackgroundMethod(control);
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

        private static Tuple<Color, Color, Color, double> Chrome_GetNewColors(Control control, Func<DependencyObject, Brush> getBackgroundMethod)
        {
            var isMouseOver = control.IsMouseOver;
            var isPressed = (control as ButtonBase)?.IsPressed ?? false;
            var backColor = Tips.GetColorFromBrush(getBackgroundMethod(control));
            Color foreColor, borderColor;
            double opacity;

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
                var biChromeForeColor = Tips.GetColorFromBrush(getBackgroundMethod == GetBichromeBackground ? GetBichromeForeground(control) : GetBichromeAnimatedForeground(control));

                opacity = control.IsEnabled ? (isMouseOver || isPressed ? 1.0 : 0.7) : 0.35;
                if (isPressed)
                {
                    backColor = Chrome_ColorMix(biChromeBackColor, biChromeForeColor, 0.5);
                    foreColor = biChromeForeColor;
                    borderColor = Chrome_ColorMix(biChromeBackColor, biChromeForeColor, 0.5);
                }
                else if (isMouseOver)
                {
                    backColor = Chrome_ColorMix(biChromeBackColor, biChromeForeColor, 0.9);
                    foreColor = biChromeForeColor;
                    borderColor = Chrome_ColorMix(biChromeBackColor, biChromeForeColor, 0.5);
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

        private static Color Chrome_ColorMix(Color color1, Color color2, double multiplier)
        {
            var multiplier2 = 1.0 - multiplier;
            return Color.FromArgb(Convert.ToByte(color1.A * multiplier + color2.A * multiplier2),
                Convert.ToByte(color1.R * multiplier + color2.R * multiplier2),
                Convert.ToByte(color1.G * multiplier + color2.G * multiplier2),
                Convert.ToByte(color1.B * multiplier + color2.B * multiplier2));
        }

        private static Func<DependencyObject, Brush> Chrome_GetBackgroundMethod(Control control)
        {
            if (GetMonochrome(control) != null) return GetMonochrome;
            if (GetMonochromeAnimated(control) != null) return GetMonochromeAnimated;
            if (GetBichromeBackground(control) != null) return GetBichromeBackground;
            return GetBichromeAnimatedBackground;
        }
        #endregion

        #endregion
    }
}
