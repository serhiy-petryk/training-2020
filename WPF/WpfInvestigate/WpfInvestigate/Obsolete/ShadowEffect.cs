using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using WpfInvestigate.Controls;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.Obsolete
{
    /// <summary>
    /// </summary>
    public class ShadowEffect
    {
        public static readonly DependencyProperty DropShadowEffectProperty = DependencyProperty.RegisterAttached(
            "DropShadowEffect", typeof(DropShadowEffect), typeof(ShadowEffect), new UIPropertyMetadata(null, OnDropShadowEffectChanged));
        public static DropShadowEffect GetDropShadowEffect(DependencyObject obj) => (DropShadowEffect)obj.GetValue(DropShadowEffectProperty);
        public static void SetDropShadowEffect(DependencyObject obj, DropShadowEffect value) => obj.SetValue(DropShadowEffectProperty, value);

        private static void OnDropShadowEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.SizeChanged -= Element_DropShadowChanged;
                //var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusWithinProperty, typeof(UIElement));
                // dpd.RemoveValueChanged(element, OnElementFocusChanged);

                if (e.NewValue is DropShadowEffect)
                {
                    element.SizeChanged += Element_DropShadowChanged;
                    Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                    {
                        Element_DropShadowChanged(element, null);
                        // dpd.AddValueChanged(element, OnElementFocusChanged);
                    }, DispatcherPriority.Loaded);
                }
            }
        }

        private static void Element_DropShadowChanged(object sender, SizeChangedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var layer = AdornerLayer.GetAdornerLayer(element);
            var adornerControl = layer.GetAdorners(element)?.OfType<AdornerControl>().FirstOrDefault(a => a.Child.Name == "Shadow");
            // var colorAnimation = (ColorAnimation)adornerControl?.Child.Resources["ColorAnimation"];
            // var thicknessAnimation = (ThicknessAnimation)adornerControl?.Child.Resources["ThicknessAnimation"];

            if (adornerControl == null)
            {
                var adorner = new Border
                {
                    Name = "Shadow",
                    Background = Brushes.Red,
                    BorderThickness = new Thickness(),
                    Focusable = false,
                    IsHitTestVisible = false,
                    UseLayoutRounding = true,
                    SnapsToDevicePixels = true
                };

                // colorAnimation = new ColorAnimation { Duration = AnimationHelper.AnimationDuration };
                // adorner.Resources.Add("ColorAnimation", colorAnimation);
                // thicknessAnimation = new ThicknessAnimation { Duration = AnimationHelper.AnimationDuration };
                // adorner.Resources.Add("ThicknessAnimation", thicknessAnimation);

                adornerControl = new AdornerControl(element)
                {
                    Child = adorner, AdornerSize = AdornerControl.AdornerSizeType.ChildElement,
                    UseLayoutRounding = true, SnapsToDevicePixels = true
                };
                layer.Add(adornerControl);
            }
            else
                adornerControl.Visibility = Visibility.Visible;

            var baseRect = new Rect {Width = element.Width, Height = element.Height};
            var shadow = GetDropShadowEffect(element);
            var mi = shadow.GetType().GetMethod("GetRenderBounds", BindingFlags.Instance | BindingFlags.NonPublic);
            var shadowRect = (Rect)mi.Invoke(shadow, new object[] { baseRect});
            var rectGeometry = new RectangleGeometry(shadowRect);

            var child = (Border)adornerControl.Child;
            child.Width = element.Width;
            child.Height = element.Height;
            child.Effect = shadow;
            child.CornerRadius = ControlHelper.GetCornerRadius(element) ?? new CornerRadius();
            var a2 = ControlHelper.GetRoundRectangle(new Rect {Width = baseRect.Width, Height = baseRect.Height}, new Thickness(0), child.CornerRadius);
            var combined = new CombinedGeometry(GeometryCombineMode.Exclude, rectGeometry, a2);
            child.Clip = combined;

            /*var thickness = GetThickness(element);
            child.Width = element.ActualWidth + thickness.Left + thickness.Right;
            child.Height = element.ActualHeight + thickness.Top + thickness.Bottom;
            child.Margin = new Thickness(-thickness.Left, -thickness.Top, -thickness.Right, -thickness.Bottom);
            var cornerRadius = ControlHelper.GetCornerRadius(element);
            if (cornerRadius.HasValue)
                child.CornerRadius = new CornerRadius(
                    cornerRadius.Value.TopLeft + Math.Max(thickness.Left, thickness.Top) / 2,
                    cornerRadius.Value.TopRight + Math.Max(thickness.Top, thickness.Right) / 2,
                    cornerRadius.Value.BottomRight + Math.Max(thickness.Right, thickness.Bottom) / 2,
                    cornerRadius.Value.BottomLeft + Math.Max(thickness.Bottom, thickness.Left) / 2);
            else
                child.CornerRadius = new CornerRadius();

            var focusBrush = GetBrush(element);
            if (focusBrush is SolidColorBrush)
            {
                if (!(child.BorderBrush is SolidColorBrush))
                    child.BorderBrush = new SolidColorBrush();

                var oldColor = ((SolidColorBrush)child.BorderBrush).Color;
                var newColor = ((SolidColorBrush)focusBrush).Color;
                if (oldColor != newColor)
                {
                    colorAnimation.SetFromToValues(oldColor, newColor);
                    child.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                }
            }
            else
                child.BorderBrush = focusBrush.Clone();*/

            // +0.25: to remove gap between focus and element
            // var newThickness = new Thickness(thickness.Left + 0.25, thickness.Top + 0.25, thickness.Right + 0.25, thickness.Bottom + 0.25);
            // thicknessAnimation.SetFromToValues(child.BorderThickness, thickness);
            // child.BeginAnimation(Control.BorderThicknessProperty, thicknessAnimation);*/
        }
    }
}
