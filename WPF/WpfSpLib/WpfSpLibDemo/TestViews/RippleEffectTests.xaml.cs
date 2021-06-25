using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for RippleEffectTests.xaml
    /// </summary>
    public partial class RippleEffectTests : Window
    {
        public RippleEffectTests()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.Print($"Click: {((ContentControl)sender).Content}");
        }
    }
}
