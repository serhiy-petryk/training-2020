using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IconViewer
{
  /// <summary>
  /// Interaction logic for DropDownButtonTest.xaml
  /// </summary>
  public partial class DropDownButtonTest : Window
  {
    public DropDownButtonTest()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Open the popup
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnPrint_Click(object sender, RoutedEventArgs e)
    {
      if (popupPrint.IsOpen)
      {
        popupPrint.IsOpen = false;
        return;
      }
      popupPrint.IsOpen = true;
      popupPrint.Closed += (senderClosed, eClosed) =>
      {
        btnPrint.IsChecked = false;
      };
    }

    /// <summary>
    /// Catch the event raised when a user click on a Popup Button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void popup_Click(object sender, RoutedEventArgs e)
    {
      System.Windows.Controls.Primitives.Popup popup = sender as System.Windows.Controls.Primitives.Popup;
      if (popup != null)
        popup.IsOpen = false;
    }

    private void btnPrintX_Click(object sender, RoutedEventArgs e)
    {
      if (popupPrintX.IsOpen)
      {
        popupPrintX.IsOpen = false;
        return;
      }
      popupPrintX.IsOpen = true;
      popupPrintX.Closed += (senderClosed, eClosed) =>
      {
        btnPrintX.IsChecked = false;
      };
    }

    /// <summary>
    /// Catch the event raised when a user click on a Popup Button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void popupX_Click(object sender, RoutedEventArgs e)
    {
      System.Windows.Controls.Primitives.Popup popup = sender as System.Windows.Controls.Primitives.Popup;
      if (popup != null)
        popup.IsOpen = false;
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
      // throw new NotImplementedException();
    }

    private void AddPresetButton_OnClick(object sender, RoutedEventArgs e)
    {
      var addButton = sender as FrameworkElement;
      var cm = this.Resources["AAA"] as ContextMenu;
      cm.PlacementTarget = addButton;
      cm.Placement = PlacementMode.Bottom;
      cm.IsOpen = true;


      /*if (addButton != null)
      {
        addButton.ContextMenu.IsOpen = true;
      }*/
    }

    private void aa1_OnClick(object sender, RoutedEventArgs e)
    {
      var button = sender as FrameworkElement;
      if (button.IsHitTestVisible)
      {

      }
      else
      {

      }
      button.ContextMenu.PlacementTarget = button;
      button.ContextMenu.Placement = PlacementMode.Bottom;
      button.ContextMenu.IsOpen = true;
    }

    private void BtnSvgSelectLayout_OnClick(object sender, RoutedEventArgs e)
    {
      var button = sender as FrameworkElement;
      button.ContextMenu.PlacementTarget = button;
      button.ContextMenu.Placement = PlacementMode.Bottom;
      button.ContextMenu.IsOpen = true;
    }

    private void BtnSelectLayout_OnClick(object sender, RoutedEventArgs e)
    {
      var cm = this.Resources["AAA"] as ContextMenu;
      var button = sender as FrameworkElement;
      cm.PlacementTarget = button;
      cm.Placement = PlacementMode.Bottom;
      cm.IsOpen = true;
      // cm.Placement = PlacementMode.Bottom;
      // button.ContextMenu.PlacementTarget = button;
      // button.ContextMenu.Placement = PlacementMode.Bottom;
      // button.ContextMenu.IsOpen = true;
      // throw new NotImplementedException();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      // throw new NotImplementedException();
    }

    private void Aa1_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      // throw new NotImplementedException();
    }

    private void Aa1_OnChecked(object sender, RoutedEventArgs e)
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
