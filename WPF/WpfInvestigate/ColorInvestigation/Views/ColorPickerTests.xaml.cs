using System.Windows;

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker.SaveColor();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker.RestoreColor();
        }

    }
}
