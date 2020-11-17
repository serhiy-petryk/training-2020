using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfInvestigate.Controls.Helpers;

namespace WpfInvestigate.Effects
{
    /// <summary>
    /// </summary>
    public class IconEffect
    {
        public static readonly DependencyProperty PathProperty = DependencyProperty.RegisterAttached(
            "Path", typeof(Path), typeof(IconEffect), new UIPropertyMetadata(null, OnPropertiesChanged));
        public static Path GetPath(DependencyObject obj) => (Path)obj.GetValue(PathProperty);
        public static void SetPath(DependencyObject obj, Path value) => obj.SetValue(PathProperty, value);

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.RegisterAttached(
            "Scale", typeof(Size), typeof(IconEffect), new UIPropertyMetadata(Size.Empty, OnPropertiesChanged));
        public static Size GetScale(DependencyObject obj) => (Size)obj.GetValue(ScaleProperty);
        public static void SetScale(DependencyObject obj, Size value) => obj.SetValue(ScaleProperty, value);

        //================
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.RegisterAttached(
            "Geometry", typeof(Geometry), typeof(IconEffect), new UIPropertyMetadata(null, OnGeometryPropertiesChanged));
        public static Geometry GetGeometry(DependencyObject obj) => (Geometry)obj.GetValue(GeometryProperty);
        public static void SetGeometry(DependencyObject obj, Geometry value) => obj.SetValue(GeometryProperty, value);

        //==================
        public static readonly DependencyProperty MarginIfHasContentProperty = DependencyProperty.RegisterAttached("MarginIfHasContent",
            typeof(Thickness), typeof(IconEffect), new UIPropertyMetadata(new Thickness(), OnGeometryPropertiesChanged));
        public static Thickness GetMarginIfHasContent(DependencyObject obj) => (Thickness)obj.GetValue(MarginIfHasContentProperty);
        public static void SetMarginIfHasContent(DependencyObject obj, Thickness value) => obj.SetValue(MarginIfHasContentProperty, value);
        //==================
        private static void OnGeometryPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ContentControl control) || !(e.NewValue is Geometry geometry)) return;

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                ControlHelper.AddIconToControl(control, true, geometry, control.HasContent ? GetMarginIfHasContent(control) : control.Padding);
            }));
        }

        //======================
        private static void OnPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ContentControl control) || !(e.NewValue is Path && e.Property == PathProperty ||
                                                    e.NewValue is Size && e.Property == ScaleProperty)) return;

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => 
                ControlHelper.AddPathToControl(control, GetPath(control), GetScale(control), true)));
        }
    }
}

