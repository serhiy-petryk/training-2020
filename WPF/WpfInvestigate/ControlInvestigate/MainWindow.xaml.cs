using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ControlInvestigate.Common;

namespace ControlInvestigate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TempControl_OnClick(object sender, RoutedEventArgs e)
        {
            // new TempControl().Show();
            var a1 = TestTextBox;
            var aa1 = Tips.GetVisualChildren(a1).ToList();
            var a2 = aa1.OfType<ScrollViewer>().First();
            var aa2 = Tips.GetVisualParents(a2).ToArray();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var a1 = sender as TextBox;
            var aa1 = Tips.GetVisualChildren(a1).ToList();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var a1 = sender as TextBox;
            var aa1 = Tips.GetVisualChildren(a1).ToList();
            var a2 = aa1.OfType<ScrollViewer>().First();
            var aa2 = Tips.GetVisualParents(a2).ToArray();
        }
    }
}
