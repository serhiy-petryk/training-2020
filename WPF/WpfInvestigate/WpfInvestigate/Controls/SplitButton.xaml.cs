using System.Windows;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for SplitButton.xaml
    /// </summary>
    public partial class SplitButton : ResourceDictionary
    {
        public SplitButton()
        {
            InitializeComponent();
        }

        private void OpenDropDownMenu(object sender, RoutedEventArgs e) => DropDownButton.OpenDropDownMenu(sender);
    }
}
