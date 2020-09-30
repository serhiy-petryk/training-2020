using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
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
            const string pathString = "FMLHVCQSTAZ+-,.0123456789fmlhvcqstaz";
            var s = ((string)o).Trim();
               if (string.IsNullOrEmpty(s) || s.Length < 3 || !"FfMm".Any(s[0].ToString().Contains) || !"Zz".Any(s[s.Length-1].ToString().Contains) ||
                   !(s.Contains(',') || s.Contains(' '))  || !s.All(c => pathString.Contains(c) || char.IsWhiteSpace(c)))
                    return null;

            try { return Geometry.Parse(s); }
            catch { return null; }
        }

        private void OnFlatButtonLoaded(object sender, RoutedEventArgs e)
        {
            var button = sender as ButtonBase;
            if (button != null)
            {
                OnFlatButtonUnloaded(sender, e);

                // if (DualPathToggleButtonEffect.GetGeometryOff(button) != Geometry.Empty)
                   // return;

                /*var geometry = GetGeometry(button.Content);
                if (geometry != null)
                {
                    var path = new Path {Stretch = Stretch.Uniform, Data = geometry};
                    var b = new Binding("Foreground")
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ContentControl), 1)
                    };
                    path.SetBinding(Shape.FillProperty, b);
                    button.Content = new Viewbox {Child = path};
                }*/

                button.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    button.Unloaded += OnFlatButtonUnloaded;
                    var dpd1 = DependencyPropertyDescriptor.FromProperty(Control.BackgroundProperty, typeof(Control));
                    dpd1.AddValueChanged(button, OnBackgroundChanged);
                    /*if (Tips.GetVisualChildren(button).Any(a => a is Shape))
                    {
                        SetColorsForShapes(button, null);
                        var dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                        dpd.AddValueChanged(button, SetColorsForShapes);
                        dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
                        dpd.AddValueChanged(button, SetColorsForShapes);
                    }*/
                    OnBackgroundChanged(button, null);
                }));
            }
        }

        private void OnBackgroundChanged(object sender, EventArgs e)
        {
            var button = sender as ButtonBase;
            if (button != null)
            {
                // Refresh button colors
                var buttonChild = VisualTreeHelper.GetChild(button, 0) as FrameworkElement;
                var stateGroup = (VisualStateManager.GetVisualStateGroups(buttonChild) as IList<VisualStateGroup>).FirstOrDefault(g => g.Name == "CommonStates");
                stateGroup?.CurrentState?.Storyboard.Begin(buttonChild);
                // Refresh shapes colors
                // SetColorsForShapes(sender, e);
            }
        }

        private void OnFlatButtonUnloaded(object sender, RoutedEventArgs e)
        {
            var button = sender as ButtonBase;
            if (button != null)
            {
                button.Loaded -= OnFlatButtonLoaded;
                button.Unloaded -= OnFlatButtonUnloaded;
                // var dpd = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(ButtonBase));
                // dpd.RemoveValueChanged(button, SetColorsForShapes);
                // dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(ButtonBase));
                // dpd.RemoveValueChanged(button, SetColorsForShapes);
                var dpd = DependencyPropertyDescriptor.FromProperty(Control.BackgroundProperty, typeof(Control));
                dpd.RemoveValueChanged(button, OnBackgroundChanged);
            }
        }
        private void SetColorsForShapes(object sender, EventArgs e)
        {
            var button = sender as ButtonBase;
            // var rippleColor = RippleEffect.GetRippleColor(button); // don't apply Pressed effect if control has ripple effect
            // var color = button.IsPressed && rippleColor == null ? Tips.GetActualBackgroundColor(button) : Tips.GetActualForegroundColor(button);
            // var color = button.IsPressed ? Tips.GetActualBackgroundColor(button) : Tips.GetActualForegroundColor(button);

            var newColor = Tips.GetActualBackgroundColor(button);
            /*if (button.IsPressed) { }
            else if (button.IsMouseOver)
                newColor = (Color)ColorHslBrush.Relative.Convert(button, typeof(Color), "+85%", null);
            else 
                newColor = (Color)ColorHslBrush.Relative.Convert(button, typeof(Color), "+70%", null);*/

            /*foreach (var shape in Tips.GetVisualChildren(button).OfType<Shape>())
            {
                var brush = shape.Fill as SolidColorBrush;
                if (brush == null || brush.IsFrozen || brush.IsSealed)
                  shape.Fill = new SolidColorBrush();

                var ca = new ColorAnimation(newColor, AnimationHelper.SlowAnimationDuration);
                shape.Fill.BeginAnimation(SolidColorBrush.ColorProperty, ca);

                if (!(shape is Path))
                {
                    brush = shape.Stroke as SolidColorBrush;
                    if (brush == null || brush.IsFrozen || brush.IsSealed)
                        shape.Stroke = new SolidColorBrush();
                    shape.Stroke.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                }
            }*/
        }
        #endregion
    }
}
