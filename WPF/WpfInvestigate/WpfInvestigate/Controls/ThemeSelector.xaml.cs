using System.Windows;
using System.Windows.Controls;
using WpfInvestigate.Themes;
using WpfInvestigate.ViewModels;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for ThemeSelector.xaml
    /// </summary>
    public partial class ThemeSelector : UserControl
    {
        public ThemeSelector()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ThemeList.Children.Clear();
            foreach (var theme in MwiThemeInfo.Themes)
            {
                var btn = new RadioButton{GroupName = "Theme", Content = theme.Value, IsChecked = theme.Value == MwiAppViewModel.Instance.CurrentTheme, Margin = new Thickness(2)};
                ThemeList.Children.Add(btn);
            }
        }
    }
}
