using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ColorInvestigation.Common;

namespace ColorInvestigation.Lib
{
    /// <summary>
    /// Interaction logic for FlatButton.xaml
    /// </summary>
    public partial class FlatButton
    {
        public FlatButton()
        {
            InitializeComponent();
        }

        #region =========  FlatButton Extension  =========
        private static Geometry GetGeometry(object o)
        {
            if (!(o is string))
                return null;
            const string pathString = "FMLHVCQSTAZ+-,.0123456789";
            var s = (string)o;
            if (string.IsNullOrEmpty(s) || !s.All(c => pathString.Contains(c) || char.IsWhiteSpace(c)))
                return null;

            try { return Geometry.Parse(s); }
            catch { return null; }
        }

        private void OnFlatButtonLoaded(object sender, RoutedEventArgs e)
        {
            var button = sender as ButtonBase;
            if (button != null)
            {
                button.Loaded -= OnFlatButtonLoaded;
                button.Unloaded -= OnFlatButtonUnloaded;

                // if (DualPathToggleButtonEffect.GetGeometryOff(button) != Geometry.Empty)
                   // return;

                var geometry = GetGeometry(button.Content);
                if (geometry != null)
                {
                    var path = new Path {Stretch = Stretch.Uniform, Data = geometry};
                    button.Content = new Viewbox {Child = path};
                }

                button.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    if (Tips.GetVisualChildren(button).Any(a => a is Shape))
                    {
                        button.Unloaded += OnFlatButtonUnloaded;
                        SetColorsForShapes(button, null);
                        var dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                        dpd.AddValueChanged(button, SetColorsForShapes);
                    }
                }));
            }
        }
        private void OnFlatButtonUnloaded(object sender, RoutedEventArgs e)
        {
            var button = sender as ButtonBase;
            if (button != null)
            {
                button.Loaded -= OnFlatButtonLoaded;
                button.Unloaded -= OnFlatButtonUnloaded;
                var dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                dpd.RemoveValueChanged(button, SetColorsForShapes);
            }
        }
        private void SetColorsForShapes(object sender, EventArgs e)
        {
            var button = sender as ButtonBase;
            // var rippleColor = RippleEffect.GetRippleColor(button); // don't apply Pressed effect if control has ripple effect
            // var color = button.IsPressed && rippleColor == null ? Tips.GetActualBackgroundColor(button) : Tips.GetActualForegroundColor(button);
            var color = button.IsPressed ? Tips.GetActualBackgroundColor(button) : Tips.GetActualForegroundColor(button);

            foreach (Shape shape in Tips.GetVisualChildren(button).Where(a => a is Shape))
            {
                var brush = shape.Fill as SolidColorBrush;
                if (brush == null || brush.IsFrozen || brush.IsSealed)
                  shape.Fill = new SolidColorBrush();

                var ca = new ColorAnimation(color, AnimationHelper.SlowAnimationDuration);
                shape.Fill.BeginAnimation(SolidColorBrush.ColorProperty, ca);

                if (!(shape is Path))
                {
                    brush = shape.Stroke as SolidColorBrush;
                    if (brush == null || brush.IsFrozen || brush.IsSealed)
                        shape.Stroke = new SolidColorBrush();
                    shape.Stroke.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                }
            }
        }
        #endregion
    }
}
