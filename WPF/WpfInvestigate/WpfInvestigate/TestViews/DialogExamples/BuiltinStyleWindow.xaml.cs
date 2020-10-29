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
    public partial class BuiltinStyleWindow : Window
    {
        public BuiltinStyleWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("/TestViews/DialogExamples/1.jpg", UriKind.RelativeOrAbsolute));
            DialogItems.Show(this, image, null, items => items.CloseOnClickBackground = false);
        }
    }
}
