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
using WpfInvestigate.Common;
using WpfInvestigate.Controls;

namespace WpfInvestigate.Effects
{
    /// <summary>
    /// Ripple Effect for FrameworkElement
    /// Usage: <Button controls:RippleEffect.RippleColor="White" Width="70" Height="30" Content="Button" />
    /// </summary>
    public class ClickEffect
    {
        public static readonly DependencyProperty ShiftOnClickOffsetProperty = DependencyProperty.RegisterAttached("ShiftOnClickOffset",
            typeof(double), typeof(ClickEffect), new UIPropertyMetadata(0.0, OnPropertiesChanged));
        public static double GetShiftOnOffsetClick(DependencyObject obj) => (double)obj.GetValue(ShiftOnClickOffsetProperty);
        /// <summary>The number of pixels by which the element will move down when the mouse button is pressed. Recommended value is 1.0 or 1.5</summary>
        public static void SetShiftOnOffsetClick(DependencyObject obj, double value) => obj.SetValue(ShiftOnClickOffsetProperty, value);

        public static readonly DependencyProperty RippleColorProperty = DependencyProperty.RegisterAttached("RippleColor",
            typeof(Color?), typeof(ClickEffect), new UIPropertyMetadata(null, OnPropertiesChanged));
        public static Color? GetRippleColor(DependencyObject obj) => (Color?)obj.GetValue(RippleColorProperty);
        public static void SetRippleColor(DependencyObject obj, Color? value) => obj.SetValue(RippleColorProperty, value);

        //=====================================
        private static void OnPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    fe.Unloaded += OnElementUnloaded;
                    fe.PreviewMouseLeftButtonDown += OnElementPreviewMouseLeftButtonDown;
                    fe.PreviewMouseLeftButtonUp += OnElementPreviewMouseLeftButtonUp;

                    // Suppress 'Pressed' VisualState for ripple FlatButton
                    foreach (var o in Tips.GetVisualChildren(fe).OfType<FrameworkElement>())
                    {
                        var groups = VisualStateManager.GetVisualStateGroups(o);
                        if (groups != null)
                            foreach (var visualGroup in groups?.OfType<VisualStateGroup>())
                            {
                                var state = visualGroup.States.Cast<VisualState>().FirstOrDefault(v => v.Name == "Pressed");
                                if (state != null)
                                    visualGroup.States.Remove(state);
                            }
                    }
                }));
            }
        }
        private static void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            fe.Unloaded -= OnElementUnloaded;
            fe.PreviewMouseLeftButtonDown -= OnElementPreviewMouseLeftButtonDown;
            fe.PreviewMouseLeftButtonUp -= OnElementPreviewMouseLeftButtonUp;
        }


        private static void OnElementPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.RenderTransform is TranslateTransform transform)
                transform.Y = 0.0;
        }

        private static void OnElementPreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var rippleColor = GetRippleColor(element);
            if (rippleColor.HasValue)
                Animate(element, e.GetPosition(element), rippleColor.Value);

            var shiftOnClickOffset = GetShiftOnOffsetClick(element);
            if (!Tips.AreEqual(0.0, shiftOnClickOffset))
            {
                if (!(element.RenderTransform is TranslateTransform))
                    element.RenderTransform = new TranslateTransform();

                element.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
                    new Action(() => ((TranslateTransform) element.RenderTransform).Y = shiftOnClickOffset));
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

            var sb = ellipse.Resources["storyboard"] as Storyboard;
            if (sb == null)
            {
                sb = new Storyboard();
                ellipse.Resources["storyboard"] = sb;
                sb.Children.Add(ellipse.CreateAnimation(FrameworkElement.MarginProperty));
                sb.Children.Add(ellipse.CreateAnimation(FrameworkElement.WidthProperty));
                sb.Children.Add(ellipse.CreateAnimation(UIElement.OpacityProperty));
            }

            var newSize = Math.Max(fe.ActualWidth, fe.ActualHeight) * 3;
            var oldMargin = new Thickness(mousePosition.X, mousePosition.Y, 0, 0);
            var newMargin = new Thickness(mousePosition.X - newSize / 2, mousePosition.Y - newSize / 2, 0, 0);
            sb.Children[0].SetFromToValues(oldMargin, newMargin);
            sb.Children[1].SetFromToValues(0.0, newSize);
            sb.Children[2].SetFromToValues(0.5, 0.0);

            var newDuration = Math.Min(1000, Math.Max(300, newSize * 2.5));
            sb.Children[0].Duration = TimeSpan.FromMilliseconds(newDuration);
            sb.Children[1].Duration = sb.Children[0].Duration;
            sb.Children[2].Duration = TimeSpan.FromMilliseconds(newDuration * 2 / 3);
            sb.Begin();
        }
    }
}
