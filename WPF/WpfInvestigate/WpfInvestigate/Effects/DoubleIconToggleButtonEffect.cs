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
using WpfInvestigate.Controls.Helpers;

namespace WpfInvestigate.Effects
{
    public class DoubleIconToggleButtonEffect
    {
        public static readonly DependencyProperty PathOnProperty = DependencyProperty.RegisterAttached("PathOn",
            typeof(Path), typeof(DoubleIconToggleButtonEffect), new UIPropertyMetadata(null, OnPathPropertyChanged));
        public static Path GetPathOn(DependencyObject obj) => (Path)obj.GetValue(PathOnProperty);
        public static void SetPathOn(DependencyObject obj, Path value) => obj.SetValue(PathOnProperty, value);
        //=============
        public static readonly DependencyProperty PathOffProperty = DependencyProperty.RegisterAttached("PathOff",
            typeof(Path), typeof(DoubleIconToggleButtonEffect), new UIPropertyMetadata(null, OnPathPropertyChanged));
        public static Path GetPathOff(DependencyObject obj) => (Path)obj.GetValue(PathOffProperty);
        public static void SetPathOff(DependencyObject obj, Path value) => obj.SetValue(PathOffProperty, value);
        //==============
        public static readonly DependencyProperty ScaleOnProperty = DependencyProperty.RegisterAttached(
            "ScaleOn", typeof(Size), typeof(DoubleIconToggleButtonEffect), new UIPropertyMetadata(Size.Empty, OnPathPropertyChanged));
        public static Size GetScaleOn(DependencyObject obj) => (Size)obj.GetValue(ScaleOnProperty);
        public static void SetScaleOn(DependencyObject obj, Size value) => obj.SetValue(ScaleOnProperty, value);
        //==============
        public static readonly DependencyProperty ScaleOffProperty = DependencyProperty.RegisterAttached(
            "ScaleOff", typeof(Size), typeof(DoubleIconToggleButtonEffect), new UIPropertyMetadata(Size.Empty, OnPathPropertyChanged));
        public static Size GetScaleOff(DependencyObject obj) => (Size)obj.GetValue(ScaleOffProperty);
        public static void SetScaleOff(DependencyObject obj, Size value) => obj.SetValue(ScaleOffProperty, value);

        private static void OnPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleButton tb)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    if ((!(tb.Content is FrameworkElement element) || !(GetViewbox(tb) is Viewbox)) && GetPathOn(tb) != null && GetPathOff(tb) != null)
                    {
                        var a1 = GetPathOn(tb).Data;
                        var a2 = GetPathOff(tb).Data;

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
            //ControlHelper.AddIconToControl(tb, false, tb.IsChecked == true ? GetGeometryOn(tb) : GetGeometryOff(tb),
              //  tb.IsChecked == true ? GetMarginOn(tb) : GetMarginOff(tb), GetWidth(tb));
            ControlHelper.AddPathToControl(tb, tb.IsChecked == true ? GetPathOn(tb) : GetPathOff(tb),
                tb.IsChecked == true ? GetScaleOn(tb) : GetScaleOff(tb), false);
        }

        private static void OnToggleButtonCheckChanged(object sender, RoutedEventArgs e)
        {
            var tb = (ToggleButton)sender;
            var viewbox = GetViewbox(tb);
            if (viewbox == null) return;

            var newPath = tb.IsChecked == true ? GetPathOn(tb) : GetPathOff(tb);
            var oldScale = tb.IsChecked == true ? GetScaleOff(tb) : GetScaleOn(tb);
            if (oldScale.IsEmpty)
                oldScale = new Size(1.0, 1.0);
            
            var newScale = tb.IsChecked == true ? GetScaleOn(tb) : GetScaleOff(tb);
            if (newScale.IsEmpty)
                newScale = new Size(1.0, 1.0);

            var storyboard = (Storyboard)viewbox.Resources["Animation"] ?? CreateAnimation(viewbox);
            var firstAnimation = ((DoubleAnimation) storyboard.Children[0]);
            firstAnimation.CurrentStateInvalidated += AnimationStateHandler;
            firstAnimation.From = oldScale.Width;
            ((DoubleAnimation)storyboard.Children[1]).To = newScale.Width;
            storyboard?.Begin();

            void AnimationStateHandler(object sender2, EventArgs args2)
            {
                firstAnimation.CurrentStateInvalidated -= AnimationStateHandler;
                var clock = (AnimationClock) sender2;
                if (clock.CurrentState == ClockState.Filling)
                {
                    viewbox.Child = newPath;
                    ((ScaleTransform)viewbox.RenderTransform).ScaleX = 0.0;
                    ((ScaleTransform)viewbox.RenderTransform).ScaleY = newScale.Height;
                }
            }
        }

