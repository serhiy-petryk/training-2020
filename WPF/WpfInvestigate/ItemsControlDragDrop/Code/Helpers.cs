using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ItemsControlDragDrop.Code
{
    public static class Helpers
    {
        public static Rect GetVisibleRect(FrameworkElement item, Visual visualAncestor)
        {
            // based on https://social.msdn.microsoft.com/Forums/vstudio/en-US/b57531cc-fdb1-4d0e-9650-324630343d62/screen-rectangle-for-visible-part-of-wpf-uielement-windows-forms-wpf-interop?forum=wpf
            // Visual _rootVisual = HwndSource.FromVisual(item).RootVisual;
            var transformToRoot = item.TransformToAncestor(visualAncestor);
            // Rect screenRect = new Rect(transformToRoot.Transform(new Point(0, 0)), transformToRoot.Transform(new Point(item.ActualWidth, item.ActualHeight)));
            var screenRect = new Rect(transformToRoot.Transform(new Point(-item.Margin.Left, -item.Margin.Top)),
                transformToRoot.Transform(new Point(item.ActualWidth + item.Margin.Right, item.ActualHeight + item.Margin.Bottom)));
            var parent = VisualTreeHelper.GetParent(item);
            while (parent != null && parent != visualAncestor)
            {
                var visual = parent as Visual;
                var element = parent as FrameworkElement;
                if (visual != null && element != null)
                {
                    transformToRoot = visual.TransformToAncestor(visualAncestor);
                    var pointAncestorTopLeft = transformToRoot.Transform(new Point(0, 0));
                    var pointAncestorBottomRight = transformToRoot.Transform(new Point(element.ActualWidth, element.ActualHeight));
                    var ancestorRect = new Rect(pointAncestorTopLeft, pointAncestorBottomRight);
                    screenRect.Intersect(ancestorRect);
                }
                parent = VisualTreeHelper.GetParent(parent);
            }

            // at this point screenRect is the bounding rectangle for the visible portion of "this" element
            return screenRect;
        }

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
