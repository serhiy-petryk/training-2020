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
  /// Interaction logic for SplitButton.xaml
  /// </summary>
  public partial class SplitButton : Window
  {
    public SplitButton()
    {
      InitializeComponent();
    }

    private void BtnPrint_OnClick(object sender, RoutedEventArgs e)
    {
      PopupPrint.IsOpen = true;
      PopupPrint.Closed += (senderClosed, eClosed) => BtnPrint.IsChecked = false;
    }

    private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
    {
      (sender as Popup).IsOpen = false;
      // throw new NotImplementedException();
    }
  }
}
