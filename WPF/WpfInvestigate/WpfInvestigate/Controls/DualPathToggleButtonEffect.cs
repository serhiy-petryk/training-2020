using System;
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
        private static void OnMarginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded,
                new Action(() =>
                {
                    if (d is ToggleButton tb && tb.Content is Grid grid &&
                        ((tb.IsChecked == true && e.Property == MarginOnProperty) ||
                         (tb.IsChecked != null && e.Property == MarginOffProperty)))
                        grid.Margin = (Thickness)e.NewValue;
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
                        OnToggleButtonUnloaded(tb, null);
                        tb.Checked += OnToggleButtonCheckChanged;
                        tb.Unchecked += OnToggleButtonCheckChanged;
                        tb.Unloaded += OnToggleButtonUnloaded;
                    }
                }));
            }
        }

        private static void Init(ToggleButton tb)
        {
            var grid = new Grid { ClipToBounds = true };
            grid.Margin = tb.IsChecked == true ? GetMarginOn(tb) : GetMarginOff(tb);
            var viewbox = new Viewbox { UseLayoutRounding = false };
            var path = new Path { Stretch = Stretch.Uniform };
            path.Data = tb.IsChecked == true ? GetGeometryOn(tb) : GetGeometryOff(tb);
            viewbox.Child = path;
            grid.Children.Add(viewbox);
            tb.Content = grid;

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                path.Fill = Tips.GetActualForegroundBrush(tb); // Delay for Tips.GetActualForegroundBrush(tb)
                CreateAnimation(grid);
            }));
        }

        private static void OnToggleButtonUnloaded(object sender, RoutedEventArgs e)
        {
            var tb = (ToggleButton)sender;
            tb.Unloaded -= OnToggleButtonUnloaded;
            tb.Checked -= OnToggleButtonCheckChanged;
            tb.Unchecked -= OnToggleButtonCheckChanged;
            if (e != null)
                tb.Content = null;
        }

        private static void OnToggleButtonCheckChanged(object sender, RoutedEventArgs e)
        {
            var tb = (ToggleButton)sender;
            var grid = tb.Content as Grid;

            var newGeometry = tb.IsChecked == true ? GetGeometryOn(tb) : GetGeometryOff(tb);
            var newMargin = tb.IsChecked == true ? GetMarginOn(tb) : GetMarginOff(tb);
            var sb = GetAnimation(grid, newGeometry, newMargin);
            sb.Begin();
        }

        //============= Animation service ===================
        private static void CreateAnimation(Grid grid)
        {
            var viewbox = (Viewbox)grid.Children[0];
            var path = (Path)viewbox.Child;
            if (!(viewbox.RenderTransform is ScaleTransform))
                viewbox.RenderTransform = new ScaleTransform(1, 1);
            ((ScaleTransform)viewbox.RenderTransform).CenterX = viewbox.ActualWidth / 2;

            var da1 = new DoubleAnimation { From = 1, To = 0, Duration = AnimationHelper.AnimationDuration };
            Storyboard.SetTarget(da1, viewbox);
            Storyboard.SetTargetProperty(da1, new PropertyPath("RenderTransform.ScaleX"));

            var da2 = new DoubleAnimation { From = 0, To = 1, BeginTime = AnimationHelper.AnimationDuration.TimeSpan, Duration = AnimationHelper.AnimationDuration };
            Storyboard.SetTarget(da2, viewbox);
            Storyboard.SetTargetProperty(da2, new PropertyPath("RenderTransform.ScaleX"));

            var storyboard = new Storyboard();
            storyboard.Children.Add(AnimationHelper.GetFrameAnimation(path, Path.DataProperty, Geometry.Empty));
            storyboard.Children.Add(AnimationHelper.GetFrameAnimation(grid, FrameworkElement.MarginProperty, new Thickness()));
            storyboard.Children.Add(da1);
            storyboard.Children.Add(da2);

            grid.Resources.Add("Animation", storyboard);
        }
        private static Storyboard GetAnimation(Grid grid, Geometry newGeometry, Thickness newMargin)
        {
            var storyboard = (Storyboard)grid.Resources["Animation"];
            ((ObjectAnimationUsingKeyFrames)storyboard.Children[0]).KeyFrames[0].Value = newGeometry;
            ((ObjectAnimationUsingKeyFrames)storyboard.Children[1]).KeyFrames[0].Value = newMargin;
            return storyboard;
        }
    }
}
