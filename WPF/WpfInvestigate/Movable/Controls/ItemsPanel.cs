using System.Windows;
using System.Windows.Controls;

namespace Movable.Controls
{
    public class ItemsPanel: Canvas
    {
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            // Track when objects are added and removed
            if (visualAdded != null)
            {
                // Do stuff with the added object
            }
            if (visualRemoved != null)
            {
                // Do stuff with the removed object
            }

            // Call base function
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }
    }
}
