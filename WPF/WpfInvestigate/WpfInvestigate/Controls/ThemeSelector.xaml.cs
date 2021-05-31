using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WpfInvestigate.Common;
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

            var dpd = DependencyPropertyDescriptor.FromProperty(ColorControl.ColorProperty, typeof(ColorControl));
            dpd.AddValueChanged(ColorControl, (sender, args) => Color = ((ColorControl) sender).Color);

            ThemeList.Children.Clear();
            foreach (var theme in MwiThemeInfo.Themes)
            {
                var btn = new RadioButton
                {
                    GroupName = "Theme",
                    Content = theme.Value,
                    IsChecked = theme.Value == MwiAppViewModel.Instance.CurrentTheme,
                    Margin = new Thickness(2, 1, 2, 1),
                    IsThreeState = false
                };
                btn.Checked += OnRadioButtonChecked;
                btn.SetBinding(ForegroundProperty, new Binding("Background") {ElementName = "Root", Converter = ColorHslBrush.Instance, ConverterParameter = "+100%"});

                ThemeList.Children.Add(btn);
            }

            OnRadioButtonChecked(null, null);
        }

        private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            foreach (RadioButton btn in ThemeList.Children)
            {
                if (Equals(btn.IsChecked, true))
                {
                    Theme = (MwiThemeInfo)btn.Content;
                    ColorControl.IsEnabled = Theme.FixedColor == null;
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
                    var newTheme = (MwiThemeInfo)btn.Content;
                    if (newTheme != MwiAppViewModel.Instance.CurrentTheme)
                        MwiAppViewModel.Instance.ChangeTheme(newTheme);
                    break;
                }
            }
        }

        #region ==============  Dependency Properties  ===============
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color",
            typeof(Color), typeof(ThemeSelector), new FrameworkPropertyMetadata(Colors.White));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
        //====================
        public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register("Theme",
            typeof(MwiThemeInfo), typeof(ThemeSelector), new FrameworkPropertyMetadata(null, OnThemeChanged));

        public MwiThemeInfo Theme
        {
            get => (MwiThemeInfo)GetValue(ThemeProperty);
            set => SetValue(ThemeProperty, value);
        }
        private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ThemeSelector selector && e.NewValue is MwiThemeInfo theme)
                selector.Color = theme.FixedColor ?? selector.ColorControl.Color;
        }

        #endregion

        // ================  TEMP  ================
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button) sender;
            var a1 = btn.GetVisualParents().OfType<MwiChild>().ToArray();
        }
    }
}
