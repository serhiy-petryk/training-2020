using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using WpfInvestigate.Common;
using WpfInvestigate.Themes;

namespace WpfInvestigate.ViewModels
{
    public class MwiAppViewModel: UIElement, INotifyPropertyChanged
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
        // public MwiContainer ContainerControl { get; set; }
        // public FontFamily DefaultFontFamily { get; } = new FontFamily("Segoe UI");
        // public Dock WindowsBarLocation { get; } = Dock.Top;
        public RelayCommand CmdChangeTheme { get; }

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

        public MwiThemeInfo CurrentTheme { get; private set; }
        public int Test => 12;

        public MwiAppViewModel()
        {
            CmdChangeTheme = new RelayCommand(o => ChangeTheme(null));
        }

        public void ChangeTheme(MwiThemeInfo theme)
        {
            var oldTheme = CurrentTheme;
            CurrentTheme = theme;
            if (CurrentTheme == null)
            {
                var k = Array.IndexOf(MwiThemeInfo.Themes, oldTheme);
                var newK = k >= 0 && k + 1 < MwiThemeInfo.Themes.Length ? k + 1 : 0;
                CurrentTheme = MwiThemeInfo.Themes[newK];
            }

            CurrentTheme.ApplyTheme();
            OnPropertiesChanged(nameof(CurrentTheme));
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
