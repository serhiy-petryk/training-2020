using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfInvestigate.Common;
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
            if (!(d is ContentControl control) || !(e.NewValue is Path path)) return;

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                // AddPathToControl(control, path, true, control.HasContent ? GetMarginIfHasContent(control) : control.Padding);
                SetPathInControl(control);
            }));
        }

        private static void SetScaleInViewbox(FrameworkElement viewbox, Size scale)
        {
            if (scale.IsEmpty)
            {
                viewbox.RenderTransform = Transform.Identity;
            }
            else
            {
                viewbox.RenderTransformOrigin = new Point(0.5, 0.5);
                viewbox.RenderTransform = new ScaleTransform(scale.Width, scale.Height);
            }
        }
        private static void SetPathInControl(ContentControl control, bool iconBeforeContent = true)
        {
            var path = GetPath(control);
            var scale = GetScale(control);

            if (control.Resources["AddedPath"] is bool)
            { // icon already exists
                var oldViewBox = Tips.GetVisualChildren(control).OfType<Viewbox>().FirstOrDefault(vb => vb.Resources["PathViewBox"] is bool);
                if (oldViewBox != null)
                {
                    // wrong Margin/Width, якщо повторно виконуємо метод => потрібно ускладнити обробку (??? чи це потрібно)
                    // oldViewBox.Margin = iconMargin;
                    // oldViewBox.Width = iconWidth;
                    if (oldViewBox.Child is Path oldPath && oldPath != path)
                        oldViewBox.Child = path;
                    SetScaleInViewbox(path, scale);
                }
                return;
            }
            control.Resources["AddedPath"] = true;

            // var path = new Path { Stretch = Stretch.Uniform, Margin = new Thickness(), Data = icon };
            /*path.SetBinding(Shape.FillProperty, new Binding("Foreground")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Control), 1)
            });*/
            var viewbox = new Viewbox
            {
                Child = path,
                VerticalAlignment = VerticalAlignment.Stretch,
                // Margin = iconMargin,
                Resources = { ["PathViewBox"] = true }
            };

            if (control.HasContent)
            {
                var grid = new Grid { ClipToBounds = true, Margin = new Thickness(), SnapsToDevicePixels = true };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = iconBeforeContent ? GridLength.Auto : new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = !iconBeforeContent ? GridLength.Auto : new GridLength(1, GridUnitType.Star) });

                grid.Children.Add(viewbox);
                Grid.SetColumn(viewbox, iconBeforeContent ? 0 : 1);

                var contentControl = new ContentPresenter
                {
                    Content = control.Content,
                    Margin = control.Padding,
                    VerticalAlignment = control.VerticalContentAlignment,
                    HorizontalAlignment = control.HorizontalContentAlignment
                };
                control.Content = null;
                control.Padding = new Thickness();
                // tb.VerticalContentAlignment = VerticalAlignment.Stretch;
                control.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                grid.Children.Add(contentControl);
                Grid.SetColumn(contentControl, iconBeforeContent ? 1 : 0);

                control.Content = grid;
            }
            else
                control.Content = viewbox;

            SetScaleInViewbox(path, scale);
        }


    }
}

