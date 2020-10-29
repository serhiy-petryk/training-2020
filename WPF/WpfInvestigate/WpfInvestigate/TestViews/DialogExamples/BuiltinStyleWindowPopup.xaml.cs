using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfInvestigate.Controls.DialogItems;

namespace WpfInvestigate.TestViews.DialogExamples
{
    /// <summary>
    /// BuiltinStyleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BuiltinStyleWindowPopup : Window
    {
        public BuiltinStyleWindowPopup()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("/TestViews/DialogExamples/1.jpg", UriKind.Relative));
            DialogItems.Show(this, image);
        }
    }
}
