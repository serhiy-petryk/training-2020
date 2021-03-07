﻿using System;
using System.Linq;
using System.Windows;
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
        public RelayCommand CmdToggleTheme { get; }

        public FrameworkElement DialogHost
        {
            get
            {
                var activeWnd = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                if (activeWnd is MwiStartup mwiStartup && mwiStartup.TopControl.Template.FindName("ContentBorder", mwiStartup.TopControl) is FrameworkElement topContentControl)
                    return topContentControl;
                return activeWnd;
            }
        }

        private MwiThemeInfo _currentTheme;

        public MwiAppViewModel()
        {
            CmdToggleTheme = new RelayCommand(o => ToggleTheme(null));
        }

        public void ToggleTheme(MwiThemeInfo theme)
        {
            var oldTheme = _currentTheme;
            _currentTheme = theme;
            if (_currentTheme == null)
            {
                var k = Array.IndexOf(MwiThemeInfo.Themes, oldTheme);
                var newK = k >= 0 && k + 1 < MwiThemeInfo.Themes.Length ? k + 1 : 0;
                _currentTheme = MwiThemeInfo.Themes[newK];
            }

            _currentTheme.ApplyTheme();

            var args = new RoutedPropertyChangedEventArgs<MwiThemeInfo>(oldTheme, _currentTheme) { RoutedEvent = ThemeChangedEvent };
            RaiseEvent(args);
        }
        #endregion
    }
}
