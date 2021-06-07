// Based on https://github.com/Taka414/RippleEffectControl

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Controls;
using WpfSpLib.Helpers;

namespace WpfSpLib.Effects
{
    /// <summary>
    /// Ripple Effect for FrameworkElement
    /// Usage: <Button controls:RippleEffect.RippleColor="White" Width="70" Height="30" Content="Button" />
    /// </summary>
    public class ClickEffect
    {
        public static readonly DependencyProperty ShiftOffsetOnClickProperty = DependencyProperty.RegisterAttached("ShiftOffsetOnClick",
            typeof(double), typeof(ClickEffect), new FrameworkPropertyMetadata(0.0, OnPropertiesChanged));
        public static double GetShiftOffsetOnClick(DependencyObject obj) => (double)obj.GetValue(ShiftOffsetOnClickProperty);
        /// <summary>The number of pixels by which the element will move down when the mouse button is pressed. Recommended value is 1.0 or 1.5</summary>
        public static void SetShiftOffsetOnClick(DependencyObject obj, double value) => obj.SetValue(ShiftOffsetOnClickProperty, value);

        public static readonly DependencyProperty RippleColorProperty = DependencyProperty.RegisterAttached("RippleColor",
            typeof(Color?), typeof(ClickEffect), new FrameworkPropertyMetadata(null, OnPropertiesChanged));
        public static Color? GetRippleColor(DependencyObject obj) => (Color?)obj.GetValue(RippleColorProperty);
        public static void SetRippleColor(DependencyObject obj, Color? value) => obj.SetValue(RippleColorProperty, value);

        //=====================================
        private static void OnPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    fe.PreviewMouseLeftButtonDown -= OnElementPreviewMouseLeftButtonDown;
                    fe.PreviewMouseLeftButtonUp -= OnElementPreviewMouseLeftButtonUp;

                    if (!fe.IsElementDisposing())
                    {
                        fe.PreviewMouseLeftButtonDown += OnElementPreviewMouseLeftButtonDown;
                        fe.PreviewMouseLeftButtonUp += OnElementPreviewMouseLeftButtonUp;

                        // Suppress 'Pressed' VisualState for ripple FlatButton
                        foreach (var o in Tips.GetVisualChildren(fe).OfType<FrameworkElement>())
                        {
                            var groups = VisualStateManager.GetVisualStateGroups(o);
                            if (groups != null)
                                foreach (var visualGroup in groups.OfType<VisualStateGroup>())
                                {
                                    var state = visualGroup.States.Cast<VisualState>()
                                        .FirstOrDefault(v => v.Name == "Pressed");
                                    if (state != null)
                                        visualGroup.States.Remove(state);
                                }
                        }
                    }
                }, DispatcherPriority.Loaded);
            }
        }

        private static void OnElementPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var content = Tips.GetVisualChildren(sender as FrameworkElement).OfType<ContentPresenter>().FirstOrDefault() ?? sender as FrameworkElement;
            if (content != null && content.RenderTransform is TranslateTransform transform)
                transform.Y = 0.0;
        }

        private static void OnElementPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement element)) return;

            var rippleColor = GetRippleColor(element);
            if (rippleColor.HasValue)
                Animate(element, e.GetPosition(element), rippleColor.Value);

            var shiftOffsetOnClick = GetShiftOffsetOnClick(element);
            if (!Tips.AreEqual(0.0, shiftOffsetOnClick))
            {
                var content = Tips.GetVisualChildren(element).OfType<ContentPresenter>().FirstOrDefault() ?? element;
                if (!(content.RenderTransform is TranslateTransform))
                    content.RenderTransform = new TranslateTransform();

                content.Dispatcher.InvokeAsync(new Action(() => ((TranslateTransform) content.RenderTransform).Y = shiftOffsetOnClick), DispatcherPriority.ContextIdle);
            }
        }

        private static void Animate(FrameworkElement fe, Point mousePosition, Color rippleColor)
        {
            var layer = AdornerLayer.GetAdornerLayer(fe);
            Grid grid;
            Ellipse ellipse;
            var adornerControl = layer.GetAdorners(fe)?.FirstOrDefault(a => a is AdornerControl && ((AdornerControl)a).Child.Name == "Ripple") as AdornerControl;
            if (adornerControl == null)
            {
                ellipse = new Ellipse
                {
                    Name = "Ripple",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Fill = new SolidColorBrush(rippleColor),
                    IsHitTestVisible = false,
                };
                var binding = new Binding("Width"){Source = ellipse};
                BindingOperations.SetBinding(ellipse, FrameworkElement.HeightProperty, binding);

                grid = new Grid{ ClipToBounds = true};
                grid.Children.Add(ellipse);
                adornerControl = new AdornerControl(fe) { Child = grid };
                layer.Add(adornerControl);
            }
            else
            {
                grid = (Grid)adornerControl.Child;
                ellipse = (Ellipse) grid.Children[0];
            }

            grid.Width = fe.ActualWidth;
            grid.Height = fe.ActualHeight;

            var newSize = Math.Max(fe.ActualWidth, fe.ActualHeight) * 3;
            var oldMargin = new Thickness(mousePosition.X, mousePosition.Y, 0, 0);
            var newMargin = new Thickness(mousePosition.X - newSize / 2, mousePosition.Y - newSize / 2, 0, 0);
            var duration = Math.Min(1000, Math.Max(500, newSize * 2.5));
            ellipse.BeginAnimation(FrameworkElement.MarginProperty, new ThicknessAnimation(oldMargin, newMargin, TimeSpan.FromMilliseconds(duration)));
            ellipse.BeginAnimation(FrameworkElement.WidthProperty, new DoubleAnimation(0.0, newSize, TimeSpan.FromMilliseconds(duration)));
            ellipse.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.5, 0.0, TimeSpan.FromMilliseconds(duration * 2 / 3)));
        }
    }
}
