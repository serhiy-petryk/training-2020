using System.Windows;
using System.Windows.Media;

namespace WpfSpLibDemo.TestViews
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

        private void ChangeColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (Brush.Color == Colors.Blue)
                Brush.Color = Colors.Yellow;
            else
                Brush.Color = Colors.Blue;
        }
    }
}
