using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Movable.Controls;

namespace Movable.Effects
{
    public class ResizeEffect
    {
        #region ==============  Properties  ==============
        public static readonly DependencyProperty EdgeThicknessProperty = DependencyProperty.RegisterAttached(
            "EdgeThickness", typeof(Thickness), typeof(ResizeEffect), new UIPropertyMetadata(new Thickness(), OnEdgeThicknessChanged));

        public static Thickness GetEdgeThickness(DependencyObject obj) => (Thickness)obj.GetValue(EdgeThicknessProperty);
        public static void SetEdgeThickness(DependencyObject obj, Thickness value) => obj.SetValue(EdgeThicknessProperty, value);
        //================
        private static void OnEdgeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if (element.IsLoaded)
                    ApplyResizeEffect(element);
                else
                    element.Loaded += (sender, args) => ApplyResizeEffect(element);
            }
        }
        #endregion

        private static void ApplyResizeEffect(FrameworkElement element)
        {
            var layer = AdornerLayer.GetAdornerLayer(element);
            if (layer == null) return;

            var adornerControl = layer.GetAdorners(element)?.OfType<AdornerControl>().FirstOrDefault(a => a.Child.Name == "ResizeControl");
            if (adornerControl == null)
            {
                if (element.Parent is Grid grid)
                {
                    grid.Children.Remove(element);
                    var decorator = new AdornerDecorator(){Child = element};
                    // Grid.SetRow(decorator, 0);
                    // Grid.SetColumn(decorator, 0);
                    grid.Children.Add(decorator);
                    element = decorator;
                    return;
                }

                else if (element.Parent is AdornerDecorator decorator)
                {
                    // element = decorator;
                }
                else
                    {
                    //return;
                }
                var control = new ResizingAdornerControl(element)
                {
                    Name = "ResizeControl",
                    Focusable = false
                };

                adornerControl = new AdornerControl(element) { Child = control, AdornerSize = AdornerControl.AdornerSizeType.AdornedElement, Opacity = 1 };
                layer.Add(adornerControl);
            }

        }

    }
}
