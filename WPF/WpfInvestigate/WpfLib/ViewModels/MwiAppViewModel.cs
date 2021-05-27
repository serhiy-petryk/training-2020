using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WpfLib.Common;
using WpfLib.Themes;

namespace WpfLib.ViewModels
{
    public class MwiAppViewModel: DependencyObject, INotifyPropertyChanged
    {
        #region ================  Static section  =====================
        public static MwiAppViewModel Instance = new MwiAppViewModel {CurrentTheme = MwiThemeInfo.Themes[0]};
        #endregion

        #region ================  Instance section  ====================
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register(nameof(ScaleValue), typeof(double), typeof(MwiAppViewModel), new UIPropertyMetadata(1.0));
        public double ScaleValue
        {
            get => (double)GetValue(ScaleValueProperty);
            set => SetValue(ScaleValueProperty, value);
        }
        //=============================
        public static readonly DependencyProperty AppColorProperty = DependencyProperty.Register(nameof(AppColor), typeof(Color), typeof(MwiAppViewModel), new UIPropertyMetadata(Colors.Yellow, OnAppColorChanged));

        private static void OnAppColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var model = (MwiAppViewModel)d;
            model.OnPropertiesChanged(nameof(AppColor));
        }

        public Color AppColor
        {
            get => (Color)GetValue(AppColorProperty);
            set => SetValue(AppColorProperty, value);
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
                var k = MwiThemeInfo.Themes.IndexOf(oldTheme);
                var newK = k >= 0 && k + 1 < MwiThemeInfo.Themes.Count ? k + 1 : 0;
                CurrentTheme = MwiThemeInfo.Themes[newK];
            }

            UpdateUI();
        }
        #endregion

        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateUI() => OnPropertiesChanged(nameof(CurrentTheme), nameof(AppColor));

        #endregion
    }
}
