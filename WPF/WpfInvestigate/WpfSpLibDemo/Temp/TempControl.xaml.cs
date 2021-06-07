using System.Windows;
using WpfSpLib.Controls;
using WpfSpLib.ViewModels;

namespace WpfSpLibDemo.Temp
{
    /// <summary>
    /// Interaction logic for TempControl.xaml
    /// </summary>
    public partial class TempControl : Window
    {
        public TempControl()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogMessage.Show(
                "Message text Message text Message text Message text Message text Message text",
                "Caption of Message block", DialogMessage.DialogMessageIcon.Success, new[] { "OK", "Cancel" }, true,
                MwiAppViewModel.Instance.DialogHost);
        }
    }
}
