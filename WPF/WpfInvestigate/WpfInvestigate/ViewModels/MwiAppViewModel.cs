using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using WpfInvestigate.Common;
using WpfInvestigate.Themes;

namespace WpfInvestigate.ViewModels
{
    public class MwiAppViewModel: UIElement
    {
        #region ================  Static section  =====================
        public static MwiAppViewModel Instance = new MwiAppViewModel();
        #endregion

        #region ================  Instance section  ====================
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register(nameof(ScaleValue), typeof(double), typeof(MwiAppViewModel), new UIPropertyMetadata(1.0));
        public double ScaleValue
        {
            get => (double)GetValue(ScaleValueProperty);
            set => SetValue(ScaleValueProperty, value);
        }
        //=============================
        public static readonly RoutedEvent ThemeChangedEvent = EventManager.RegisterRoutedEvent("ThemeChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<MwiThemeInfo>), typeof(MwiAppViewModel));
        public event RoutedPropertyChangedEventHandler<MwiThemeInfo> ThemeChanged
        {
            add => AddHandler(ThemeChangedEvent, value);
            remove => RemoveHandler(ThemeChangedEvent, value);
        }
        //=============================
        // public MwiContainer ContainerControl { get; set; }
        // public FontFamily DefaultFontFamily { get; } = new FontFamily("Segoe UI");
        // public Dock WindowsBarLocation { get; } = Dock.Top;
        public RelayCommand CmdToggleScheme { get; }

        private MwiThemeInfo _currentTheme;

        public MwiAppViewModel()
        {
            CmdToggleScheme = new RelayCommand(o => ApplyTheme(null));
        }

        public void ApplyTheme(MwiThemeInfo theme)
        {
            var oldTheme = _currentTheme;
            _currentTheme = theme == null ? MwiThemeInfo.Themes.First(t => t != _currentTheme) : theme;
            _currentTheme.ApplyTheme();

            var args = new RoutedPropertyChangedEventArgs<MwiThemeInfo>(oldTheme, _currentTheme) { RoutedEvent = ThemeChangedEvent };
            RaiseEvent(args);
        }
        #endregion
    }
}
