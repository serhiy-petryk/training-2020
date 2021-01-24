using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using WpfInvestigate.Samples;

namespace WpfInvestigate.Obsolete.TestViews
{
    /// <summary>
    /// DialogTests.xaml の相互作用ロジック
    /// </summary>
    public partial class DialogItemsTests
    {
        private BitmapImage _testImage = new BitmapImage(new Uri("/Resources/1.jpg", UriKind.RelativeOrAbsolute));

        public DialogItemsTests()
        {
            InitializeComponent();
            DataContext = this;
        }

        private Style WithMarginStyle => TryFindResource("WithMarginStyle") as Style;

        private void ClearSubGrid()
        {
            var layer = AdornerLayer.GetAdornerLayer(subGrid);
            if (layer != null && layer.GetAdorners(subGrid) != null)
                foreach (var toRemove in layer.GetAdorners(subGrid))
                    layer.Remove(toRemove);
        }


        private void OnClickShowButtonPopup(object sender, RoutedEventArgs e)
        {
            var dialogItems = new DialogItems{CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value};
            dialogItems.Show(new SampleDialogMovable(), Host);
            MessageBox.Show("dialog item already shown");
        }

        private void OnClickShowDialogButtonPopup(object sender, RoutedEventArgs e)
        {
            var dialogItems = new DialogItems { CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            dialogItems.ShowDialog(new SampleDialogMovable(), Host);
            MessageBox.Show("dialog item already shown");
        }

        private async void OnClickShowAsyncButtonPopup(object sender, RoutedEventArgs e)
        {
            var dialogItems = new DialogItems { CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            await dialogItems.ShowAsync(new SampleDialogMovable(), Host);
            MessageBox.Show("dialog item already shown");
        }

        private void OnClickShowUserControlPopup(object sender, RoutedEventArgs e)
        {
            var dialogItems = new DialogItems { CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            dialogItems.Show(new SampleDialogMovable(), this);
        }
        private void OnClickShowImagePopup(object sender, RoutedEventArgs e)
        {
            var image = new Image {Source = _testImage};
            var dialogItems = new DialogItems{Style = WithMarginStyle, CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            dialogItems.Show(image, this);
        }

        private void OnClickShowInGridPopup(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var image = new Image {Source = _testImage};
            var dialogItems = new DialogItems { Style = WithMarginStyle, CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            dialogItems.Show(image, subGrid);
        }

        #region 別ウィンドウで開くサンプルなど
        private async void OnClickShowMultiplePopup(object sender, RoutedEventArgs e)
        {
            var dialogItems = new DialogItems { Style = Resources["MultipleDialogStyle"] as Style, CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            dialogItems.Show(new SampleDialogMovable(), this);

            await Task.Delay(500);
            dialogItems.Items.Add(new SampleDialogMovable());

            await Task.Delay(500);
            dialogItems.Items.Add(new SampleDialogMovable());
        }

        private async void OnClickAddDialogMethodPopup(object sender, RoutedEventArgs e)
        {
            var dialogs = new DialogItems { Style = Resources["MultipleDialogStyle"] as Style, CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            dialogs.Show(null, this);

            dialogs.AddDialog(new SampleDialogMovable());
            await Task.Delay(500);

            dialogs.AddDialog(new SampleDialogMovable());
            await Task.Delay(500);

            dialogs.AddDialog(new SampleDialogMovable());
        }

        private void ShowBuiltinStyleWindowPopup(object sender, RoutedEventArgs e)
        {
            var image = new Image {Source = _testImage};
            var dialogItems = new DialogItems { Style = Resources["DialogBultinStyle"] as Style, CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            dialogItems.Show(image, this);
        }

        private void ShowCustomStyleWindowPopup(object sender, RoutedEventArgs e)
        {
            var image = new Image {Source = _testImage};
            var dialogItems = new DialogItems { Style = Resources["DialogCustomStyle"] as Style, CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            dialogItems.Show(image, this);
        }

        private void OnClickShowButtonMovablePopup(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var content = new SampleDialogMovable();
            var dialogItems = new DialogItems{ CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            dialogItems.Show(content, subGrid);
            MessageBox.Show("dialog item already shown");
        }
        private void OnClickShowDialogMovableButtonPopup(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var content = new SampleDialogMovable();
            var dialogItems = new DialogItems{ CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            dialogItems.ShowDialog(content, subGrid);
            MessageBox.Show("dialog item already shown");
        }
        private async void OnClickShowAsyncMovableButtonPopup(object sender, RoutedEventArgs e)
        {
            ClearSubGrid();
            var content = new SampleDialogMovable();
            var dialogItems = new DialogItems{ CloseOnClickBackground = CloseOnClickBackgroundCheckBox.IsChecked.Value };
            await dialogItems.ShowAsync(content, subGrid);
            MessageBox.Show("dialog item already shown");
        }
        #endregion

        private void OnClickSyncMessageBlock(object sender, RoutedEventArgs e)
        {
            var a1 = MessageBlock.Show("Message text Message text Message text Message text Message text Message text ",
                "Show Sync", null, new[] { "OK", "Cancel", "Right", "Left" });
        }
        private async void OnClickAsyncMessageBlock(object sender, RoutedEventArgs e)
        {
            var a1 = await MessageBlock.ShowAsync("Message text Message text Message text Message text Message text Message text ",
                "Show Async", null, new[] { "OK", "Cancel", "Right", "Left" });
        }
        private void OnClickDialogMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.ShowDialog("Message text Message text Message text Message text Message text Message text ",
                "Show Dialog", null, new[] { "OK", "Cancel", "Right", "Left" });
        }
        private void OnClickQuestionMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.ShowDialog("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Question);
        }
        private void OnClickStopMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.ShowDialog("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Stop);
        }
        private void OnClickErrorMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.ShowDialog("Message text Message text Message text Message text Message text Message text ",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Error);
        }
        private async void OnClickWarningMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = await MessageBlock.ShowAsync("Message (Show Async)", "Caption of Message block",
                MessageBlock.MessageBlockIcon.Warning, new[] {"OK", "Cancel", "Right", "Left"});

            await MessageBlock.ShowAsync($"You pressed '{aa ?? "X"}' button", null, MessageBlock.MessageBlockIcon.Info, new[] { "OK" });
        }
        private void OnClickInformationMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.ShowDialog("Message text Message text Message text Message text Message text Message text",
                "Caption of Message block", MessageBlock.MessageBlockIcon.Info, new[] { "OK" });
        }
        private void OnClickSuccessMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.ShowDialog("Message (Show) ", "Caption of Message block",
                MessageBlock.MessageBlockIcon.Success, new[] {"OK", "Cancel"}, false);

            MessageBlock.ShowDialog($"You pressed '{aa ?? "X" }' button", null, MessageBlock.MessageBlockIcon.Info, new[] { "OK" });
        }
        private void OnClickShortMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.ShowDialog("Test message 0 1 2 3 4", "Show Dialog", MessageBlock.MessageBlockIcon.Question);
        }
    }
}
