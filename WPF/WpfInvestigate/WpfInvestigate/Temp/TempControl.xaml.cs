using System.Windows;
using WpfInvestigate.Controls;
using WpfInvestigate.ViewModels;

namespace WpfInvestigate.Temp
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
