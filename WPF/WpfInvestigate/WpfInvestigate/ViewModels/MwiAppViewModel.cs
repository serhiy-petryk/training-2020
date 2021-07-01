using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace WpfInvestigate.ViewModels
{
    public class MwiAppViewModel: DependencyObject
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
        //========
        public static readonly DependencyProperty CultureProperty = DependencyProperty.Register(nameof(Culture), typeof(CultureInfo), typeof(MwiAppViewModel), new UIPropertyMetadata(Thread.CurrentThread.CurrentCulture, OnCultureChanged));

        private static void OnCultureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newCulture = e.NewValue as CultureInfo ?? CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;
            CultureInfo.DefaultThreadCurrentCulture = newCulture;
            CultureInfo.DefaultThreadCurrentUICulture = newCulture;
            // FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));
        }

        public CultureInfo Culture
        {
            get => (CultureInfo)GetValue(CultureProperty);
            set => SetValue(CultureProperty, value);
        }
        //========
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
        #endregion
    }
}
