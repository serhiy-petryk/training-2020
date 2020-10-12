using System;
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
                var path = new Path { Stretch = Stretch.Uniform, Data = geometry };
                var viewbox = new Viewbox { Child = path };
                if (control.Content is string || control.Content is UIElement)
                {
                    viewbox.Margin = new Thickness(0, 0, 4, 0);
                    var panel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    panel.Children.Add(viewbox);

                    var content = control.Content;
                    if (content is string)
                    {
                        var textBlock = new TextBlock{ Text = (string)content, VerticalAlignment = VerticalAlignment.Center };
                        var b = new Binding("Foreground")
                        {
                            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Control), 1)
                        };
                        textBlock.SetBinding(Control.ForegroundProperty, b);
                        content = textBlock;
                    }

                    control.Content = null;
                    panel.Children.Add(content as UIElement);
                    control.Content = panel;
                }
                else
                    control.Content = viewbox;
            }));
        }
    }
}

