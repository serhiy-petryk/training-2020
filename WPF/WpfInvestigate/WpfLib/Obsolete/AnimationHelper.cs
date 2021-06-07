using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WpfLib.Obsolete
{
    static class AnimationHelper
    {
        public static TimelineCollection[] GetPathAnimations(Path element, Geometry from, Geometry to)
        {
            var result = new[] { new TimelineCollection(), new TimelineCollection() };
            result[0].Add(CreateFrameAnimation(element, Path.DataProperty, to));
            result[1].Add(CreateFrameAnimation(element, Path.DataProperty, from));

            var widthPartAnimations = CreateAnimations(element, FrameworkElement.WidthProperty, FrameworkElement.WidthProperty);
            widthPartAnimations[0].SetFromToValues(element.Width, 0.0);
            widthPartAnimations[1].SetFromToValues(0.0, element.Width);
            widthPartAnimations[1].BeginTime = Common.AnimationHelper.AnimationDuration.TimeSpan;

            result[0].Add(widthPartAnimations[0]);
            result[0].Add(widthPartAnimations[1]);
            result[1].Add(widthPartAnimations[0]);
            result[1].Add(widthPartAnimations[1]);

            return result;
        }

        public static ObjectAnimationUsingKeyFrames CreateFrameAnimation(FrameworkElement element, DependencyProperty dataProperty, object value)
        {
            if (element == null || dataProperty == null || value == null)
                throw new NullReferenceException();
            if (!dataProperty.PropertyType.IsInstanceOfType(value))
                throw new Exception("GetFrameAnimation error. Data type of 'value' parameter doesn't match dataProperty.PropertyType.");

            var animation = new ObjectAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = value, KeyTime = Common.AnimationHelper.AnimationDuration.TimeSpan });

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath(dataProperty));
            return animation;
        }

        public static void SetFromToValues(this Timeline timeline, double from, double to)
        {
            ((DoubleAnimation)timeline).From = from;
            ((DoubleAnimation)timeline).To = to;
        }

        #region ================  Create animation  ===================
        public static Timeline[] CreateAnimations(this IAnimatable element, params DependencyProperty[] propertyPaths) =>
            propertyPaths.Select(p => CreateAnimation(element, new PropertyPath(p), p.PropertyType)).ToArray();
        public static Timeline CreateAnimation(this IAnimatable element, PropertyPath propertyPath, Type propertyType, Duration? duration = null)
        {
            AnimationTimeline animation = null;
            if (propertyType == typeof(double)) animation = new DoubleAnimation();
            else if (propertyType == typeof(Point)) animation = new PointAnimation();
            else if (propertyType == typeof(Thickness)) animation = new ThicknessAnimation();
            else if (propertyType == typeof(Color)) animation = new ColorAnimation();

            if (animation == null)
                throw new NotImplementedException();

            animation.Duration = duration ?? Common.AnimationHelper.AnimationDuration;
            Storyboard.SetTarget(animation, (DependencyObject)element);
            Storyboard.SetTargetProperty(animation, propertyPath);
            return animation;
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

        public static Task<bool> BeginAsync(this Storyboard storyboard, FrameworkElement target)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (storyboard != null && target != null)
            {
                var temp = target.CacheMode;
                target.CacheMode = new BitmapCache();
                var animation = storyboard.Clone();
                animation.Completed += (s, e) =>
                {
                    target.CacheMode = temp;
                    tcs.SetResult(true);
                };
                animation.Freeze();
                animation.Begin(target);
            }
            else
                tcs.SetResult(false);

            return tcs.Task;
        }
    }
}
