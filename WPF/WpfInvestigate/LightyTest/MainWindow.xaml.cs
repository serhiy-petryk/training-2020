using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LightyTest.Source;

namespace LightySample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnClickShowButton(object sender, RoutedEventArgs e)
        {
            LightBox.Show(this, new SampleDialog());
            MessageBox.Show("Hello.");
        }

        private void OnClickShowDialogButton(object sender, RoutedEventArgs e)
        {
            LightBox.ShowDialog(this, new SampleDialog());
            MessageBox.Show("Hello.");
        }

        private async void OnClickShowAsyncButton(object sender, RoutedEventArgs e)
        {
            await LightBox.ShowAsync(this, new SampleDialog());
            MessageBox.Show("Hello.");
        }



        private void OnClickShowUserControl(object sender, RoutedEventArgs e)
        {
            LightBox.Show(this, new SampleDialog());
        }
        private void OnClickShowImage(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            LightBox.Show(this, image);
        }

        private void OnClickShowInGrid(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            LightBox.Show(this.subGrid, image);
        }

        #region 別ウィンドウで開くサンプルなど
        private void OnClickShowMultiple(object sender, RoutedEventArgs e)
        {
            var win = new MultipleLightBoxWindow();
            win.Owner = this;
            win.Show();
        }

        private void ShowBuiltinStyleWindow(object sender, RoutedEventArgs e)
        {
            var win = new BuiltinStyleWindow();
            win.Owner = this;
            win.Show();
        }

        private void ShowCustomStyleWindow(object sender, RoutedEventArgs e)
        {
            var win = new CustomStyleWindow();
            win.Owner = this;
            win.Show();
        }
        #endregion

        // ================================================
        private void OnClickShowButtonDB(object sender, RoutedEventArgs e)
        {
            DialogBlock.Show(this, new SampleDialog());
            MessageBox.Show("Hello.");
        }

        private void OnClickShowDialogButtonDB(object sender, RoutedEventArgs e)
        {
            DialogBlock.ShowDialog(this, new SampleDialog());
            MessageBox.Show("Hello.");
        }

        private async void OnClickShowAsyncButtonDB(object sender, RoutedEventArgs e)
        {
            await DialogBlock.ShowAsync(this, new SampleDialog());
            MessageBox.Show("Hello.");
        }

        private void OnClickShowUserControlDB(object sender, RoutedEventArgs e)
        {
            DialogBlock.Show(this, new SampleDialog());
        }
        private void OnClickShowImageDB(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            DialogBlock.Show(this, image);
        }

        private void OnClickShowInGridDB(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            DialogBlock.Show(this.subGrid, image);
        }

        private void ShowBuiltinStyleWindowDB(object sender, RoutedEventArgs e)
        {
            var win = new BuiltinStyleWindowDB();
            win.Owner = this;
            win.Show();
        }

        private void ShowCustomStyleWindowDB(object sender, RoutedEventArgs e)
        {
            var win = new CustomStyleWindowDB();
            win.Owner = this;
            win.Show();
        }


    }
}
