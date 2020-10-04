using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfInvestigate.Common
{
    #region ======  AnimationHelper  =====
    public static class AnimationHelper
    {
        public static readonly Duration AnimationDuration = TimeSpan.FromMilliseconds(120);
        public const double SlowAnimationTime = 240.0;
        public static readonly Duration SlowAnimationDuration = TimeSpan.FromMilliseconds(SlowAnimationTime);

        #region ========  Set From, To, duration values  ===================
        public static void SetFromToValues(this Timeline timeline, Color from, Color to)
        {
            ((ColorAnimation)timeline).From = from;
            ((ColorAnimation)timeline).To = to;
        }
        public static void SetFromToValues(this Timeline timeline, double from, double to)
        {
            ((DoubleAnimation)timeline).From = from;
            ((DoubleAnimation)timeline).To = to;
        }
        public static void SetFromToValues(this Timeline timeline, Point from, Point to)
        {
            ((PointAnimation)timeline).From = from;
            ((PointAnimation)timeline).To = to;
        }
        public static void SetFromToValues(this Timeline timeline, Thickness from, Thickness to)
        {
            ((ThicknessAnimation)timeline).From = from;
            ((ThicknessAnimation)timeline).To = to;
        }
        #endregion

        #region ================  Create animation  ===================
        public static Timeline CreateAnimation(this FrameworkElement element, DependencyProperty propertyPath, Duration? duration = null) =>
            CreateAnimation(element, new[] { propertyPath }, duration);
        public static Timeline CreateAnimation(this FrameworkElement element, DependencyProperty propertyPath, double duration) =>
            CreateAnimation(element, new[] { propertyPath }, TimeSpan.FromMilliseconds(duration));
        public static Timeline CreateAnimation(this FrameworkElement element, DependencyProperty[] propertyPath, double duration) =>
            CreateAnimation(element, propertyPath, TimeSpan.FromMilliseconds(duration));

        public static Timeline CreateAnimation(this FrameworkElement element, DependencyProperty[] propertyPath, Duration? duration = null)
        {
            var dataProperty = propertyPath[propertyPath.Length - 1];
            AnimationTimeline animation = null;
            if (dataProperty.PropertyType == typeof(double)) animation = new DoubleAnimation();
            else if (dataProperty.PropertyType == typeof(Point)) animation = new PointAnimation();
            else if (dataProperty.PropertyType == typeof(Thickness)) animation = new ThicknessAnimation();
            else if (dataProperty.PropertyType == typeof(Color)) animation = new ColorAnimation();

            if (animation == null)
                throw new NotImplementedException();

            animation.Duration = duration ?? AnimationDuration;
            Storyboard.SetTarget(animation, element);
            var path = string.Join(".", propertyPath.Select((x, index) => $"({index})"));
            Storyboard.SetTargetProperty(animation, new PropertyPath(path, propertyPath));
            return animation;
        }
        #endregion

        public static Timeline GetScrollViewerVerticalOffsetAnimation(ScrollViewer element, double from, double to, FillBehavior fillBehavior = FillBehavior.HoldEnd) =>
            GetFromToAnimation(element, ScrollViewerAnimator.VerticalOffsetProperty, from, to, fillBehavior);
        public static Timeline GetMarginAnimation(FrameworkElement element, Thickness from, Thickness to, FillBehavior fillBehavior = FillBehavior.HoldEnd) =>
            GetFromToAnimation(element, FrameworkElement.MarginProperty, from, to, fillBehavior);
        public static Timeline GetWidthAnimation(FrameworkElement element, double from, double to, FillBehavior fillBehavior = FillBehavior.HoldEnd) =>
            GetFromToAnimation(element, FrameworkElement.WidthProperty, from, to, fillBehavior);
        public static Timeline GetHeightAnimation(FrameworkElement element, double from, double to, FillBehavior fillBehavior = FillBehavior.HoldEnd) =>
            GetFromToAnimation(element, FrameworkElement.HeightProperty, from, to, fillBehavior);
        public static Timeline GetLeftAnimation(FrameworkElement element, double from, double to, FillBehavior fillBehavior = FillBehavior.HoldEnd) =>
            GetFromToAnimation(element, Canvas.LeftProperty, from, to, fillBehavior);
        public static Timeline GetWindowLeftAnimation(Window element, double from, double to, FillBehavior fillBehavior = FillBehavior.HoldEnd) =>
            GetFromToAnimation(element, Window.LeftProperty, from, to, fillBehavior);
        public static Timeline GetTopAnimation(FrameworkElement element, double from, double to, FillBehavior fillBehavior = FillBehavior.HoldEnd) =>
            GetFromToAnimation(element, Canvas.TopProperty, from, to, fillBehavior);
        public static Timeline GetWindowTopAnimation(Window element, double from, double to, FillBehavior fillBehavior = FillBehavior.HoldEnd) =>
            GetFromToAnimation(element, Window.TopProperty, from, to, fillBehavior);
        public static Timeline GetOpacityAnimation(FrameworkElement element, double from, double to, FillBehavior fillBehavior = FillBehavior.HoldEnd) =>
            GetFromToAnimation(element, UIElement.OpacityProperty, from, to, fillBehavior);
        //public static Timeline GetPositionAnimation(MwiChild element, Point from, Point to, FillBehavior fillBehavior = FillBehavior.HoldEnd) =>
        //  GetFromToAnimation(element, MwiChild.PositionProperty, from, to, fillBehavior);
        public static LinearGradientBrush RunLinearGradientBrushAnimation(LinearGradientBrush newBrush, LinearGradientBrush oldBrush)
        {
            // usage: tabItem.Background = AnimationHelper.RunLinearGradientBrushAnimation(newBrush, (LinearGradientBrush)tabItem.Background);
            if (newBrush.GradientStops.Count != oldBrush.GradientStops.Count)
                throw new Exception("RunLinearGradientBrushAnimation error! Different size of newBrush and oldBrush GradientStops collection");

            newBrush = newBrush.Clone();
            for (var k = 0; k < newBrush.GradientStops.Count; k++)
            {
                var ca = new ColorAnimation(oldBrush.GradientStops[k].Color, newBrush.GradientStops[k].Color, AnimationDuration);
                newBrush.GradientStops[k].BeginAnimation(GradientStop.ColorProperty, ca);
            }
            return newBrush;
        }

        public static ObjectAnimationUsingKeyFrames GetFrameAnimation(FrameworkElement element, DependencyProperty dataProperty, object value)
        {
            if (element == null || dataProperty == null || value == null)
                throw new NullReferenceException();
            if (!dataProperty.PropertyType.IsInstanceOfType(value))
                throw new Exception("GetFrameAnimation error. Data type of 'value' parameter doesn't match dataProperty.PropertyType.");

            var animation = new ObjectAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = value, KeyTime = AnimationDuration.TimeSpan });

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath(dataProperty));
            return animation;
        }

        //====================   Private section   ===================
        private static Timeline GetFromToAnimation(FrameworkElement element, DependencyProperty dataProperty, object from, object to, FillBehavior fillBehavior = FillBehavior.HoldEnd)
        {
            if (element == null || dataProperty == null || from == null || to == null)
                throw new NullReferenceException();
            if (!dataProperty.PropertyType.IsInstanceOfType(from) || !dataProperty.PropertyType.IsInstanceOfType(to))
                throw new Exception("GetFromToAnimation error. Data type of 'from'/'to' parameter doesn't match dataProperty.PropertyType.");

            AnimationTimeline animation = null;
            if (dataProperty.PropertyType == typeof(double))
                animation = new DoubleAnimation { From = (double)from, To = (double)to };
            else if (dataProperty.PropertyType == typeof(Point))
                animation = new PointAnimation { From = (Point)from, To = (Point)to };
            else if (dataProperty.PropertyType == typeof(Thickness))
                animation = new ThicknessAnimation { From = (Thickness)from, To = (Thickness)to };

            if (animation != null)
            {
                animation.Duration = AnimationDuration;
                animation.FillBehavior = fillBehavior;
                Storyboard.SetTarget(animation, element);
                Storyboard.SetTargetProperty(animation, new PropertyPath(dataProperty));
                return animation;
            }

            throw new NotImplementedException();
        }
    }
    #endregion 

    #region =====  ScrollViewerAnimator  =====
    // Helper class to animate ScrollViewer.Horizontal/Vertical Offset
    // from https://marlongrech.wordpress.com/2009/09/14/how-to-set-wpf-scrollviewer-verticaloffset-and-horizontal-offset/
    public class ScrollViewerAnimator
    {
        // #region HorizontalOffset
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ScrollViewerAnimator), new FrameworkPropertyMetadata((double)0.0, OnHorizontalOffsetChanged));
        public static double GetHorizontalOffset(DependencyObject d) => (double)d.GetValue(HorizontalOffsetProperty);
        public static void SetHorizontalOffset(DependencyObject d, double value) => d.SetValue(HorizontalOffsetProperty, value);
        private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((ScrollViewer)d).ScrollToHorizontalOffset((double)e.NewValue);

        // #region VerticalOffset
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerAnimator), new FrameworkPropertyMetadata((double)0.0, OnVerticalOffsetChanged));
        public static double GetVerticalOffset(DependencyObject d) => (double)d.GetValue(VerticalOffsetProperty);
        public static void SetVerticalOffset(DependencyObject d, double value) => d.SetValue(VerticalOffsetProperty, value);
        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((ScrollViewer)d).ScrollToVerticalOffset((double)e.NewValue);
    }
    #endregion

}
