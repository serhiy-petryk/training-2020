using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using WpfSpLib.Common;
using WpfSpLib.Helpers;

namespace WpfSpLib
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // var vCulture = new CultureInfo("uk");
            var vCulture = Tips.InvariantCulture;

            Thread.CurrentThread.CurrentCulture = vCulture;
            Thread.CurrentThread.CurrentUICulture = vCulture;
            CultureInfo.DefaultThreadCurrentCulture = vCulture;
            CultureInfo.DefaultThreadCurrentUICulture = vCulture;

            var a1 = Thread.CurrentThread.CurrentCulture;
            var a2 = Thread.CurrentThread.CurrentUICulture;
            var a3 = CultureInfo.DefaultThreadCurrentCulture;
            var a4 = CultureInfo.DefaultThreadCurrentUICulture;

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));

            // global event handlers 
            EventManager.RegisterClassHandler(typeof(ToolTip), ToolTip.OpenedEvent, new RoutedEventHandler(OnToolTipOpened));
            EventManager.RegisterClassHandler(typeof(ContextMenu), ContextMenu.OpenedEvent, new RoutedEventHandler(OnContextMenuOpened));
            EventManager.RegisterClassHandler(typeof(ToggleButton), ToggleButton.CheckedEvent, new RoutedEventHandler(OnToggleButtonChecked));

            SelectAllOnFocusForTextBox.ActivateGlobally();

            base.OnStartup(e);
        }

        private void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            if (!(sender is ContextMenu contextMenu)) return;
            if (!(contextMenu.PlacementTarget is FrameworkElement owner)) return;
            contextMenu.ApplyTransform(owner);
        }
        private void OnToolTipOpened(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToolTip toolTip)) return;
            if (!(toolTip.PlacementTarget is FrameworkElement owner)) return;
            toolTip.ApplyTransform(owner);
        }

        private void OnToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button && Equals(button.IsChecked, true))
            {
                var cm = button.Tag as ContextMenu ?? button.Resources.Values.OfType<ContextMenu>().FirstOrDefault();
                if (cm != null && !cm.IsOpen) // ContextMenu may be already opened (?bug (binding mode=TwoWay=>twice event call when value changed), see SplitButtonStyle)
                {
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
}
