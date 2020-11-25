using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Helpers;

namespace WpfInvestigate
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

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));

            SelectAllOnFocusForTextBox.ActivateGlobally();

            base.OnStartup(e);
        }
    }
}
