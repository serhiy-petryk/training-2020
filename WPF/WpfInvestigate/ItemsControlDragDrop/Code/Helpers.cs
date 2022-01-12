using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ItemsControlDragDrop.Code
{
    public static class Helpers
    {
        public delegate Point GetPositionDelegate(IInputElement element);
        public static bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(target);
            var mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }

        // ===================================
        public static List<DependencyObject> GetElementsUnderMouseClick(UIElement sender, MouseButtonEventArgs e)
        {
            var hitTestResults = new List<DependencyObject>();
            VisualTreeHelper.HitTest(sender, null, result => GetHitTestResult(result, hitTestResults), new PointHitTestParameters(e.GetPosition(sender)));
            return hitTestResults;
        }
        private static HitTestResultBehavior GetHitTestResult(HitTestResult result, List<DependencyObject> hitTestResults)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            hitTestResults.Add(result.VisualHit);
            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject current)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);
                yield return child;

                foreach (var childOfChild in GetVisualChildren(child))
                    yield return childOfChild;
            }
        }

        public static IEnumerable<DependencyObject> GetVisualParents(this DependencyObject current)
        {
            while (current != null)
            {
                yield return current;
                current = VisualTreeHelper.GetParent(current) ?? (current as FrameworkElement)?.Parent;
            }
        }


    }
}
