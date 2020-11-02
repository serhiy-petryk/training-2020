using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfInvestigate.Controls;
using WpfInvestigate.Controls.DialogItems;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// DialogTests.xaml の相互作用ロジック
    /// </summary>
    public partial class DialogTests
    {
        private BitmapImage _testImage = new BitmapImage(new Uri("/TestViews/Resources/1.jpg", UriKind.RelativeOrAbsolute));

        public DialogTests()
        {
            InitializeComponent();
            DataContext = this;
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
            var image = new Image {Source = _testImage};
            DialogItems.Show(this, image);
        }

        private void OnClickShowInGridPopup(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var image = new Image {Source = _testImage};
            DialogItems.Show(this.subGrid, image);
        }

        #region 別ウィンドウで開くサンプルなど
        private async void OnClickShowMultiplePopup(object sender, RoutedEventArgs e)
        {
            var style = Resources["MultipleDialogStyle"] as Style;

            DialogItems.Show(this, new SampleDialog(), style);

            await Task.Delay(500);
            DialogItems.Show(this, new SampleDialog(), style);

            await Task.Delay(500);
            DialogItems.Show(this, new SampleDialog(), style);
        }

        private void ShowBuiltinStyleWindowPopup(object sender, RoutedEventArgs e)
        {
            var style = Resources["DialogBultinStyle"] as Style;
            var image = new Image {Source = _testImage};
            DialogItems.Show(this, image, style);
        }

        private void ShowCustomStyleWindowPopup(object sender, RoutedEventArgs e)
        {
            var style = Resources["DialogCustomStyle"] as Style;
            var image = new Image {Source = _testImage};
            DialogItems.Show(this, image, style);
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
            var image = new Image {Source = _testImage};
            image.PreviewMouseLeftButtonDown += (o, args) => ApplicationCommands.Close.Execute(null, image);
            DialogItems.Show(this, image, null, _closeOnClickBackgroundCallback);
        }

        private void OnClickShowInGrid(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var image = new Image {Source = _testImage};
            DialogItems.Show(this.subGrid, image, null, _closeOnClickBackgroundCallback);
        }

        #region 別ウィンドウで開くサンプルなど
        private async void OnClickShowMultiple(object sender, RoutedEventArgs e)
        {
            var style = Resources["MultipleDialogStyle"] as Style;
            DialogItems.Show(this, new SampleDialog(), style, items => items.CloseOnClickBackground = false);

            await Task.Delay(500);
            DialogItems.Show(this, new SampleDialog(), style);

            await Task.Delay(500);
            DialogItems.Show(this, new SampleDialog(), style);
        }

        private void ShowBuiltinStyleWindow(object sender, RoutedEventArgs e)
        {
            var style = Resources["DialogBultinStyle"] as Style;
            var image = new Image {Source = _testImage};
            DialogItems.Show(this, image, style, items => items.CloseOnClickBackground = false);
        }

        private void ShowCustomStyleWindow(object sender, RoutedEventArgs e)
        {
            var style = Resources["DialogCustomStyle"] as Style;
            var image = new Image {Source = _testImage};
            DialogItems.Show(this, image, style, items => items.CloseOnClickBackground = false);
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
                "Caption of Message block", null, new[] { "OK", "Cancel", "Right", "Left" });
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
        private async void OnClickWarningMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = await MessageBlock.ShowAsync("Message (Show Async)",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Warning, new[] { "OK", "Cancel", "Right", "Left" });
            await MessageBlock.ShowAsync($"You pressed '{aa ?? "X"}' button", null, MessageBlock.MessageBlockIcon.Info, new[] { "OK" });
        }
        private void OnClickInformationMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.Show("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Info, new[] { "OK" });
        }
        private void OnClickSuccessMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.Show("Message (Show) ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Success, new []{"OK", "Cancel"});
            MessageBlock.Show($"You pressed '{aa ?? "X" }' button", null, MessageBlock.MessageBlockIcon.Info, new[] { "OK" });
        }
    }
}
