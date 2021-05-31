using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfInvestigate.Themes;
using WpfInvestigate.ViewModels;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for ThemeSelector.xaml
    /// </summary>
    public partial class ThemeSelector
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
                var btn = new RadioButton
                {
                    GroupName = "Theme", Content = theme.Value,
                    IsChecked = theme.Value == MwiAppViewModel.Instance.CurrentTheme,
                    Margin = new Thickness(2, 1, 2, 1), IsThreeState = false
                };
                btn.Checked += OnRadioButtonChecked;
                ThemeList.Children.Add(btn);
            }

            OnRadioButtonChecked(null, null);

            ColorControl.Color = MwiAppViewModel.Instance.AppColor;
        }

        private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            foreach (RadioButton btn in ThemeList.Children)
            {
                if (Equals(btn.IsChecked, true))
                {
                    var theme = (MwiThemeInfo)btn.Content;
                    ColorControl.IsEnabled = theme.FixedColor == null;
                    break;
                }
            }
        }

        private void OnApplyButtonClick(object sender, RoutedEventArgs e)
        {
            if (MwiAppViewModel.Instance.AppColor != ColorControl.Color)
                MwiAppViewModel.Instance.AppColor = ColorControl.Color;

            foreach (RadioButton btn in ThemeList.Children)
            {
                if (Equals(btn.IsChecked, true))
                {
                    var newTheme = (MwiThemeInfo) btn.Content;
                    if (newTheme != MwiAppViewModel.Instance.CurrentTheme)
                        MwiAppViewModel.Instance.ChangeTheme(newTheme);
                    break;
                }
            }
        }
    }
}
