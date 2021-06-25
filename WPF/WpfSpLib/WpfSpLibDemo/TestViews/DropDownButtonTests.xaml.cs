using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfSpLib.Common;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for ToggleButtonAndPopup.xaml
    /// </summary>
    public partial class DropDownButtonTests : Window
    {
        private int _clickCount = 0;
        public DropDownButtonTests()
        {
            InitializeComponent();
        }

        private void SplitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (((ToggleButton) sender).IsChecked != true)
                SplitButtonLabel.Text = $"SplitButtonStyle (Click count={++_clickCount}):";
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            // var a1 = T1;
            // var a2 = Tips.GetVisualChildren(T1).ToArray();
            var a12 = T2;
            var a22 = Tips.GetVisualChildren(T2).ToArray();
        }

        private void IsVisible_OnChecked(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox) sender;
            T2.Visibility = cb.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
