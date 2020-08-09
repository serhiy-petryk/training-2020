using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyWpfMwi.Common;
using MyWpfMwi.Mwi;
using MyWpfMwi.Themes;

namespace MyWpfMwi.ViewModels
{
    public class AppViewModel : UIElement, INotifyPropertyChanged
    {
        //==========  Static section  ===========
        public static AppViewModel Instance = new AppViewModel();

        //=========================================
        //==========  Instance section  ===========
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register(nameof(ScaleValue), typeof(double), typeof(AppViewModel), new UIPropertyMetadata(1.0));
        public static readonly RoutedEvent ThemeChangedEvent = EventManager.RegisterRoutedEvent("ThemeChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ThemeInfo>), typeof(AppViewModel));

        public double ScaleValue
        {
            get => (double)GetValue(ScaleValueProperty);
            set => SetValue(ScaleValueProperty, value);
        }

        public event RoutedPropertyChangedEventHandler<ThemeInfo> ThemeChanged
        {
            add => AddHandler(ThemeChangedEvent, value);
            remove => RemoveHandler(ThemeChangedEvent, value);
        }

        public MwiContainer ContainerControl { get; set; }
        public FontFamily DefaultFontFamily { get; } = new FontFamily("Segoe UI");
        public Dock WindowsBarLocation { get; } = Dock.Top;
        public RelayCommand CmdToggleScheme { get; }

        private ThemeInfo _currentTheme;

        public AppViewModel() => CmdToggleScheme = new RelayCommand(ToggleTheme);

        //=========================
        private void ToggleTheme(object parameter)
        {
            var oldTheme = _currentTheme;
            _currentTheme = ThemeInfo.Themes.First(t => t != _currentTheme);
            _currentTheme.ApplyTheme();

            var args = new RoutedPropertyChangedEventArgs<ThemeInfo>(oldTheme, _currentTheme) { RoutedEvent = ThemeChangedEvent };
            RaiseEvent(args);
        }

        //===========  INotifyPropertyChanged  =======================
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertiesChanged(string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
