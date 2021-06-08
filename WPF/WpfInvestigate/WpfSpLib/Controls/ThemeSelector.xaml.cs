using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfSpLib.Common;
using WpfSpLib.Helpers;
using WpfSpLib.Themes;

namespace WpfSpLib.Controls
{
    /// <summary>
    /// Interaction logic for ThemeSelector.xaml
    /// </summary>
    public partial class ThemeSelector: INotifyPropertyChanged
    {
        private IColorThemeSupport _target;

        public IColorThemeSupport Target
        {
            get => _target;
            set
            {
                _target = value;

                var d = _target as DependencyObject;
                DefaultTheme = _target?.ActualTheme;
                if (_target?.Theme != null)
                {
                    var a1 = d.GetVisualParents().OfType<IColorThemeSupport>().FirstOrDefault(a => !Equals(a, _target) && a.Theme != null);
                    DefaultTheme = a1?.Theme ?? MwiThemeInfo.DefaultTheme;
                }

                DefaultThemeColor = _target?.ActualThemeColor ?? MwiThemeInfo.DefaultThemeColor;
                if (_target?.ThemeColor != null)
                {
                    var a1 = d.GetVisualParents().OfType<IColorThemeSupport>().FirstOrDefault(a => !Equals(a, _target) && a.ThemeColor != null);
                    DefaultThemeColor = a1?.ThemeColor ?? MwiThemeInfo.DefaultThemeColor;
                }

                Theme = _target?.Theme;
                ThemeColor = _target?.ThemeColor;
            }
        }

        public List<Tuple<MwiThemeInfo, Color?>> Changes = new List<Tuple<MwiThemeInfo, Color?>>();
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
        public bool IsRestoreButtonEnabled => Changes.Count > 0;

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

            OnPropertiesChanged(nameof(ActualTheme), nameof(ActualThemeColor), nameof(IsRestoreButtonEnabled),
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
            ThemeColor = cb.IsChecked == true ? (Color?)null : ActualThemeColor;
            UpdateUI();
        }

        private void OnUseDefaultThemeChanged(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            Theme = cb.IsChecked == true ? null : ActualTheme;
            UpdateUI();
        }

        private void OnApplyAndCloseButtonClick(object sender, RoutedEventArgs e)
        {
            ApplyTheme();
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

        private void OnApplyButtonClick(object sender, RoutedEventArgs e)
        {
            Changes.Add(new Tuple<MwiThemeInfo, Color?>(Target.Theme, Target.ThemeColor));
            ApplyTheme();
        }

        private void OnRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (Changes.Count > 0)
            {
                Theme = Changes[Changes.Count - 1].Item1;
                ThemeColor = Changes[Changes.Count - 1].Item2;
                Changes.RemoveAt(Changes.Count - 1);
                ApplyTheme();
            }
        }

        private void ApplyTheme()
        {
            Target.Theme = Theme;
            if (Target.ActualTheme.FixedColor.HasValue)
            {
                if (Target is Control cntrl)
                    cntrl.Background = null;
            }
            else
                Target.ThemeColor = ThemeColor;
            UpdateUI();
        }
    }
}
