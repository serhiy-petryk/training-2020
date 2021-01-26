using System.Windows;
using System.Windows.Controls;

namespace WpfInvestigate.Samples
{
    public partial class ResizableSample : UserControl
    {
        public ResizableSample()
        {
            InitializeComponent();
        }

        private void OnToggleMaximizedClick(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this);
            if (wnd != null)
                wnd.WindowState = wnd.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }
}
