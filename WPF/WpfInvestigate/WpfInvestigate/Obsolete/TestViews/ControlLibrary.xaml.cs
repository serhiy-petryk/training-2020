using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfInvestigate.Common;

namespace WpfInvestigate.Obsolete.TestViews
{
    /// <summary>
    /// Interaction logic for ControlLibrary.xaml
    /// </summary>
    public partial class ObsoleteControlLibrary
    {
        public ObsoleteControlLibrary()
        {
            InitializeComponent();
        }

        private void OpenSettingButton_OnChecked(object sender, RoutedEventArgs e) => ToggleButtonHelper.OpenMenu_OnCheck(sender);

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("ButtonBase_OnClick");
        }
        private void Dropdown_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ChangeNullable_OnClick(object sender, RoutedEventArgs e)
        {
            test2.IsNullable = !test2.IsNullable;
            test2.DecimalPlaces = 2;
            test2.StringFormat = "N2";
        }

        private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox) sender;
            var a1 = tb.SelectionStart;
            var a12 = tb.SelectionLength;
            var a2 = tb.CaretIndex;
        }

        private void XButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var x = NNN;
            var a1 = x.ActualHeight;
            var a2 = x.ActualWidth;
            var aa = Tips.GetVisualChildren(x).ToArray();
            // throw new System.NotImplementedException();
        }

        private void RemoveTextBox_OnClick(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(VVV) as StackPanel;
            parent?.Children.Remove(VVV);
        }
    }
}
