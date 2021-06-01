using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Themes;
using WpfInvestigate.ViewModels;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for ThemeSelector.xaml
    /// </summary>
    public partial class ThemeSelector: INotifyPropertyChanged
    {
        public bool IsSaved { get; private set; }
        public ThemeSelector()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var dpd = DependencyPropertyDescriptor.FromProperty(ColorControl.ColorProperty, typeof(ColorControl));
            dpd.AddValueChanged(ColorControl, (sender, args) => OnPropertiesChanged(nameof(Color)));

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
            IsSaved = true;
            ApplicationCommands.Close.Execute(null, this);

            /*if (MwiAppViewModel.Instance.AppColor != ColorControl.Color)
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
            }*/
        }

        #region ==============  Dependency Properties  ===============
        public Color Color => (Theme?.FixedColor ?? ColorControl?.Color) ?? Colors.White;

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
                selector.OnPropertiesChanged(nameof(Color));
        }

        #endregion

        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
