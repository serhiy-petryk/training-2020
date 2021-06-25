using System.Windows;
using System.Windows.Media;
using WpfSpLib.Effects;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for CalculatorTests.xaml
    /// </summary>
    public partial class TextBoxTests : Window
    {
        public TextBoxTests()
        {
            InitializeComponent();
        }

        private void ChangeBackground_OnClick(object sender, RoutedEventArgs e)
        {
            if (TestTextBox.Background == Brushes.Yellow)
                TestTextBox.Background = Brushes.YellowGreen;
            else
                TestTextBox.Background = Brushes.Yellow;
        }

        private void ChangeForeground_OnClick(object sender, RoutedEventArgs e)
        {
            if (TestTextBox.Foreground == Brushes.Blue)
                TestTextBox.Foreground = Brushes.Violet;
            else
                TestTextBox.Foreground = Brushes.Blue;
        }

        private void ChangeButton_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = (int)TextBoxEffects.GetVisibleButtons(TestTextBox);
            var a2 = (a1 + 1) % 16;
            TextBoxEffects.SetVisibleButtons(TestTextBox, (TextBoxEffects.Buttons)a2);
        }
    }
}
