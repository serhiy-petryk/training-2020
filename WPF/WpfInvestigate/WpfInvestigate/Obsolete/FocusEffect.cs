using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfInvestigate.Controls;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.Obsolete
{
    /// <summary>
    /// </summary>
    public class FocusEffect
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.RegisterAttached(
            "Brush", typeof(Brush), typeof(FocusEffect), new FrameworkPropertyMetadata(null, OnBrushChanged));
        public static Brush GetBrush(DependencyObject obj) => (Brush)obj.GetValue(BrushProperty);
        public static void SetBrush(DependencyObject obj, Brush value) => obj.SetValue(BrushProperty, value);

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.RegisterAttached(
            "Thickness", typeof(Thickness), typeof(FocusEffect), new FrameworkPropertyMetadata(new Thickness(3)));
        public static Thickness GetThickness(DependencyObject obj) => (Thickness)obj.GetValue(ThicknessProperty);
        public static void SetThickness(DependencyObject obj, Thickness value) => obj.SetValue(ThicknessProperty, value);

        //=====================================
        private static void OnBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.SizeChanged -= Element_ChangeFocus;
                var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusWithinProperty, typeof(UIElement));
                dpd.RemoveValueChanged(element, OnElementFocusChanged);

                if (e.NewValue is Brush newBrush && newBrush != Brushes.Transparent)
                {
                    Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                    {
                        element.SizeChanged += Element_ChangeFocus;
                        dpd.AddValueChanged(element, OnElementFocusChanged );
                    }, DispatcherPriority.Background);
                }
            }
        }

        private static void OnElementFocusChanged(object sender, EventArgs e) => Element_ChangeFocus(sender, null);
        private static void Element_ChangeFocus(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            var isFocused = element.IsKeyboardFocusWithin;

            if (isFocused)
            {
                var layer = AdornerLayer.GetAdornerLayer(element);
                var adornerControl = layer.GetAdorners(element)?.OfType<AdornerControl>().FirstOrDefault(a => a.Child.Name == "Focus");

                if (adornerControl == null)
                {
                    var adorner = new Border
                    {
                        Name = "Focus", Background = Brushes.Transparent, Focusable = false, IsHitTestVisible = false,
                        UseLayoutRounding = false, SnapsToDevicePixels = false
                    };

                    adornerControl = new AdornerControl(element) { Child = adorner, AdornerSize  = AdornerControl.AdornerSizeType.ChildElement};
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
                    child.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(oldColor, newColor, Common.AnimationHelper.AnimationDuration));
                }
                else
                    child.BorderBrush = focusBrush.Clone();

                // +0.25: to remove gap between focus and element
                // var newThickness = new Thickness(thickness.Left + 0.25, thickness.Top + 0.25, thickness.Right + 0.25, thickness.Bottom + 0.25);
                child.BeginAnimation(Control.BorderThicknessProperty, new ThicknessAnimation(child.BorderThickness, thickness, Common.AnimationHelper.AnimationDuration));
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
                        border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(oldColor, Colors.Transparent, Common.AnimationHelper.AnimationDuration));
                    }

                    border.BeginAnimation(Control.BorderThicknessProperty, new ThicknessAnimation(border.BorderThickness, new Thickness(), Common.AnimationHelper.AnimationDuration));
                }

                // if isFocused=false не завжди спрацьовує фокус на новому елементі -> Activate focus on focused element
                var focusedControl = Keyboard.FocusedElement as FrameworkElement;
                if (focusedControl != null && focusedControl != element && GetBrush(focusedControl) != null)
                    Element_ChangeFocus(focusedControl, new RoutedEventArgs());
            }
        }
    }
}
