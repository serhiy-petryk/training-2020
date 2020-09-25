using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorInvestigation.Common
{
    public static class Tips
    {
        public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
        private const double DefaultPrecision = 0.0001;

        public static void Beep() => SystemSounds.Beep.Play();

        public static bool AreEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < DefaultPrecision;
        }

        public static Geometry GeometryFromString(object o)
        {
            if (o is string)
            {
                const string pathString = "FMLHVCQSTAZ+-,.0123456789fmlhvcqstaz";
                var s = ((string)o).Trim();
                if (!string.IsNullOrEmpty(s) && s.Length > 2 && "FfMm".Any(s[0].ToString().Contains) &&
                    "Zz".Any(s[s.Length - 1].ToString().Contains) && (s.Contains(',') || s.Contains(' ')) &&
                    s.All(c => pathString.Contains(c) || char.IsWhiteSpace(c)))
                    try { return Geometry.Parse(s); }
                    catch { } // ignored
            }
            return null;
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

        public static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject current)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);
                yield return child;

                foreach (var childOfChild in GetVisualChildren(child))
                    yield return childOfChild;
            }
        }

        // ============  Type  ==============
        public static bool IsNullableType(Type type) => type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        public static Type GetNotNullableType(Type type)
        {
            if (IsNullableType(type))
                return Nullable.GetUnderlyingType(type);
            return type;
        }

    }
}
