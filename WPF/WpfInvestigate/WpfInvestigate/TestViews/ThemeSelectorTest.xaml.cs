using System.Diagnostics;
using System.Windows;
using WpfInvestigate.Controls;
using WpfInvestigate.Samples;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ThemeSelectorTest.xaml
    /// </summary>
    public partial class ThemeSelectorTest : Window
    {
        public ThemeSelectorTest()
        {
            InitializeComponent();
        }

        private void OnOpenColorControlClick(object sender, RoutedEventArgs e)
        {
            var a1 = new DialogAdorner(Host) { CloseOnClickBackground = false };

            var content = new MwiChild
            {
                Content = new ThemeSelector{Margin = new Thickness(0)},
                Width = 700,
                Height = 600,
                LimitPositionToPanelBounds = true,
                Title = "Theme Selector"
            };
            a1.ShowContentDialog(content);
        }
    }
}
