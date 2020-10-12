using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfInvestigate.Controls.Effects
{
    /// <summary>
    /// </summary>
    public class PathDataEffect
    {
        public static readonly DependencyProperty PathDataProperty = DependencyProperty.RegisterAttached(
            "PathData", typeof(Geometry), typeof(PathDataEffect), new UIPropertyMetadata(null, OnPathDataChanged));
        public static Geometry GetPathData(DependencyObject obj) => (Geometry)obj.GetValue(PathDataProperty);
        public static void SetPathData(DependencyObject obj, Geometry value) => obj.SetValue(PathDataProperty, value);

        private static void OnPathDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ContentControl control)
            {
                if (e.NewValue is Geometry geometry)
                {
                    var path = new Path { Stretch = Stretch.Uniform, Data = geometry };
                    var viewbox = new Viewbox {Child = path};
                    if (control.HasContent)
                    {
                        var grid = new Grid {VerticalAlignment = VerticalAlignment.Stretch, Background = Brushes.Red};
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        grid.Children.Add(viewbox);
                        Grid.SetColumn(viewbox, 0);
                        var oldContent = control.Content;
                        if (oldContent is string)
                        {
                            var textBlock = new TextBlock
                            {
                                Text = (string) oldContent, Foreground = Brushes.Black,
                                Margin = new Thickness(4, 0, 0, 0),
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            var b = new Binding("Foreground")
                            {
                                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Control), 1)
                            };
                            textBlock.SetBinding(Control.ForegroundProperty, b);
                            oldContent = textBlock;
                        }

                        control.Content = null;
                        grid.Children.Add(oldContent as UIElement);
                        Grid.SetColumn(oldContent as UIElement, 1);
                        control.Content = grid;
                    }
                    else
                        control.Content = viewbox;
                    /*var b = new Binding("Foreground")
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Control), 1)
                    };
                    path.SetBinding(Shape.FillProperty, b);*/
                }
                else
                    control.Content = null;
            }
        }
    }
}
