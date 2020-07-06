using System.Threading.Tasks;
using System.Windows;
using LightyTest.Source;

namespace LightySample
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
            LightBox.Show(this, new SampleDialog());

            await Task.Delay(1000);
            LightBox.Show(this, new SampleDialog());

            await Task.Delay(1000);
            LightBox.Show(this, new SampleDialog());
        }
    }
}
