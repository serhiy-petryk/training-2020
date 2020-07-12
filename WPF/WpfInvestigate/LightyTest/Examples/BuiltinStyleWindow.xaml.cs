using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LightyTest.Source;

namespace LightyTest.Examples
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
            image.Source = new BitmapImage(new Uri("/Images/1.jpg", UriKind.Relative));
            DialogItems.Show(this, image, items => items.CloseOnClickBackground = false);
        }
    }
}
