using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.Effects
{
    /// <summary>
    /// </summary>
    public class FocusEffect
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.RegisterAttached(
            "Brush", typeof(Brush), typeof(FocusEffect), new UIPropertyMetadata(null, OnBrushChanged));
        public static Brush GetBrush(DependencyObject obj) => (Brush)obj.GetValue(BrushProperty);
        public static void SetBrush(DependencyObject obj, Brush value) => obj.SetValue(BrushProperty, value);

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.RegisterAttached(
            "Thickness", typeof(Thickness), typeof(FocusEffect), new UIPropertyMetadata(new Thickness(3)));
        public static Thickness GetThickness(DependencyObject obj) => (Thickness)obj.GetValue(ThicknessProperty);
        public static void SetThickness(DependencyObject obj, Thickness value) => obj.SetValue(ThicknessProperty, value);

        //=====================================
        private static void OnBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                Element_Unloaded(element, null);

                if (e.NewValue is Brush newBrush && newBrush != Brushes.Transparent)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                    {
                        element.SizeChanged += Element_ChangeFocus;
                        var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusWithinProperty, typeof(UIElement));
                        dpd.AddValueChanged(element, OnElementFocusChanged );
                        element.Unloaded += Element_Unloaded;
                    }));
                }
            }
        }

        private static void OnElementFocusChanged(object sender, EventArgs e) => Element_ChangeFocus(sender, null);
        private static void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            element.SizeChanged += Element_ChangeFocus;
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusWithinProperty, typeof(UIElement));
            dpd.RemoveValueChanged(element, OnElementFocusChanged);
            element.Unloaded -= Element_Unloaded;
        }

        private static void Element_ChangeFocus(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            var isFocused = element.IsKeyboardFocusWithin;

            if (isFocused)
            {
                var layer = AdornerLayer.GetAdornerLayer(element);
                var adornerControl = layer.GetAdorners(element)?.OfType<AdornerControl>().FirstOrDefault(a => a.Child.Name == "Focus");
                var colorAnimation = (ColorAnimation)adornerControl?.Child.Resources["ColorAnimation"];
                var thicknessAnimation = (ThicknessAnimation)adornerControl?.Child.Resources["ThicknessAnimation"];

                if (adornerControl == null)
                {
                    var adorner = new Border
                    {
                        Name = "Focus", Background = Brushes.Transparent, Focusable = false, IsHitTestVisible = false,
                        UseLayoutRounding = false, SnapsToDevicePixels = false
                    };

                    colorAnimation = new ColorAnimation {Duration = AnimationHelper.AnimationDuration};
                    adorner.Resources.Add("ColorAnimation", colorAnimation);
                    thicknessAnimation = new ThicknessAnimation { Duration = AnimationHelper.AnimationDuration };
                    adorner.Resources.Add("ThicknessAnimation", thicknessAnimation);

                    adornerControl = new AdornerControl(element) { Child = adorner, UseAdornedElementSize = false };
                    layer.Add(adornerControl);
                }
                else
                    adornerControl.Visibility = Visibility.Visible;

                var thickness = GetThickness(element);
                var child = (Border)adornerControl.Child;
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
                    var newColor = ((SolidColorBrush) focusBrush).Color;
                    if (oldColor != newColor)
                    {
                        colorAnimation.SetFromToValues(oldColor, newColor);
                        child.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                    }
                }
                else
                    child.BorderBrush = focusBrush.Clone();

                // +0.25: to remove gap between focus and element
                // var newThickness = new Thickness(thickness.Left + 0.25, thickness.Top + 0.25, thickness.Right + 0.25, thickness.Bottom + 0.25);
                thicknessAnimation.SetFromToValues(child.BorderThickness, thickness);
                child.BeginAnimation(Control.BorderThicknessProperty, thicknessAnimation);
            }
            else
            {
                var layer = AdornerLayer.GetAdornerLayer(element);
                var adorners = layer?.GetAdorners(element) ?? new Adorner[0];
                foreach (var adorner in adorners.OfType<AdornerControl>().Where(a => a.Child.Name == "Focus"))
                {
                    var border = (Border) adorner.Child;
                    if (border.BorderBrush is SolidColorBrush)
                    {
                        var oldColor = ((SolidColorBrush)border.BorderBrush).Color;
                        if (oldColor != Colors.Transparent)
                        {
                            var animation = (ColorAnimation)adorner.Child.Resources["ColorAnimation"];
                            animation.SetFromToValues(oldColor, Colors.Transparent);
                            border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                        }
                    }

                    var thicknessAnimation = (ThicknessAnimation)adorner.Child.Resources["ThicknessAnimation"];
                    thicknessAnimation.SetFromToValues(border.BorderThickness, new Thickness());
                    border.BeginAnimation(Control.BorderThicknessProperty, thicknessAnimation);
                }
            }
        }
    }
}
