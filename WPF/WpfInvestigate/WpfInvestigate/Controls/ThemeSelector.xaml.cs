using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfInvestigate.Themes;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for ThemeSelector.xaml
    /// </summary>
    public partial class ThemeSelector: INotifyPropertyChanged
    {
        public MwiThemeInfo Theme { get; set; }
        public Color? ThemeColor { get; set; }
        public MwiThemeInfo DefaultTheme { get; set; }
        public Color DefaultThemeColor { get; set; }
        public bool UseDefaultThemeColor => cbUseDefaultTheme.IsChecked == true;
        public bool UseDefaultColor => cbUseDefaultColor.IsChecked == true;
        public MwiThemeInfo ActualTheme => Theme ?? DefaultTheme;
        public Color ActualThemeColor => ActualTheme?.FixedColor ?? (ThemeColor ?? DefaultThemeColor);
        public bool IsThemeSelectorEnabled => !UseDefaultThemeColor;
        public bool IsColorSelectorEnabled => ActualTheme != null && !ActualTheme.FixedColor.HasValue;
        public bool IsColorControlEnabled => IsColorSelectorEnabled && !UseDefaultColor;
        public bool IsSaved { get; private set; }

        public ThemeSelector()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var dpd = DependencyPropertyDescriptor.FromProperty(ColorControl.ColorProperty, typeof(ColorControl));
            dpd.AddValueChanged(ColorControl, (sender, args) =>
            {
                if (_isUpdating) return;
                if (IsColorControlEnabled) 
                    ThemeColor = ColorControl.Color;
                UpdateUI();
            });

            ThemeList.Children.Clear();
            foreach (var theme in MwiThemeInfo.Themes)
            {
                var btn = new RadioButton
                {
                    Content = theme.Value,
                    IsChecked = Equals(theme.Value, Theme),
                };
                btn.Checked += OnRadioButtonChecked;
                ThemeList.Children.Add(btn);
            }

            UpdateUI();
        }

        private bool _isUpdating;
        private void UpdateUI()
        {
            if (_isUpdating) return;
            _isUpdating = true;

            var checkedRadioButton = ThemeList.Children.OfType<RadioButton>().FirstOrDefault(a => a.Content == ActualTheme);
            if (checkedRadioButton != null)
                checkedRadioButton.IsChecked = true;

            ColorControl.Color = ActualThemeColor;

            cbUseDefaultTheme.IsChecked = Theme == null;
            cbUseDefaultColor.IsChecked = !ThemeColor.HasValue;

            OnPropertiesChanged(nameof(ActualTheme), nameof(ActualThemeColor),
                nameof(IsThemeSelectorEnabled), nameof(IsColorSelectorEnabled), nameof(IsColorControlEnabled));
            
            _isUpdating = false;
        }

        private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            foreach (RadioButton btn in ThemeList.Children)
            {
                if (Equals(btn.IsChecked, true))
                {
                    Theme = (MwiThemeInfo)btn.Content;
                    break;
                }
            }
            _isUpdating = false;

            UpdateUI();
        }


        private void OnUseDefaultColorChanged(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox) sender;
            ThemeColor = cb.IsChecked == true ? (Color?)null : DefaultThemeColor;
            UpdateUI();
        }

        private void OnUseDefaultThemeChanged(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            Theme = cb.IsChecked == true ? null : ActualTheme;
            UpdateUI();
        }

        private void OnApplyButtonClick(object sender, RoutedEventArgs e)
        {
            IsSaved = true;
            ApplicationCommands.Close.Execute(null, this);
        }

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
