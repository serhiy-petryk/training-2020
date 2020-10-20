using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfInvestigate.Obsolete.TestViews
{
    /// <summary>
    /// Interaction logic for NumericUpDownTests.xaml
    /// </summary>
    public partial class NumericUpDownTests
    {
        public NumericUpDownTests()
        {
            InitializeComponent();
        }

        private void ChangeNullable_OnClick(object sender, RoutedEventArgs e)
        {
            test2.IsNullable = !test2.IsNullable;
            test2.DecimalPlaces = 2;
            test2.StringFormat = "N2";
        }

        private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            var a1 = tb.SelectionStart;
            var a12 = tb.SelectionLength;
            var a2 = tb.CaretIndex;
        }
    }
}
