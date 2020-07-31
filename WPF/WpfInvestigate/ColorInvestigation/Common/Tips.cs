using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorInvestigation.Common
{
    public static class Tips
    {
        public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
        private const double DefaultPrecision = 0.0001;

        public static bool AreEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < DefaultPrecision;
        }

        public static Brush GetActualBackgroundBrush(DependencyObject d)
        {
            // valid only for SolidColorBrush
            foreach (var c in GetVisualParents(d).Where(a1 => a1 is Control || a1 is Panel))
            {
                var brush = c is Control ? ((Control)c).Background : ((Panel)c).Background;
                if (brush is SolidColorBrush)
                {
                    var color = ((SolidColorBrush)brush).Color;
                    if (color != Colors.Transparent)
                        return brush;
                }
                else if (brush != null)
                    return brush;
            }
            return null;
        }
        public static Brush GetActualForegroundBrush(DependencyObject d)
        {
            // valid only for SolidColorBrush
            foreach (var o in GetVisualParents(d).Where(a1 => a1 is Control))
            {
                var c = (Control)o;
                var brush = c.Foreground;
                if (brush is SolidColorBrush)
                {
                    if (((SolidColorBrush)brush).Color != Colors.Transparent)
                        return brush;
                }
                else if (brush != null)
                    return brush;
            }
            return null;
        }

        public static Color GetActualBackgroundColor(DependencyObject d)
        {
            var color = GetColorFromBrush(GetActualBackgroundBrush(d));
            return color;
        }
        public static Color GetActualForegroundColor(DependencyObject d)
        {
            var color = GetColorFromBrush(GetActualForegroundBrush(d));
            return color;
        }
        public static Color GetColorFromBrush(Brush brush)
        {
            if (brush is SolidColorBrush)
                return ((SolidColorBrush)brush).Color;
            if (brush is LinearGradientBrush)
            {
                var gcs = ((LinearGradientBrush)brush).GradientStops;
                var color = gcs[gcs.Count / 2].Color;
                return color;
            }
            return Colors.Transparent;
        }

        public static IEnumerable<DependencyObject> GetVisualParents(DependencyObject current)
        {
            while (current != null)
            {
                yield return current;
                current = VisualTreeHelper.GetParent(current) ?? (current as FrameworkElement)?.Parent;
            }
        }
    }
}
