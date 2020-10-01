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
                    dpd.AddValueChanged(control, UpdateMonochrome);
                    dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
                    dpd.AddValueChanged(control, UpdateMonochrome);
                    dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                    dpd.AddValueChanged(control, UpdateMonochrome);
                }));

                UpdateMonochrome(control, null);
            }
        }

        private static void MonochromeRemoveEvents(Control control)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, UpdateMonochrome);
            dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, UpdateMonochrome);
            dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
            dpd.RemoveValueChanged(control, UpdateMonochrome);
        }
        private static void UpdateMonochrome(object sender, EventArgs e)
        {
            if (sender is Control control)
            {
                var isMouseOver = control.IsMouseOver;
                var isPressed = (control as ButtonBase)?.IsPressed ?? false;
                var backColor = Tips.GetColorFromBrush(GetMonochrome(control));
                if (isPressed)
                    backColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+60%", null);
                else if (isMouseOver)
                    backColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+20%", null);

                var foreColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+75%", null);
                var borderColor = isPressed || isMouseOver ? (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+30%", null) : backColor;

                control.Background = new SolidColorBrush(backColor);
                control.Foreground = new SolidColorBrush(foreColor);
                control.BorderBrush = new SolidColorBrush(borderColor);
                control.Opacity = control.IsEnabled ? 1.0 : 0.4;
            }
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
                    dpd.AddValueChanged(control, UpdateMonochromeAnimated);
                    dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
                    dpd.AddValueChanged(control, UpdateMonochromeAnimated);
                    dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                    dpd.AddValueChanged(control, UpdateMonochromeAnimated);
                }));

                UpdateMonochromeAnimated(control, null);
            }
        }
        private static void MonochromeAnimatedRemoveEvents(Control control)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, UpdateMonochromeAnimated);
            dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, UpdateMonochromeAnimated);
            dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
            dpd.RemoveValueChanged(control, UpdateMonochromeAnimated);
        }
        private static void UpdateMonochromeAnimated(object sender, EventArgs e)
        {
            if (sender is Control control)
            {
                var isMouseOver = control.IsMouseOver;
                var isPressed = (control as ButtonBase)?.IsPressed ?? false;
                var backColor = Tips.GetColorFromBrush(GetMonochromeAnimated(control));
                if (isPressed)
                    backColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+60%", null);
                else if (isMouseOver)
                    backColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+20%", null);

                var foreColor = (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+75%", null);
                var borderColor = isPressed || isMouseOver ? (Color)ColorHslBrush.Instance.Convert(backColor, typeof(Color), "+30%", null) : backColor;

                if (!(control.Background is SolidColorBrush && !((SolidColorBrush)control.Background).IsSealed))
                    control.Background = new SolidColorBrush(backColor);
                if (!(control.Foreground is SolidColorBrush && !((SolidColorBrush)control.Foreground).IsSealed))
                    control.Foreground = new SolidColorBrush(foreColor);
                if (!(control.BorderBrush is SolidColorBrush && !((SolidColorBrush)control.BorderBrush).IsSealed))
                    control.BorderBrush = new SolidColorBrush(borderColor);

                AnimateSolidColorBrush((SolidColorBrush)control.Background, backColor);
                AnimateSolidColorBrush((SolidColorBrush)control.Foreground, foreColor);
                AnimateSolidColorBrush((SolidColorBrush)control.BorderBrush, borderColor);

                if (!Tips.AreEqual(control.Opacity, control.IsEnabled ? 1.0 : 0.4))
                {
                    var animation = new DoubleAnimation { From = control.Opacity, To = control.IsEnabled ? 1.0 : 0.4, Duration = AnimationHelper.SlowAnimationDuration };
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
                    dpd.AddValueChanged(control, UpdateBichrome);
                    dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
                    dpd.AddValueChanged(control, UpdateBichrome);
                    dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                    dpd.AddValueChanged(control, UpdateBichrome);
                }));

                UpdateBichrome(control, null);
            }

            /*Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                if (d is Control control && e.Property == BichromeBackgroundProperty)
                    control.Background = (Brush)e.NewValue;
                else if (d is Control control2 && e.Property == BichromeForegroundProperty)
                    control2.Foreground = (Brush)e.NewValue;
                else if (d is Panel panel && e.Property == BichromeBackgroundProperty)
                    panel.Background = (Brush)e.NewValue;
                else if (d is TextBlock textBlock && e.Property == BichromeBackgroundProperty)
                    textBlock.Background = (Brush)e.NewValue;
                else return;

                var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
                dpd.RemoveValueChanged(d, UpdateBichrome);
                dpd.AddValueChanged(d, UpdateBichrome);
            }));*/
        }
        private static void BichromeRemoveEvents(Control control)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, UpdateBichrome);
            dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(UIElement));
            dpd.RemoveValueChanged(control, UpdateBichrome);
            dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
            dpd.RemoveValueChanged(control, UpdateBichrome);
        }

        private static void UpdateBichrome(object sender, EventArgs e)
        {
        }
    }
}
