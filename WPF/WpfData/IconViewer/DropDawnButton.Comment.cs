using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconViewer
{
  class DropDawnButton_Comment
  {
    /* Simplest
     *       <Button Name="SvgBtnSelectLayout" Background="Transparent" BorderBrush="Transparent" Click="BtnSelectLayout_OnClick" ContextMenuService.Placement="Bottom">
        <ContentControl>
          <StackPanel Orientation="Horizontal">
            <svg1:SVGImage Source="/DG;component/Resources/folder.svg" Width="24" Height="24" OverrideColor="#FF3399FF"/>
            <Path x:Name="xxxArrow" Width="9" Height="5" Data="{StaticResource DownArrowGeometry}" Fill="#FF3399FF" Margin="2,1,0,0" />
          </StackPanel>
        </ContentControl>
        <Button.ContextMenu>
          <ContextMenu>
            <MenuItem Header="Копировать"></MenuItem>
            <MenuItem Header="Вставить"></MenuItem>
            <MenuItem Header="Вырезать"></MenuItem>
            <MenuItem Header="Удалить"></MenuItem>
          </ContextMenu>
        </Button.ContextMenu>
      </Button>

     */


    /* From 
     *       <!-- Print Toggle Button -->
      <ToggleButton Name="btnPrint" ToolTip="Print your documents">
        <StackPanel Orientation="Horizontal">
          <Image Source="Resources\Print.png" />
          <TextBlock Margin="2" VerticalAlignment="Center">Print</TextBlock>
        </StackPanel>
        <ToggleButton.Style>
          <Style TargetType="{x:Type ToggleButton}">
            <Setter Property="IsHitTestVisible" Value="True"/>
            <Style.Triggers>
              <DataTrigger Binding="{Binding ElementName=popupPrint, Path=IsOpen}" Value="True">
                <Setter Property="IsHitTestVisible" Value="False"/>
              </DataTrigger>
            </Style.Triggers>
          </Style>
        </ToggleButton.Style>
      </ToggleButton>

      <!-- Print Toggle Button Popup -->
      <Popup x:Name="popupPrint" StaysOpen="False" PlacementTarget="{Binding ElementName=btnPrint}"
             IsOpen="{Binding IsChecked, ElementName=btnPrint, Mode=TwoWay}"  PopupAnimation="Slide">
        <Border>
          <StackPanel>
            <!-- Print to local -->
            <Button ToolTip="Print your documents to a local printer">
              <StackPanel Orientation="Horizontal">
                <Image Source="Print.png" />
                <TextBlock Margin="0" VerticalAlignment="Center">Print to local</TextBlock>
              </StackPanel>
            </Button>
            <!-- Print to network -->
            <Button ToolTip="Print your documents to a network printer">
              <StackPanel Orientation="Horizontal">
                <Image Source="Print.png" />
                <TextBlock Margin="0" VerticalAlignment="Center">Print to network</TextBlock>
              </StackPanel>
            </Button>
            <Button ToolTip="Print your documents to a network printer">
              <StackPanel Orientation="Horizontal">
                <TextBlock Margin="2" VerticalAlignment="Center">XX Print to network</TextBlock>
              </StackPanel>
            </Button>
          </StackPanel>
        </Border>
      </Popup>


     */
  }
}
