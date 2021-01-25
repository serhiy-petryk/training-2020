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
            else if (pType == typeof(Color)) animation = new ColorAnimation((Color)from, (Color)to, d);
            if (animation == null)
                throw new NotImplementedException();

            animation.FillBehavior = FillBehavior.Stop;
            ((DependencyObject)element).SetValue(property, to);
            return element.BeginAnimationAsync(property, animation);
        }
        #endregion

        #region ================  Animation as Task  ================
        private static Task BeginAnimationAsync(this IAnimatable animatable, DependencyProperty animationProperty, AnimationTimeline animation)
        {
            // state in constructor == animation cancel action
            var tcs = new TaskCompletionSource<bool>(new Action(() => animatable.BeginAnimation(animationProperty, null)));
            animation.Completed += (s, e) => tcs.SetResult(true);
            animation.Freeze(); // one time animation
            animatable.BeginAnimation(animationProperty, animation);
            return tcs.Task;
        }
        #endregion
    }
    #endregion 
}
