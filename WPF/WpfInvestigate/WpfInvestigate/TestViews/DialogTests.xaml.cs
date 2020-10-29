using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfInvestigate.Controls.DialogItems;
using WpfInvestigate.TestViews.DialogExamples;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// DialogTests.xaml の相互作用ロジック
    /// </summary>
    public partial class DialogTests
    {
        public DialogTests()
        {
            InitializeComponent();
        }

        private Style MovableDialogStyle => TryFindResource("MovableDialogStyle") as Style;
        private void ClearSubGrid()
        {
            var layer = AdornerLayer.GetAdornerLayer(subGrid);
            if (layer != null && layer.GetAdorners(subGrid) != null)
                foreach (var toRemove in layer.GetAdorners(subGrid))
                    layer.Remove(toRemove);
        }

        private void OnClickShowButtonPopup(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(null, new SampleDialog());
            MessageBox.Show("dialog item already shown");
        }

        private void OnClickShowDialogButtonPopup(object sender, RoutedEventArgs e)
        {
            DialogItems.ShowDialog(null, new SampleDialog());
            MessageBox.Show("dialog item already shown");
        }

        private async void OnClickShowAsyncButtonPopup(object sender, RoutedEventArgs e)
        {
            await DialogItems.ShowAsync(null, new SampleDialog());
            MessageBox.Show("dialog item already shown");
        }

        private void OnClickShowUserControlPopup(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(this, new SampleDialog());
        }
        private void OnClickShowImagePopup(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("/TestViews/DialogExamples/1.jpg", UriKind.RelativeOrAbsolute));
            DialogItems.Show(this, image);
        }

        private void OnClickShowInGridPopup(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var image = new Image();
            image.Source = new BitmapImage(new Uri("/TestViews/DialogExamples/1.jpg", UriKind.RelativeOrAbsolute));
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

        private void OnClickShowButtonMovablePopup(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var content = new SampleDialogMovable();
            DialogItems.Show(subGrid, content, MovableDialogStyle, DialogItems.GetAfterCreationCallbackForMovableDialog(content, true));
            MessageBox.Show("dialog item already shown");
        }
        private void OnClickShowDialogMovableButtonPopup(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var content = new SampleDialogMovable();
            DialogItems.ShowDialog(subGrid, content, MovableDialogStyle, DialogItems.GetAfterCreationCallbackForMovableDialog(content, true));
            MessageBox.Show("dialog item already shown");
        }
        private async void OnClickShowAsyncMovableButtonPopup(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var content = new SampleDialogMovable();
            await DialogItems.ShowAsync(subGrid, content, MovableDialogStyle, DialogItems.GetAfterCreationCallbackForMovableDialog(content, true));
            MessageBox.Show("dialog item already shown");
        }
        #endregion

        // ================================================
        private Action<DialogItems> _closeOnClickBackgroundCallback = items => items.CloseOnClickBackground = false;
        private void OnClickShowButton(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(this, new SampleDialog(), null, _closeOnClickBackgroundCallback);
            MessageBox.Show("dialog item already shown");
        }

        private void OnClickShowDialogButton(object sender, RoutedEventArgs e)
        {
            DialogItems.ShowDialog(this, new SampleDialog(), null, _closeOnClickBackgroundCallback);
            MessageBox.Show("dialog item already shown");
        }

        private async void OnClickShowAsyncButton(object sender, RoutedEventArgs e)
        {
            await DialogItems.ShowAsync(this, new SampleDialog(), null, _closeOnClickBackgroundCallback);
            MessageBox.Show("dialog item already shown");
        }

        private void OnClickShowUserControl(object sender, RoutedEventArgs e)
        {
            DialogItems.Show(this, new SampleDialog(), null, _closeOnClickBackgroundCallback);
        }
        private void OnClickShowImage(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("/TestViews/DialogExamples/1.jpg", UriKind.RelativeOrAbsolute));
            image.PreviewMouseLeftButtonDown += (o, args) => ApplicationCommands.Close.Execute(null, image);
            DialogItems.Show(this, image, null, _closeOnClickBackgroundCallback);
        }

        private void OnClickShowInGrid(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var image = new Image();
            image.Source = new BitmapImage(new Uri("/TestViews/DialogExamples/1.jpg", UriKind.RelativeOrAbsolute));
            DialogItems.Show(this.subGrid, image, null, _closeOnClickBackgroundCallback);
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
        private void OnClickShowButtonMovable(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var content = new SampleDialogMovable();
            DialogItems.Show(subGrid, content, MovableDialogStyle, DialogItems.GetAfterCreationCallbackForMovableDialog(content, false));
            MessageBox.Show("dialog item already shown");
        }
        private void OnClickShowDialogMovableButton(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var content = new SampleDialogMovable();
            DialogItems.ShowDialog(subGrid, content, MovableDialogStyle, DialogItems.GetAfterCreationCallbackForMovableDialog(content, false));
            MessageBox.Show("dialog item already shown");
        }
        private async void OnClickShowAsyncMovableButton(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var content = new SampleDialogMovable();
            await DialogItems.ShowAsync(subGrid, content, MovableDialogStyle, DialogItems.GetAfterCreationCallbackForMovableDialog(content, false));
            MessageBox.Show("dialog item already shown");
        }
        #endregion

        private void OnClickMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.Show("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block");
        }
        private void OnClickQuestionMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.Show("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Question);
        }
        private void OnClickStopMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.Show("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Stop);
        }
        private void OnClickErrorMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.Show("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Error);
        }
        private void OnClickWarningMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.Show("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Warning);
        }
        private void OnClickInformationMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.Show("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Information);
        }
        private void OnClickSuccessMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.Show("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Success, new []{"OK", "Cancel"});
            if (aa != null)
                MessageBlock.Show($"You pressed '{aa}' button", null, MessageBlock.MessageBlockIcon.Information, new[] { "OK" });
        }
    }
}
