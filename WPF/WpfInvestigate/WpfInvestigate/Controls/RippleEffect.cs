// Based on https://github.com/Taka414/RippleEffectControl
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Ripple Effect for FrameworkElement
    /// Usage: <Button controls:RippleEffect.RippleColor="White" Width="70" Height="30" Content="Button" />
    /// </summary>
    public class RippleEffect
    {
        public static readonly DependencyProperty RippleColorProperty = DependencyProperty.RegisterAttached("RippleColor",
            typeof(Color?), typeof(RippleEffect), new UIPropertyMetadata(null, OnPropertiesChanged));
        public static Color? GetRippleColor(DependencyObject obj) => (Color?) obj.GetValue(RippleColorProperty);
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

                    // Suppress 'Pressed' VisualState for ripple FlatButton
                    foreach (FrameworkElement o in Tips.GetVisualChildren(fe).Where(a => a is FrameworkElement))
                    {
                        var groups = VisualStateManager.GetVisualStateGroups(o);
                        if (groups != null)
                            foreach (VisualStateGroup visualGroup in groups?.Cast<object>().Where(g => g is VisualStateGroup))
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
            var fe = (FrameworkElement) sender;
            fe.Unloaded -= OnElementUnloaded;
            fe.PreviewMouseLeftButtonDown -= OnElementPreviewMouseLeftButtonDown;
        }

        private static void OnElementPreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Animate(sender as FrameworkElement, e.GetPosition(sender as IInputElement), GetRippleColor(sender as DependencyObject));
        }

        private static void Animate(FrameworkElement fe, Point mousePosition, Color? rippleColor)
        {
            if (fe == null || !rippleColor.HasValue)
                return;

            var color = rippleColor.Value;
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
                    Fill = new SolidColorBrush(color),
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
            var sb = new Storyboard();

            var marginAnimation = AnimationHelper.GetMarginAnimation(ellipse, oldMargin, newMargin);
            marginAnimation.Duration = TimeSpan.FromMilliseconds(800);
            sb.Children.Add(marginAnimation);

            var widthAnimation = AnimationHelper.GetWidthAnimation(ellipse, 0, newSize);
            widthAnimation.Duration = TimeSpan.FromMilliseconds(800);
            sb.Children.Add(widthAnimation);

            var opacityAnimation = AnimationHelper.GetOpacityAnimation(ellipse, 0.5, 0);
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(600);
            sb.Children.Add(opacityAnimation);

            sb.Begin();
        }
    }
}