        private static Storyboard CreateAnimation(Viewbox viewbox)
        {
            if (!(viewbox.RenderTransform is ScaleTransform))
                viewbox.RenderTransform = new ScaleTransform(1, 1);

            var a1 = viewbox.CreateAnimation(new PropertyPath("RenderTransform.ScaleX"), typeof(double));
            a1.SetFromToValues(0.0, 0.0);
            var a2 = viewbox.CreateAnimation(new PropertyPath("RenderTransform.ScaleX"), typeof(double));
            a2.BeginTime = a1.Duration.TimeSpan;
            a2.SetFromToValues(0.0, 0.0);
            
            var storyboard = new Storyboard();
            storyboard.Children.Add(a1);
            storyboard.Children.Add(a2);

            viewbox.Resources.Add("Animation", storyboard);
            return storyboard;
        }

        //=============================
        //=========  OLD  =============
        //=============================
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
                if (d is ToggleButton tb && XXGetViewbox(tb) is Viewbox viewbox)
                    viewbox.Width = (double)e.NewValue;
            }));
        }
        private static void OnMarginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                if (d is ToggleButton tb && (XXGetViewbox(tb) is Viewbox viewbox) &&
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
                    if ((!(tb.Content is FrameworkElement element) || !(XXGetViewbox(tb) is Viewbox)) && GetGeometryOn(tb) != Geometry.Empty && GetGeometryOff(tb) != Geometry.Empty)
                    {
                        XXInit(tb);
                        tb.Checked -= XXOnToggleButtonCheckChanged;
                        tb.Unchecked -= XXOnToggleButtonCheckChanged;
                        tb.Checked += XXOnToggleButtonCheckChanged;
                        tb.Unchecked += XXOnToggleButtonCheckChanged;
                    }
                }));
            }
        }

        private static void XXInit(ToggleButton tb)
        {
            ControlHelper.AddIconToControl(tb, false, tb.IsChecked == true ? GetGeometryOn(tb) : GetGeometryOff(tb),
                tb.IsChecked == true ? GetMarginOn(tb) : GetMarginOff(tb), GetWidth(tb));
        }

        private static void XXOnToggleButtonCheckChanged(object sender, RoutedEventArgs e)
        {
            var tb = (ToggleButton)sender;
            var newGeometry = tb.IsChecked == true ? GetGeometryOn(tb) : GetGeometryOff(tb);
            var newMargin = tb.IsChecked == true ? GetMarginOn(tb) : GetMarginOff(tb);
            var sb = XXGetAnimation(XXGetViewbox(tb), newGeometry, newMargin);
            sb?.Begin();
        }

        //============= Animation service ===================
        private static Storyboard XXCreateAnimation(Viewbox viewbox)
        {
            var path = (Path)viewbox.Child;
            if (!(viewbox.RenderTransform is ScaleTransform))
                viewbox.RenderTransform = new ScaleTransform(1, 1);
            // ((ScaleTransform)viewbox.RenderTransform).CenterX = viewbox.ActualWidth / 2;
            viewbox.RenderTransformOrigin = new Point(0.5, 0.5);

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

            viewbox.Resources.Add("Animation", storyboard);
            return storyboard;
        }

        private static Storyboard XXGetAnimation(Viewbox viewbox, Geometry newGeometry, Thickness newMargin)
        {
            if (viewbox == null) return null;

            var storyboard = (Storyboard)viewbox.Resources["Animation"];
            if (storyboard == null)
            {
                storyboard = XXCreateAnimation(viewbox);
                if (storyboard == null)
                    return null;
            }

            ((ObjectAnimationUsingKeyFrames)storyboard.Children[0]).KeyFrames[0].Value = newGeometry;
            ((ObjectAnimationUsingKeyFrames)storyboard.Children[1]).KeyFrames[0].Value = newMargin;
            return storyboard;
        }

        // private static Viewbox GetViewbox(Grid grid) => grid.Children.OfType<Viewbox>().FirstOrDefault(vb => Grid.GetColumn(vb) == 1);
        private static Viewbox XXGetViewbox(ToggleButton tb) => Tips.GetVisualChildren(tb).OfType<Viewbox>().FirstOrDefault(vb => vb.Resources["IconViewBox"] is bool);
        private static Viewbox GetViewbox(ToggleButton tb) => Tips.GetVisualChildren(tb).OfType<Viewbox>().FirstOrDefault(vb => vb.Resources["PathViewBox"] is bool);
    }
}
