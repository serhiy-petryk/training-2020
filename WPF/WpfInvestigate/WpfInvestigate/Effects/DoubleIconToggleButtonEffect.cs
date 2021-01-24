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
using WpfInvestigate.Helpers;

namespace WpfInvestigate.Effects
{
    public class DoubleIconToggleButtonEffect
    {
        public static readonly DependencyProperty GeometryOnProperty = DependencyProperty.RegisterAttached("GeometryOn",
            typeof(Geometry), typeof(DoubleIconToggleButtonEffect), new UIPropertyMetadata(Geometry.Empty, OnGeometryPropertyChanged));
        public static Geometry GetGeometryOn(DependencyObject obj) => (Geometry)obj.GetValue(GeometryOnProperty);
        public static void SetGeometryOn(DependencyObject obj, Geometry value) => obj.SetValue(GeometryOnProperty, value);
        //================
        public static readonly DependencyProperty GeometryOffProperty = DependencyProperty.RegisterAttached("GeometryOff",
            typeof(Geometry), typeof(DoubleIconToggleButtonEffect), new UIPropertyMetadata(Geometry.Empty, OnGeometryPropertyChanged));
        public static Geometry GetGeometryOff(DependencyObject obj) => (Geometry)obj.GetValue(GeometryOffProperty);
        public static void SetGeometryOff(DependencyObject obj, Geometry value) => obj.SetValue(GeometryOffProperty, value);
        //================
        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached("Width",
            typeof(double), typeof(DoubleIconToggleButtonEffect), new UIPropertyMetadata(double.NaN, OnWidthPropertyChanged));
        public static double GetWidth(DependencyObject obj) => (double)obj.GetValue(WidthProperty);
        public static void SetWidth(DependencyObject obj, double value) => obj.SetValue(WidthProperty, value);
        //================
        public static readonly DependencyProperty MarginOnProperty = DependencyProperty.RegisterAttached("MarginOn",
            typeof(Thickness), typeof(DoubleIconToggleButtonEffect), new UIPropertyMetadata(new Thickness(), OnMarginPropertyChanged));
        public static Thickness GetMarginOn(DependencyObject obj) => (Thickness)obj.GetValue(MarginOnProperty);
        public static void SetMarginOn(DependencyObject obj, Thickness value) => obj.SetValue(MarginOnProperty, value);
        //================
        public static readonly DependencyProperty MarginOffProperty = DependencyProperty.RegisterAttached("MarginOff",
            typeof(Thickness), typeof(DoubleIconToggleButtonEffect), new UIPropertyMetadata(new Thickness(), OnMarginPropertyChanged));
        public static Thickness GetMarginOff(DependencyObject obj) => (Thickness)obj.GetValue(MarginOffProperty);
        public static void SetMarginOff(DependencyObject obj, Thickness value) => obj.SetValue(MarginOffProperty, value);

        //=====================================
        private static void OnWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                if (d is ToggleButton tb && GetViewbox(tb) is Viewbox viewbox)
                    viewbox.Width = (double)e.NewValue;
            }));
        }
        private static void OnMarginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                if (d is ToggleButton tb && (GetViewbox(tb) is Viewbox viewbox) &&
                    ((tb.IsChecked == true && e.Property == MarginOnProperty) ||
                     (tb.IsChecked != null && e.Property == MarginOffProperty)))
                    viewbox.Margin = (Thickness)e.NewValue;
            }));
        }

        private static void OnGeometryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleButton tb)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    if ((!(tb.Content is FrameworkElement) || GetViewbox(tb) == null) && GetGeometryOn(tb) != Geometry.Empty && GetGeometryOff(tb) != Geometry.Empty)
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
            ControlHelper.AddIconToControl(tb, false, tb.IsChecked == true ? GetGeometryOn(tb) : GetGeometryOff(tb),
                tb.IsChecked == true ? GetMarginOn(tb) : GetMarginOff(tb), GetWidth(tb));
        }

        private static void OnToggleButtonCheckChanged(object sender, RoutedEventArgs e)
        {
            var tb = (ToggleButton)sender;
            var newGeometry = tb.IsChecked == true ? GetGeometryOn(tb) : GetGeometryOff(tb);
            var newMargin = tb.IsChecked == true ? GetMarginOn(tb) : GetMarginOff(tb);
            var viewbox = GetViewbox(tb);

            var sb = (Storyboard)viewbox.Resources["Animation"];
            if (sb == null)
                sb = CreateAnimation(viewbox);
            ((ObjectAnimationUsingKeyFrames)sb.Children[0]).KeyFrames[0].Value = newGeometry;
            ((ObjectAnimationUsingKeyFrames)sb.Children[1]).KeyFrames[0].Value = newMargin;
            sb.Begin();
        }

        //============= Animation service ===================
        private static Storyboard CreateAnimation(Viewbox viewbox)
        {
            var path = (Path)viewbox.Child;
            if (!(viewbox.RenderTransform is ScaleTransform))
                viewbox.RenderTransform = new ScaleTransform(1, 1);
            viewbox.RenderTransformOrigin = new Point(0.5, 0.5);

            var a1 = viewbox.CreateAnimation(new PropertyPath("RenderTransform.ScaleX"), typeof(double));
            // var a11 = viewbox.CreateAnimations(ScaleTransform.ScaleXProperty)[0];
            a1.SetFromToValues(1.0, 0.0);
            var a2 = viewbox.CreateAnimation(new PropertyPath("RenderTransform.ScaleX"), typeof(double));
            // var a21 = viewbox.CreateAnimations(ScaleTransform.ScaleXProperty)[0];
            a2.BeginTime = a1.Duration.TimeSpan;
            a2.SetFromToValues(0.0, 1.0);

            var storyboard = new Storyboard();
            storyboard.Children.Add(AnimationHelper.CreateFrameAnimation(path, Path.DataProperty, Geometry.Empty));
            storyboard.Children.Add(AnimationHelper.CreateFrameAnimation(viewbox, FrameworkElement.MarginProperty, new Thickness()));
            storyboard.Children.Add(a1);
            storyboard.Children.Add(a2);

            viewbox.Resources.Add("Animation", storyboard);
            return storyboard;
        }

        // private static Viewbox GetViewbox(Grid grid) => grid.Children.OfType<Viewbox>().FirstOrDefault(vb => Grid.GetColumn(vb) == 1);
        private static Viewbox GetViewbox(ToggleButton tb) => Tips.GetVisualChildren(tb).OfType<Viewbox>().FirstOrDefault(vb => vb.Resources["IconViewBox"] is bool);
    }
}
