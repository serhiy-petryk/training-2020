using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WpfInvestigate.Controls.Helpers;

namespace WpfInvestigate.Controls.Effects
{
    /// <summary>
    /// </summary>
    public class IconEffect
    {
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.RegisterAttached(
            "Geometry", typeof(Geometry), typeof(IconEffect), new UIPropertyMetadata(null, OnPropertiesChanged));

        public static Geometry GetGeometry(DependencyObject obj) => (Geometry)obj.GetValue(GeometryProperty);
        public static void SetGeometry(DependencyObject obj, Geometry value) => obj.SetValue(GeometryProperty, value);

        //==================
        public static readonly DependencyProperty PaddingIfHasContentProperty = DependencyProperty.RegisterAttached("PaddingIfHasContent",
            typeof(Thickness), typeof(DualPathToggleButtonEffect), new UIPropertyMetadata(new Thickness(), OnPropertiesChanged));
        public static Thickness GetPaddingIfHasContent(DependencyObject obj) => (Thickness)obj.GetValue(PaddingIfHasContentProperty);
        public static void SetPaddingIfHasContent(DependencyObject obj, Thickness value) => obj.SetValue(PaddingIfHasContentProperty, value);
        //==================
        private static void OnPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ContentControl control) || !(e.NewValue is Geometry geometry)) return;

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                ControlHelper.AddIconToControl(control, true, geometry, control.HasContent ? GetPaddingIfHasContent(control) : control.Padding);
            }));
        }
    }
}

