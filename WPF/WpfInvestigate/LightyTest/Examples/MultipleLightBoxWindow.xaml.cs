using System.Threading.Tasks;
using System.Windows;
using LightyTest.Source;

namespace LightyTest.Examples
{
    /// <summary>
    /// MultipleLightBoxWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MultipleLightBoxWindow : Window
    {
        public MultipleLightBoxWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(this, new SampleDialog(), null, items => items.CloseOnClickBackground = false);

            await Task.Delay(1000);
            DialogItems.Show(this, new SampleDialog(), null, items => items.CloseOnClickBackground = false);

            await Task.Delay(1000);
            DialogItems.Show(this, new SampleDialog(), null, items => items.CloseOnClickBackground = false);
        }
    }
}
