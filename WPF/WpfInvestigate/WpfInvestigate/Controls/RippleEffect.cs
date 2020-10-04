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
                    foreach (var o in Tips.GetVisualChildren(fe).OfType<FrameworkElement>())
                    {
                        var groups = VisualStateManager.GetVisualStateGroups(o);
                        if (groups != null)
                            foreach (VisualStateGroup visualGroup in groups?.OfType<VisualStateGroup>())
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

            var sb = ellipse.Resources["storyboard"] as Storyboard;
            if (sb == null)
            {
                sb = new Storyboard();
                ellipse.Resources["storyboard"] = sb;
                sb.Children.Add(ellipse.CreateAnimation(FrameworkElement.MarginProperty, 800));
                sb.Children.Add(ellipse.CreateAnimation(FrameworkElement.WidthProperty, 800));
                sb.Children.Add(ellipse.CreateAnimation(UIElement.OpacityProperty, 600));
            }

            var newSize = Math.Max(fe.ActualWidth, fe.ActualHeight) * 3;
            var oldMargin = new Thickness(mousePosition.X, mousePosition.Y, 0, 0);
            var newMargin = new Thickness(mousePosition.X - newSize / 2, mousePosition.Y - newSize / 2, 0, 0);

            sb.Children[0].SetFromToValues(oldMargin, newMargin);
            sb.Children[1].SetFromToValues(0.0, newSize);
            sb.Children[2].SetFromToValues(0.5, 0.0);

            sb.Begin();
        }
    }
}
