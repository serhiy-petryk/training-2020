using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfInvestigate.Controls.Effects
{
    /// <summary>
    /// </summary>
    public class ControlEffect
    {
        #region ================  PathData  ==========================
        public static readonly DependencyProperty PathDataProperty = DependencyProperty.RegisterAttached(
            "PathData", typeof(Geometry), typeof(ControlEffect), new UIPropertyMetadata(null, OnPathDataChanged));
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
            "CornerRadius", typeof(CornerRadius), typeof(ControlEffect), new UIPropertyMetadata(new CornerRadius(), OnCornerRadiusChanged));
        public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
        public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);
        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                var border = Helpers.ControlHelper.GetMainBorders(d as FrameworkElement).FirstOrDefault();
                if (border != null)
                    border.CornerRadius = (CornerRadius)e.NewValue;
            }));
        }
        #endregion
    }
}
