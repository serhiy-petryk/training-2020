using System.Windows;
using ColorInvestigation.Common;

namespace ColorInvestigation.Views
{
    /// <summary>
    /// Interaction logic for ColorPickerTests.xaml
    /// </summary>
    public partial class ColorPickerTests : Window
    {
        public ColorPickerTests()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) => ColorPicker.SaveColor();
        private void SaveButtonVM_Click(object sender, RoutedEventArgs e) => ColorPickerViewModel.SaveColor();
        private void SaveButtonAsync_Click(object sender, RoutedEventArgs e) => ColorPickerAsync.SaveColor();
        private void SaveButtonDelay_Click(object sender, RoutedEventArgs e) => ColorPickerDelay.SaveColor();
        private void SaveButtonLabel_Click(object sender, RoutedEventArgs e) => ColorPickerLabel.SaveColor();

        private void RestoreButton_Click(object sender, RoutedEventArgs e) => ColorPicker.RestoreColor();
        private void RestoreButtonVM_Click(object sender, RoutedEventArgs e) => ColorPickerViewModel.RestoreColor();
        private void RestoreButtonAsync_Click(object sender, RoutedEventArgs e) => ColorPickerAsync.RestoreColor();
        private void RestoreButtonDelay_Click(object sender, RoutedEventArgs e) => ColorPickerDelay.RestoreColor();
        private void RestoreButtonLabel_Click(object sender, RoutedEventArgs e) => ColorPickerLabel.RestoreColor();

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            ColorSpacesCheck.RunTests();
        }

    }
}
