using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfLib.Obsolete
{
    public static class ToggleButtonHelper
    {
        public static void OpenMenu_OnCheck(object sender)
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
