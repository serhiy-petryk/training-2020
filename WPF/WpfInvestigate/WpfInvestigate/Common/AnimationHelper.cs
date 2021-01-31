﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfInvestigate.Common
{
    #region ======  AnimationHelper  =====
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
                var ca = new ColorAnimation(oldBrush.GradientStops[k].Color, newBrush.GradientStops[k].Color, AnimationDuration);
                newBrush.GradientStops[k].BeginAnimation(GradientStop.ColorProperty, ca);
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
            return element.BeginAnimationAsync(property, animation, value);
        }

        public static Task BeginAnimationAsync(this IAnimatable element, DependencyProperty property, object from, object to, Duration? duration = null)
        {
            if (element == null || property == null || from == null || to == null)
                throw new NullReferenceException();
            var pType = property.PropertyType;
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
        private static Task BeginAnimationAsync(this IAnimatable element, DependencyProperty property, AnimationTimeline animation, object to)
        {
            // state in constructor == animation cancel action
            // var tcs = new TaskCompletionSource<bool>(new Action(() => element.BeginAnimation(property, null)));
            var tcs = new TaskCompletionSource<bool>();
            animation.Completed += (s, e) =>
            {
                ((DependencyObject) element).SetValue(property, to);
                tcs.SetResult(true);
            };
            animation.Freeze(); // one time animation
            element.BeginAnimation(property, animation);
            return tcs.Task;
        }
        #endregion
    }
    #endregion 
}
