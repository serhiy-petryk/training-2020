using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LightyTest.Service;
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

        private void OnClickShowButtonPopup(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(this, new SampleDialog());
            MessageBox.Show("dialog item already shown");
        }

        private void OnClickShowDialogButtonPopup(object sender, RoutedEventArgs e)
        {
            DialogItems.ShowDialog(this, new SampleDialog());
            MessageBox.Show("dialog item already shown");
        }

        private async void OnClickShowAsyncButtonPopup(object sender, RoutedEventArgs e)
        {
            await DialogItems.ShowAsync(this, new SampleDialog());
            MessageBox.Show("dialog item already shown");
        }

        private void OnClickShowUserControlPopup(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(this, new SampleDialog());
        }
        private void OnClickShowImagePopup(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            DialogItems.Show(this, image);
        }

        private void OnClickShowInGridPopup(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            DialogItems.Show(this.subGrid, image);
        }

        #region 別ウィンドウで開くサンプルなど
        private void OnClickShowMultiplePopup(object sender, RoutedEventArgs e)
        {
            var win = new MultipleLightBoxWindowPopup();
            win.Owner = this;
            win.Show();
        }

        private void ShowBuiltinStyleWindowPopup(object sender, RoutedEventArgs e)
        {
            var win = new BuiltinStyleWindowPopup();
            win.Owner = this;
            win.Show();
        }

        private void ShowCustomStyleWindowPopup(object sender, RoutedEventArgs e)
        {
            var win = new CustomStyleWindowPopup();
            win.Owner = this;
            win.Show();
        }
        private void OnClickShowMovablePopup(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(this, new SampleDialogMovable(), a =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    var itemsPanel = Tips.GetVisualChildren(a).OfType<FrameworkElement>().FirstOrDefault(x => x.Name == "ItemsPanel");
                    if (itemsPanel != null)
                    {
                        itemsPanel.HorizontalAlignment = HorizontalAlignment.Left;
                        itemsPanel.VerticalAlignment = VerticalAlignment.Top;
                    }
                    a.ItemContainerStyle = null;
                    a.CloseOnClickBackground = false;

                    // var aa1 = Tips.GetVisualParents(a).ToArray();
                    // var aa2 = Tips.GetVisualChildren(a).ToArray();
                }));
            });
        }
        #endregion

        // ================================================
        private Action<DialogItems> _closeOnClickBackgroundCallback = items => items.CloseOnClickBackground = false;
        private void OnClickShowButton(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(this, new SampleDialog(), _closeOnClickBackgroundCallback);
            MessageBox.Show("dialog item already shown");
        }

        private void OnClickShowDialogButton(object sender, RoutedEventArgs e)
        {
            DialogItems.ShowDialog(this, new SampleDialog(), _closeOnClickBackgroundCallback);
            MessageBox.Show("dialog item already shown");
        }

        private async void OnClickShowAsyncButton(object sender, RoutedEventArgs e)
        {
            await DialogItems.ShowAsync(this, new SampleDialog(), _closeOnClickBackgroundCallback);
            MessageBox.Show("dialog item already shown");
        }

        private void OnClickShowUserControl(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(this, new SampleDialog(), _closeOnClickBackgroundCallback);
        }
        private void OnClickShowImage(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            image.PreviewMouseLeftButtonDown += (o, args) => ApplicationCommands.Close.Execute(null, image);
            DialogItems.Show(this, image, _closeOnClickBackgroundCallback);
        }

        private void OnClickShowInGrid(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            DialogItems.Show(this.subGrid, image, _closeOnClickBackgroundCallback);
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
    }
}
