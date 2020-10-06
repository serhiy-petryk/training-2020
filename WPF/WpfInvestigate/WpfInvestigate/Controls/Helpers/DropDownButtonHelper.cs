using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfInvestigate.Controls.Helpers
{
    public static class DropDownButtonHelper
    {
        public static ContextMenu OpenDropDownMenu(object sender)
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
                        cm.Closed += (senderClosed, eClosed) => ((ToggleButton) sender).IsChecked = false;
                    }
                    cm.IsOpen = true;
                    return cm;
                }
            }
            return null;
        }

        /* Examples of 2 types of DropDownButton (Popup and ContextMenu):
         
         XAML file:
         =========
                      <ToggleButton Grid.Row="6" x:Name="PopupButton" Margin="5" Content="DropDown button with Popup" IsThreeState="False" Width="300"
                          Style="{StaticResource FlatButtonStyle}" Background="Gainsboro"
                          IsHitTestVisible="{Binding ElementName=PART_Popup, Path=IsOpen, Converter={x:Static common:InverseBoolConverter.Instance}}"/>

            <Popup x:Name="PART_Popup"
                   AllowsTransparency="True"
                   Focusable="False"
                   Placement="Bottom"
                   PlacementTarget="{Binding ElementName=PopupButton}"
                   PopupAnimation="Fade"
                   IsOpen="{Binding ElementName=PopupButton, Path=IsChecked}"
                   StaysOpen="False">
                <controls:Calculator/>
            </Popup>

            <ToggleButton Grid.Row="8" Margin="5" Content="DropDown button with ContextMenu in Resources" IsThreeState="False" IsChecked="False" Width="300"
                          Checked="OpenDropDownMenu"
                          Style="{StaticResource FlatButtonStyle}" Background="Gainsboro">
                <ToggleButton.Resources>
                    <ContextMenu x:Key="cm" StaysOpen="False">
                        <MenuItem Header="Копировать"/>
                        <MenuItem Header="Вставить"/>
                        <MenuItem Header="Вырезать"/>
                        <MenuItem Header="Удалить"/>
                    </ContextMenu>
                </ToggleButton.Resources>
            </ToggleButton>

            <ToggleButton Grid.Row="9" Margin="5" Content="DropDown button with ContextMenu in tag" IsThreeState="False" IsChecked="False" Width="300"
                          Checked="OpenDropDownMenu"
                          Style="{StaticResource FlatButtonStyle}" Background="Gainsboro">
                <ToggleButton.Tag>
                    <ContextMenu StaysOpen="False">
                        <MenuItem Header="Copy"/>
                        <MenuItem Header="Paste"/>
                        <MenuItem Header="Cut"/>
                        <MenuItem Header="Delete"/>
                    </ContextMenu>
                </ToggleButton.Tag>
            </ToggleButton>

        CS file:
        =======
                private void OpenDropDownMenu(object sender, RoutedEventArgs e) => DropDownButton.OpenDropDownMenu(sender);

         */
    }
}
