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
            "Geometry", typeof(Geometry), typeof(IconEffect), new UIPropertyMetadata(null, OnGeometryChanged));

        public static Geometry GetGeometry(DependencyObject obj) => (Geometry)obj.GetValue(GeometryProperty);
        public static void SetGeometry(DependencyObject obj, Geometry value) => obj.SetValue(GeometryProperty, value);

        private static void OnGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ContentControl control) || !(e.NewValue is Geometry geometry)) return;

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                ControlHelper.AddIconToControl(control, true, geometry, null,
                    new Thickness(control.Padding.Left, control.Padding.Top,  control.HasContent ? 0 : control.Padding.Right, control.Padding.Bottom));
            }));
        }
    }
}

