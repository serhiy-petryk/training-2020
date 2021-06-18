using System.Windows;
using System.Windows.Input;
using WpfSpLib.Common;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for ButtonStyleTests.xaml
    /// </summary>
    public partial class ButtonStyleTests : Window
    {
        public ButtonStyleTests()
        {
            InitializeComponent();
        }

        private void ChangeContent_OnClick(object sender, RoutedEventArgs e)
        {
            TB1.Content = TB1.Content.ToString() + "X";
        }

        private void ChangeBorder_OnClick(object sender, RoutedEventArgs e)
        {
            if (Tips.AreEqual(TestTB.BorderThickness.Right, 2))
                TestTB.BorderThickness = new Thickness(4.0);
            else
                TestTB.BorderThickness = new Thickness(2.0);
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Click");
        }
    }
}
