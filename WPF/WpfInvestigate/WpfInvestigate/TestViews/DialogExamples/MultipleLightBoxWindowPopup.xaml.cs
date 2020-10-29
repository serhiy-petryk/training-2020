using System.Threading.Tasks;
using System.Windows;
using WpfInvestigate.Controls.DialogItems;

namespace WpfInvestigate.TestViews.DialogExamples
{
    /// <summary>
    /// MultipleLightBoxWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MultipleLightBoxWindowPopup : Window
    {
        public MultipleLightBoxWindowPopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(this, new SampleDialog());

            await Task.Delay(1000);
            // DialogItems.Show(this, new SampleDialog(), null, items => items.CloseOnClickBackground = false);
            DialogItems.Show(this, new SampleDialog());

            await Task.Delay(1000);
            DialogItems.Show(this, new SampleDialog());
        }
    }
}
