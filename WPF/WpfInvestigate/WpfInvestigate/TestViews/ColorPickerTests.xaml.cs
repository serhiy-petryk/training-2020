using System.Windows;
using System.Windows.Media;
using WpfInvestigate.ViewModels;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ColorControlTests.xaml
    /// </summary>
    public partial class ColorControlTests : Window
    {
        public ColorControlTests()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ColorControl.SaveColor();
            MwiAppViewModel.Instance.AppColor = ColorControl.Color;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e) => ColorControl.RestoreColor();

        private void ChangeColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (Brush.Color == Colors.Blue)
                Brush.Color = Colors.Yellow;
            else
                Brush.Color = Colors.Blue;
        }
    }
}
