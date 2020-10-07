using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// </summary>
    public class FocusEffect
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.RegisterAttached(
            "Brush", typeof(SolidColorBrush), typeof(FocusEffect), new UIPropertyMetadata(null, OnBrushChanged));
        public static SolidColorBrush GetBrush(DependencyObject obj) => (SolidColorBrush)obj.GetValue(BrushProperty);
        public static void SetBrush(DependencyObject obj, SolidColorBrush value) => obj.SetValue(BrushProperty, value);

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

                if (e.NewValue is SolidColorBrush newBrush && newBrush != Brushes.Transparent)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                    {
                        element.GotFocus += Element_ChangeFocus;
                        element.LostFocus += Element_ChangeFocus;
                        element.Unloaded += Element_Unloaded;
                    }));
                }
            }
        }

        private static void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            element.GotFocus -= Element_ChangeFocus;
            element.LostFocus -= Element_ChangeFocus;
            element.Unloaded -= Element_Unloaded;
        }

        private static void Element_ChangeFocus(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            var isFocused = element.IsFocused;

            if (isFocused)
            {
                var layer = AdornerLayer.GetAdornerLayer(element);
                var adornerControl = layer.GetAdorners(element)?.OfType<AdornerControl>().FirstOrDefault(a => a.Child.Name == "Focus");
                var animation = (ColorAnimation) adornerControl?.Child.Resources["animation"];
                var thickness = GetThickness(element);

                if (adornerControl == null)
                {
                    var adorner = new Border
                    {
                        Name = "Focus", Background = Brushes.Transparent, BorderBrush = new SolidColorBrush(),
                        Focusable = false, IsHitTestVisible = false, UseLayoutRounding = false, SnapsToDevicePixels = false
                    };

                    animation = new ColorAnimation {Duration = AnimationHelper.SlowAnimationDuration};
                    adorner.Resources.Add("animation", animation);

                    adornerControl = new AdornerControl(element) { Child = adorner, UseAdornedElementSize = false };
                    layer.Add(adornerControl);
                }
                else
                    adornerControl.Visibility = Visibility.Visible;

                var child = (Border)adornerControl.Child;
                // +0.25: to remove gap between focus and element
                child.BorderThickness = new Thickness(thickness.Left + 0.25, thickness.Top + 0.25, thickness.Right + 0.25, thickness.Bottom + 0.25);
                child.Width = element.ActualWidth + thickness.Left + thickness.Right;
                child.Height = element.ActualHeight + thickness.Top + thickness.Bottom;
                child.Margin = new Thickness(-thickness.Left, -thickness.Top, -thickness.Right, -thickness.Bottom);
                var cornerRadius = Helpers.ControlHelper.GetCornerRadius(element);
                if (cornerRadius.HasValue)
                    child.CornerRadius = new CornerRadius(
                        cornerRadius.Value.TopLeft + Math.Max(thickness.Left, thickness.Top) / 2,
                        cornerRadius.Value.TopRight + Math.Max(thickness.Top, thickness.Right) / 2,
                        cornerRadius.Value.BottomRight + Math.Max(thickness.Right, thickness.Bottom) / 2,
                        cornerRadius.Value.BottomLeft + Math.Max(thickness.Bottom, thickness.Left) / 2);
                else
                    child.CornerRadius = new CornerRadius();

                var oldColor = ((SolidColorBrush)child.BorderBrush).Color;
                var newColor = GetBrush(element).Color;
                if (oldColor != newColor)
                {
                    animation.SetFromToValues(oldColor, newColor);
                    child.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
            }
            else
            {
                var layer = AdornerLayer.GetAdornerLayer(element);
                var adorners = layer?.GetAdorners(element) ?? new Adorner[0];
                foreach (var adorner in adorners.OfType<AdornerControl>().Where(a => a.Child.Name == "Focus"))
                {
                    var oldColor = ((SolidColorBrush)((Border)adorner.Child).BorderBrush).Color;
                    if (oldColor != Colors.Transparent)
                    {
                        var animation = (ColorAnimation) adorner.Child.Resources["animation"];
                        animation.SetFromToValues(oldColor, Colors.Transparent);
                        ((Border) adorner.Child).BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                    }
                }
            }
        }
    }
}
