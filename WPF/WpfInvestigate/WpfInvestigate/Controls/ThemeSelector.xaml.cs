using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
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

            ThemeList.ItemsSource = MwiThemeInfo.Themes.Values;
            ThemeList.SelectedItem = MwiAppViewModel.Instance.CurrentTheme;

            ColorControl.Color = MwiAppViewModel.Instance.AppColor;
        }

        private void ThemeList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorControl.IsEnabled = ((MwiThemeInfo)ThemeList.SelectedItem).FixedColor == null;
        }
    }
}
