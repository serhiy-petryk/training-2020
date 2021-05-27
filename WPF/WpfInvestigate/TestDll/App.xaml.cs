using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using WpfLib.Common;
using WpfLib.Helpers;
using WpfLib.Themes;
using WpfLib.ViewModels;

namespace TestDll
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // var vCulture = new CultureInfo("uk");
            var vCulture = Tips.InvariantCulture;

            Thread.CurrentThread.CurrentCulture = vCulture;
            Thread.CurrentThread.CurrentUICulture = vCulture;
            CultureInfo.DefaultThreadCurrentCulture = vCulture;
            CultureInfo.DefaultThreadCurrentUICulture = vCulture;

            var a1 = Thread.CurrentThread.CurrentCulture;
            var a2 = Thread.CurrentThread.CurrentUICulture;
            var a3 = CultureInfo.DefaultThreadCurrentCulture;
            var a4 = CultureInfo.DefaultThreadCurrentUICulture;

            MwiThemeInfo.Themes.Insert(0, new MwiThemeInfo("Test dll theme", null, new[] { $"pack://application:,,,/TestDll;component/Themes/Mwi.TestTheme.xaml" }));

            MwiAppViewModel.Instance.ChangeTheme(MwiThemeInfo.Themes[0]);

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));

            SelectAllOnFocusForTextBox.ActivateGlobally();

            base.OnStartup(e);
        }

    }
}
