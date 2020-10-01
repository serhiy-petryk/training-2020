using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ColorInvestigation.Common;

namespace ColorInvestigation.Controls
{
    /// <summary>
    /// </summary>
    public class ControlEffects
    {
        #region ================  PathData  ==========================
        public static readonly DependencyProperty PathDataProperty = DependencyProperty.RegisterAttached(
            "PathData", typeof(Geometry), typeof(ControlEffects), new UIPropertyMetadata(null, OnPathDataChanged));
        public static Geometry GetPathData(DependencyObject obj) => (Geometry)obj.GetValue(PathDataProperty);
        public static void SetPathData(DependencyObject obj, Geometry value) => obj.SetValue(PathDataProperty, value);

        private static void OnPathDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ContentControl control)
            {
                if (e.NewValue is Geometry geometry)
                {
                    var path = new Path { Stretch = Stretch.Uniform, Data = geometry };
                    var b = new Binding("Foreground")
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Control), 1)
                    };
                    path.SetBinding(Shape.FillProperty, b);
                    control.Content = new Viewbox { Child = path };
                }
                else
                    control.Content = null;
            }
        }
        #endregion

        #region ================  CornerRadius  =======================
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius", typeof(double), typeof(ControlEffects), new UIPropertyMetadata(double.NaN, OnCornerRadiusChanged));
        public static double GetCornerRadius(DependencyObject obj) => (double)obj.GetValue(CornerRadiusProperty);
        public static void SetCornerRadius(DependencyObject obj, double value) => obj.SetValue(CornerRadiusProperty, value);
        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newRadius = (double)e.NewValue;
            if (!double.IsNaN(newRadius) && newRadius > 0)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    var border = Tips.GetVisualChildren(d).OfType<Border>().FirstOrDefault();
                    if (border != null)
                        border.CornerRadius = new CornerRadius(newRadius);
                }));
            }
        }
        #endregion

        // =========================
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
            "Background", typeof(Brush), typeof(ControlEffects), new UIPropertyMetadata(null, OnPropertiesChanged));
        public static Brush GetBackground(DependencyObject obj) => (Brush)obj.GetValue(BackgroundProperty);
        public static void SetBackground(DependencyObject obj, Brush value) => obj.SetValue(BackgroundProperty, value);
        
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
            "Foreground", typeof(Brush), typeof(ControlEffects), new FrameworkPropertyMetadata(null, OnPropertiesChanged));
        public static Brush GetForeground(DependencyObject obj) => (Brush)obj.GetValue(ForegroundProperty);
        public static void SetForeground(DependencyObject obj, Brush value) => obj.SetValue(ForegroundProperty, value);
        private static void OnPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                if (d is Control control && e.Property == BackgroundProperty)
                    control.Background = (Brush)e.NewValue;
                else if (d is Control control2 && e.Property == ForegroundProperty)
                    control2.Foreground = (Brush)e.NewValue;
                else if (d is Panel panel && e.Property == BackgroundProperty)
                    panel.Background = (Brush)e.NewValue;
                else if (d is TextBlock textBlock && e.Property == BackgroundProperty)
                    textBlock.Background = (Brush) e.NewValue;
                else return;

                var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
                dpd.RemoveValueChanged(d, OnMouseOver);
                dpd.AddValueChanged(d, OnMouseOver);
            }));
            
        }

        private static void OnMouseOver(object sender, EventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                var element = (UIElement) sender;
                var endColor = element.IsMouseOver ? Colors.Red : ((SolidColorBrush) GetBackground(element)).Color;
                // VisualStateManager.GoToState((FrameworkElement)element, "Pressed", false);
                // VisualStateManager.GoToElementState((FrameworkElement)element, "Disabled", false);
                var startColor = element.IsMouseOver ? ((SolidColorBrush) GetBackground(element)).Color : Colors.Red;
                var animation = new ColorAnimation(startColor, endColor, TimeSpan.FromMilliseconds(120), FillBehavior.HoldEnd);
                if (element is Control control)
                {
                    control.Background = new SolidColorBrush(startColor);
                    control.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
            }));
        }
    }
}
