using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public class DualPathToggleButtonEffect
    {
        public static readonly DependencyProperty GeometryOnProperty = DependencyProperty.RegisterAttached("GeometryOn",
            typeof(Geometry), typeof(DualPathToggleButtonEffect), new UIPropertyMetadata(Geometry.Empty, OnGeometryPropertyChanged));
        public static Geometry GetGeometryOn(DependencyObject obj) => (Geometry)obj.GetValue(GeometryOnProperty);
        public static void SetGeometryOn(DependencyObject obj, Geometry value) => obj.SetValue(GeometryOnProperty, value);
        //================
        public static readonly DependencyProperty GeometryOffProperty = DependencyProperty.RegisterAttached("GeometryOff",
            typeof(Geometry), typeof(DualPathToggleButtonEffect), new UIPropertyMetadata(Geometry.Empty, OnGeometryPropertyChanged));
        public static Geometry GetGeometryOff(DependencyObject obj) => (Geometry)obj.GetValue(GeometryOffProperty);
        public static void SetGeometryOff(DependencyObject obj, Geometry value) => obj.SetValue(GeometryOffProperty, value);
        //================
        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached("Width",
            typeof(double), typeof(DualPathToggleButtonEffect), new UIPropertyMetadata(double.NaN, OnWidthPropertyChanged));
        public static double GetWidth(DependencyObject obj) => (double)obj.GetValue(WidthProperty);
        public static void SetWidth(DependencyObject obj, double value) => obj.SetValue(WidthProperty, value);
        //================
        public static readonly DependencyProperty MarginOnProperty = DependencyProperty.RegisterAttached("MarginOn",
            typeof(Thickness), typeof(DualPathToggleButtonEffect), new UIPropertyMetadata(new Thickness(), OnMarginPropertyChanged));
        public static Thickness GetMarginOn(DependencyObject obj) => (Thickness)obj.GetValue(MarginOnProperty);
        public static void SetMarginOn(DependencyObject obj, Thickness value) => obj.SetValue(MarginOnProperty, value);
        //================
        public static readonly DependencyProperty MarginOffProperty = DependencyProperty.RegisterAttached("MarginOff",
            typeof(Thickness), typeof(DualPathToggleButtonEffect), new UIPropertyMetadata(new Thickness(), OnMarginPropertyChanged));
        public static Thickness GetMarginOff(DependencyObject obj) => (Thickness)obj.GetValue(MarginOffProperty);
        public static void SetMarginOff(DependencyObject obj, Thickness value) => obj.SetValue(MarginOffProperty, value);

        //=====================================
        private static void OnWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded,
                new Action(() =>
                {
                    if (d is ToggleButton tb && tb.Content is Grid grid)
                    {
                        var viewbox = GetViewbox(grid);
                        if (viewbox != null)
                            viewbox.Width = (double)e.NewValue;
                    }
                }));
        }
        private static void OnMarginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded,
                new Action(() =>
                {
                    if (d is ToggleButton tb && tb.Content is Grid grid &&
                        ((tb.IsChecked == true && e.Property == MarginOnProperty) ||
                         (tb.IsChecked != null && e.Property == MarginOffProperty)))
                    {
                        var viewbox = GetViewbox(grid);
                        if (viewbox != null)
                            viewbox.Margin = (Thickness) e.NewValue;
                    }
                }));
        }

        private static void OnGeometryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleButton tb)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    if (!(tb.Content is Grid) && GetGeometryOn(tb) != Geometry.Empty && GetGeometryOff(tb) != Geometry.Empty)
                    {
                        Init(tb);
                        tb.Checked -= OnToggleButtonCheckChanged;
                        tb.Unchecked -= OnToggleButtonCheckChanged;
                        tb.Checked += OnToggleButtonCheckChanged;
                        tb.Unchecked += OnToggleButtonCheckChanged;
                    }
                }));
            }
        }

        private static void Init(ToggleButton tb)
        {
            var grid = new Grid {ClipToBounds = true, Margin = new Thickness()};
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var path = new Path
            {
                Stretch = Stretch.Uniform, Margin = new Thickness(),
                Data = tb.IsChecked == true ? GetGeometryOn(tb) : GetGeometryOff(tb)
            };
            var viewbox = new Viewbox
            {
                Margin = tb.IsChecked == true ? GetMarginOn(tb) : GetMarginOff(tb),
                Child = path, Width = GetWidth(tb), VerticalAlignment = VerticalAlignment.Stretch
            };

            if (tb.Content != null)
            {
                var contentControl = new ContentPresenter
                {
                    Content = tb.Content, Margin = tb.Padding, VerticalAlignment = tb.VerticalContentAlignment,
                    HorizontalAlignment = tb.HorizontalContentAlignment
                };
                tb.Content = null;
                tb.Padding = new Thickness();
                // tb.VerticalContentAlignment = VerticalAlignment.Stretch;
                tb.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                grid.Children.Add(contentControl);
                Grid.SetColumn(contentControl, 0);
            }

            grid.Children.Add(viewbox);
            Grid.SetColumn(viewbox, 1);
            tb.Content = grid;

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                path.Fill = Tips.GetActualForegroundBrush(tb); // Delay in Tips.GetActualForegroundBrush(tb)
            }));
        }

        private static void OnToggleButtonCheckChanged(object sender, RoutedEventArgs e)
        {
            var tb = (ToggleButton)sender;
            var grid = tb.Content as Grid;

            var newGeometry = tb.IsChecked == true ? GetGeometryOn(tb) : GetGeometryOff(tb);
            var newMargin = tb.IsChecked == true ? GetMarginOn(tb) : GetMarginOff(tb);
            var sb = GetAnimation(grid, newGeometry, newMargin);
            sb?.Begin();
        }

        //============= Animation service ===================
        private static Storyboard CreateAnimation(Grid grid)
        {
            var viewbox = GetViewbox(grid);
            if (viewbox == null)
                return null;

            var path = (Path)viewbox.Child;
            if (!(viewbox.RenderTransform is ScaleTransform))
                viewbox.RenderTransform = new ScaleTransform(1, 1);
            ((ScaleTransform)viewbox.RenderTransform).CenterX = viewbox.ActualWidth / 2;

            var a1 = viewbox.CreateAnimation(new PropertyPath("RenderTransform.ScaleX"), typeof(double));
            a1.SetFromToValues(1.0, 0.0);
            var a2 = viewbox.CreateAnimation(new PropertyPath("RenderTransform.ScaleX"), typeof(double));
            a2.BeginTime = a1.Duration.TimeSpan;
            a2.SetFromToValues(0.0, 1.0);

            var storyboard = new Storyboard();
            storyboard.Children.Add(AnimationHelper.CreateFrameAnimation(path, Path.DataProperty, Geometry.Empty));
            storyboard.Children.Add(AnimationHelper.CreateFrameAnimation(viewbox, FrameworkElement.MarginProperty, new Thickness()));
            storyboard.Children.Add(a1);
            storyboard.Children.Add(a2);

            grid.Resources.Add("Animation", storyboard);
            return storyboard;
        }

        private static Storyboard GetAnimation(Grid grid, Geometry newGeometry, Thickness newMargin)
        {
            var storyboard = (Storyboard)grid.Resources["Animation"];
            if (storyboard == null)
            {
                storyboard = CreateAnimation(grid);
                if (storyboard == null)
                    return null;
            }

            ((ObjectAnimationUsingKeyFrames)storyboard.Children[0]).KeyFrames[0].Value = newGeometry;
            ((ObjectAnimationUsingKeyFrames)storyboard.Children[1]).KeyFrames[0].Value = newMargin;
            return storyboard;
        }

        private static Viewbox GetViewbox(Grid grid) => grid.Children.OfType<Viewbox>().FirstOrDefault(vb => Grid.GetColumn(vb) == 1);
    }
}
