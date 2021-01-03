using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        //================
        public static readonly DependencyProperty LimitPositionToPanelBoundsProperty = DependencyProperty.RegisterAttached(
            "LimitPositionToPanelBounds", typeof(bool), typeof(ResizeEffect), new UIPropertyMetadata(false));
        public static bool GetLimitPositionToPanelBounds(DependencyObject obj) => (bool)obj.GetValue(LimitPositionToPanelBoundsProperty);
        public static void SetLimitPositionToPanelBounds(DependencyObject obj, bool value) => obj.SetValue(LimitPositionToPanelBoundsProperty, value);
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
                    var row = Grid.GetRow(element);
                    var column = Grid.GetColumn(element);
                    grid.Children.Remove(element);
                    var decorator = new AdornerDecorator {Child = element};
                    Grid.SetRow(decorator, row);
                    Grid.SetColumn(decorator, column);
                    grid.Children.Add(decorator);
                    return;
                }

                if (element.Parent is Canvas canvas)
                {
                    canvas.Children.Remove(element);
                    var decorator = new AdornerDecorator { Child = element };
                    canvas.Children.Add(decorator);
                    return;
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
        // ==========================
        public static readonly DependencyProperty MovingThumbProperty = DependencyProperty.RegisterAttached(
            "MovingThumb", typeof(Thumb), typeof(ResizeEffect), new PropertyMetadata(null, OnMovingThumbChanged));
        public static Thumb GetMovingThumb(DependencyObject obj) => (Thumb)obj.GetValue(MovingThumbProperty);
        public static void SetMovingThumb(DependencyObject obj, Thumb value) => obj.SetValue(MovingThumbProperty, value);

        private static void OnMovingThumbChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                /*if (e.OldValue is Thumb oldThumb)
                    oldThumb.DragDelta -= control.MoveThumb_OnDragDelta;
                if (e.NewValue is Thumb newThumb)
                    newThumb.DragDelta += control.MoveThumb_OnDragDelta;*/
            }
        }

    }
}
