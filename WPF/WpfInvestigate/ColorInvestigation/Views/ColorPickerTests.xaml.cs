using System.Windows;
using System.Windows.Media;
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
        private void SaveButtonVM_Click(object sender, RoutedEventArgs e) => ColorPickerWithViewModel.SaveColor();
        private void SaveButtonVMAsync_Click(object sender, RoutedEventArgs e) => ColorPickerWithViewModelAsync.SaveColor();
        private void SaveButtonAsync_Click(object sender, RoutedEventArgs e) => ColorPickerAsync.SaveColor();
        private void SaveButtonLabel_Click(object sender, RoutedEventArgs e) => ColorPickerLabel.SaveColor();
        private void SaveButtonDT_Click(object sender, RoutedEventArgs e) => ColorPickerDT.SaveColor();

        private void RestoreButton_Click(object sender, RoutedEventArgs e) => ColorPicker.RestoreColor();
        private void RestoreButtonVM_Click(object sender, RoutedEventArgs e) => ColorPickerWithViewModel.RestoreColor();
        private void RestoreButtonVMAsync_Click(object sender, RoutedEventArgs e) => ColorPickerWithViewModelAsync.RestoreColor();
        private void RestoreButtonDT_Click(object sender, RoutedEventArgs e) => ColorPickerDT.RestoreColor();

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            ColorSpacesCheck.RunTests();
        }

        private void ChangeColorButtonVM_Click(object sender, RoutedEventArgs e)
        {
            if (BrushVM.Color == Colors.Blue)
                BrushVM.Color = Colors.Yellow;
            else
                BrushVM.Color = Colors.Blue;
        }

        private void ChangeColorButtonVMAsync_Click(object sender, RoutedEventArgs e)
        {
            if (BrushVMAsync.Color == Colors.Blue)
                BrushVMAsync.Color = Colors.Yellow;
            else
                BrushVMAsync.Color = Colors.Blue;
        }

        private void ChangeColorButtonDT_Click(object sender, RoutedEventArgs e)
        {
            if (BrushDT.Color == Colors.Blue)
                BrushDT.Color = Colors.Yellow;
            else
                BrushDT.Color = Colors.Blue;
        }

        private void ChangeColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (Brush.Color == Colors.Blue)
                Brush.Color = Colors.Yellow;
            else
                Brush.Color = Colors.Blue;
        }

    }
}
