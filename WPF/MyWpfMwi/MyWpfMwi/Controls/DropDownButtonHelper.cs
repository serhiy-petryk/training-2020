using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MyWpfMwi.Controls
{
    public static class DropDownButtonHelper
    {
        public static void OpenDropDownMenu(object sender)
        {
            if (sender is ToggleButton button && Equals(button.IsChecked, true))
            {
                var cm = button.Resources.Values.OfType<ContextMenu>().First();
                if (cm.PlacementTarget == null)
                {
                    cm.PlacementTarget = button;
                    cm.Placement = PlacementMode.Bottom;
                    cm.Closed += (senderClosed, eClosed) => ((ToggleButton)sender).IsChecked = false;
                }
                cm.IsOpen = true;
            }

        }
    }
}
