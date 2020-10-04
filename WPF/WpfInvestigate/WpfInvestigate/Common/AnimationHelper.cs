﻿using System;
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
        public const double AnimationTime = 120.0;
        public static readonly Duration AnimationDuration = TimeSpan.FromMilliseconds(AnimationTime);
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
            CreateAnimation(element, new PropertyPath(propertyPath), propertyPath.PropertyType, duration);
        public static Timeline CreateAnimation(this FrameworkElement element, DependencyProperty propertyPath, double? duration) =>
            CreateAnimation(element, new PropertyPath(propertyPath), propertyPath.PropertyType, TimeSpan.FromMilliseconds(duration ?? AnimationTime));
        public static Timeline CreateAnimation(this FrameworkElement element, string propertyPath, Type propertyType, double? duration = null) =>
            CreateAnimation(element, new PropertyPath(propertyPath), propertyType, TimeSpan.FromMilliseconds(duration ?? AnimationTime));
        public static Timeline CreateAnimation(this FrameworkElement element, PropertyPath propertyPath, Type propertyType, Duration? duration = null)
        {
            AnimationTimeline animation = null;
            if (propertyType == typeof(double)) animation = new DoubleAnimation();
            else if (propertyType == typeof(Point)) animation = new PointAnimation();
            else if (propertyType == typeof(Thickness)) animation = new ThicknessAnimation();
            else if (propertyType == typeof(Color)) animation = new ColorAnimation();

            if (animation == null)
                throw new NotImplementedException();

            animation.Duration = duration ?? AnimationDuration;
            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, propertyPath);
            return animation;
        }

        public static ObjectAnimationUsingKeyFrames CreateFrameAnimation(FrameworkElement element, DependencyProperty dataProperty, object value)
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
        #endregion
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
