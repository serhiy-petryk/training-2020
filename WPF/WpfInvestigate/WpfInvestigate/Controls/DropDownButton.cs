using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public static class DropDownButton
    {
        public static void OpenDropDownMenu(object sender)
        {
            if (sender is ToggleButton button && Equals(button.IsChecked, true))
            {
                foreach (var element in Tips.GetVisualParents(button).OfType<FrameworkElement>())
                {
                    var cm = element.Tag as ContextMenu ?? element.Resources.Values.OfType<ContextMenu>().FirstOrDefault();
                    if (cm != null)
                    {
                        if (cm.PlacementTarget == null)
                        {
                            cm.PlacementTarget = element;
                            cm.Placement = PlacementMode.Bottom;
                            cm.Closed += (senderClosed, eClosed) => ((ToggleButton)sender).IsChecked = false;
                        }
                        cm.IsOpen = true;
                        return;
                    }
                }
                throw new Exception($"Can't find context menu in DropDown button");
            }
        }

        /* Examples for 2 types of DropDownButton (Popup and ContextMenu):
         
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
