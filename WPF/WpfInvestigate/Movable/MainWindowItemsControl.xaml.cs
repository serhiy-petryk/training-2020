using System.Windows;
using WpfInvestigate.Samples;

namespace Movable
{
    /// <summary>
    /// Interaction logic for MainWindowItemsControl.xaml
    /// </summary>
    public partial class MainWindowItemsControl : Window
    {
        public MainWindowItemsControl()
        {
            InitializeComponent();
        }

        private void AddChildToItemsControl_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            ItemsControlCanvas.Items.Add(a1);
        }
    }
}
