using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfSpLib.Common;

namespace WpfSpLib.Helpers
{
    public static class AnimationHelper
    {
        public const double AnimationTime = 120.0;
        public static readonly Duration AnimationDuration = TimeSpan.FromMilliseconds(AnimationTime);
        public static readonly Duration AnimationDurationSlow = TimeSpan.FromMilliseconds(AnimationTime * 2);

        public static LinearGradientBrush BeginLinearGradientBrushAnimation(LinearGradientBrush newBrush, LinearGradientBrush oldBrush)
        {
            // usage: tabItem.Background = AnimationHelper.RunLinearGradientBrushAnimation(newBrush, (LinearGradientBrush)tabItem.Background);
            if (newBrush.GradientStops.Count != oldBrush.GradientStops.Count)
                throw new Exception("RunLinearGradientBrushAnimation error! Different size of newBrush and oldBrush GradientStops collection");

            newBrush = newBrush.Clone();
            for (var k = 0; k < newBrush.GradientStops.Count; k++)
            {
                var a = new ColorAnimation(oldBrush.GradientStops[k].Color, newBrush.GradientStops[k].Color, AnimationDuration);
                a.Freeze();
                newBrush.GradientStops[k].BeginAnimation(GradientStop.ColorProperty, a);
            }

            return newBrush;
        }

        #region ================  Begin animation async ===================
        public static Task BeginFrameAnimationAsync(this IAnimatable element, DependencyProperty property, object value)
        {
            if (element == null || property == null || value == null)
                throw new NullReferenceException();
            if (!property.PropertyType.IsInstanceOfType(value))
                throw new Exception("BeginFrameAnimationAsync error. Data type of 'value' parameter doesn't match property.PropertyType.");

            var animation = new ObjectAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = value, KeyTime = AnimationDuration.TimeSpan });
            return element.BeginAnimationAsync(property, animation);
        }

        public static Task BeginAnimationAsync(this IAnimatable element, DependencyProperty property, object from, object to, Duration? duration = null)
        {
            if (element == null || property == null || from == null || to == null) throw new NullReferenceException();

            if (Equals(from, to) || Equals(from, Colors.Transparent) || Equals(to, Colors.Transparent) || (element is UIElement uiElement && !uiElement.IsVisible))
            {
                ((DependencyObject)element).SetCurrentValueSmart(property, to);
                return Task.FromResult(true);
            }

            var pType = Tips.GetNotNullableType(property.PropertyType);
            if (!pType.IsInstanceOfType(from))
                throw new Exception("BeginAnimationAsync error. Data type of 'from' parameter doesn't match property.PropertyType.");
            if (!pType.IsInstanceOfType(to))
                throw new Exception("BeginAnimationAsync error. Data type of 'to' parameter doesn't match property.PropertyType.");

            var d = duration ?? AnimationDuration;
            AnimationTimeline animation = null;
            if (pType == typeof(double)) animation = new DoubleAnimation((double)from, (double)to, d);
            else if (pType == typeof(Point)) animation = new PointAnimation((Point)from, (Point)to, d);
            else if (pType == typeof(Thickness)) animation = new ThicknessAnimation((Thickness)from, (Thickness)to, d);
            else if (pType == typeof(Rect)) animation = new RectAnimation((Rect)from, (Rect)to, d);
            else if (pType == typeof(Color)) animation = new ColorAnimation((Color)from, (Color)to, d);
            if (animation == null)
                throw new NotImplementedException();
            animation.FillBehavior = FillBehavior.Stop;

            return element.BeginAnimationAsync(property, animation, to);
        }
        #endregion

        #region ================  Animation as Task  ================
        private static Task BeginAnimationAsync(this IAnimatable element, DependencyProperty property, AnimationTimeline animation, object endValue = null)
        {
            // state in constructor == animation cancel action
            // var tcs = new TaskCompletionSource<bool>(new Action(() => element.BeginAnimation(property, null)));
            var tcs = new TaskCompletionSource<bool>();
            animation.Completed += (s, e) =>
            {
                if (endValue != null)
                    ((DependencyObject)element).SetCurrentValueSmart(property, endValue);
                tcs.SetResult(true);
                tcs.Task.Dispose();
            };
            animation.Freeze(); // one time animation
            // animation.Completed += OnAnimationCompleted;
            element.BeginAnimation(property, animation);
            return tcs.Task;

            /*void OnAnimationCompleted(object sender, EventArgs e)
            {
                // animation.Completed -= OnAnimationCompleted;
                EventHelper.RemovePropertyChangeEventHandlers(animation);
                if (endValue != null)
                    ((DependencyObject)element).SetCurrentValueSmart(property, endValue);
                tcs.SetResult(true);

                // EventHelper.RemovePropertyChangeEventHandlers(animation);
            }*/
        }

        #endregion
    }
}
