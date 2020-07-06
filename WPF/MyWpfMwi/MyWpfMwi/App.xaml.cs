using System.Windows;
using MyWpfMwi.ViewModels;

namespace MyWpfMwi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_OnStartup(object sender, StartupEventArgs e) => ApplicationViewModel.Instance.CmdToggleScheme.Execute(null);
    }
}
