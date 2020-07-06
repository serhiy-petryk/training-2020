using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using IconViewer.Themes;

namespace IconViewer
{
  /// <summary>
  /// Interaction logic for DropDownButtonTest.xaml
  /// </summary>
  public partial class ToolbarTest : Window
  {
    private Themes.ThemeInfo[] _themes = IconViewer.Themes.ThemeInfo.Themes;
    public ToolbarTest()
    {
      InitializeComponent();
      Init();
      Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      Loaded -= OnLoaded;
      Themes.Visibility = Visibility.Hidden;
      Themes.IsDropDownOpen = true;
      Themes.IsDropDownOpen = false;
      Themes.Visibility = Visibility.Visible;
      double width = 0;
      for (int i = 0; i < Themes.Items.Count; i++)
      {
        var item = (ComboBoxItem)Themes.ItemContainerGenerator.ContainerFromIndex(i);
        if (item.ActualWidth > width)
          width = item.ActualWidth;
        /*if (((ThemeInfo)Themes.Items[i]).Resource != null)
        {
          var style = ((ThemeInfo)Themes.Items[i]).Resource[typeof(ToolBar)] as Style;
          var setter = style.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == BackgroundProperty);
          if (setter != null)
            item.Background = setter.Value as Brush;
        }*/
      }

      Themes.Width = width + 20;
    }

    private void Init()
    {
      Themes.ItemsSource = new ObservableCollection<ThemeInfo>();
      foreach(var item in IconViewer.Themes.ThemeInfo.Themes)
        ((ObservableCollection< ThemeInfo > )Themes.ItemsSource).Add(item);

      Themes.ItemsSource = IconViewer.Themes.ThemeInfo.Themes;
      // ThemeDummy.ItemsSource= ThemeInfo.Themes;
      Themes.SelectedValue = IconViewer.Themes.ThemeInfo.Themes[0];
      Themes.UpdateLayout();

      // Themes.IsDropDownOpen = false;

      // Themes.ItemContainerStyleSelector = new ThemeStyleSelector();
      // StyleSelector selector = lstCars.ItemContainerStyleSelector;
      // lstCars.ItemContainerStyleSelector = null;
      // lstCars.ItemContainerStyleSelector = selector;

      /*for (int i = 0; i < Themes.Items.Count; i++)
      {
        ComboBoxItem it = (ComboBoxItem)Themes.ItemContainerGenerator.ContainerFromIndex(i); ;
        // MessageBox.Show(it.Content.ToString());
      }*/
    }

    // override onlo

    private void MyVersion_OnChecked(object sender, RoutedEventArgs e)
    {
      if (sender is ToggleButton button && Equals(button.IsChecked, true))
      {
        var items = button.Resources["items"];
        if (items is ContextMenu)
        {
          var cm = (ContextMenu)items;
          if (cm.PlacementTarget == null)
          {
            cm.PlacementTarget = button;
            cm.Placement = PlacementMode.Bottom;
            cm.Closed += (senderClosed, eClosed) => ((ToggleButton)sender).IsChecked = false;
          }
          cm.IsOpen = true;
        }
        else if (items is Popup)
        {
          var popup = (Popup)items;
          if (popup.PlacementTarget == null)
          {
            popup.PlacementTarget = button;
            popup.Placement = PlacementMode.Bottom;
            popup.Closed += (senderClosed, eClosed) => ((ToggleButton)sender).IsChecked = false;
          }
          popup.IsOpen = true;
        }
      }
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

    private void Button_Click(object sender, RoutedEventArgs e)
    {
    }

    private void Themes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var item = e.AddedItems[0] as Themes.ThemeInfo;
      // Clear old themes
      foreach (var a in IconViewer.Themes.ThemeInfo.Themes.Where(th => th.Resource != null))
        Resources.MergedDictionaries.Remove(a.Resource);
      // Add new theme
      if (item.Resource != null)
        Resources.MergedDictionaries.Add(item.Resource);
    }
  }

}
