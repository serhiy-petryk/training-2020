using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LightyTest.Source;

namespace LightySample
{
    /// <summary>
    /// BuiltinStyleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BuiltinStyleWindowDB : Window
    {
        public BuiltinStyleWindowDB()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            DialogBlock.Show(this, image);
        }
    }
}
