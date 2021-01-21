using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using WpfInvestigate.Controls;
using WpfInvestigate.Samples;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ResizingControl.xaml
    /// </summary>
    public partial class ResizingControlTests : Window
    {
        public ResizingControlTests()
        {
            InitializeComponent();

            var resizingControl = new ResizingControl
                {Content = new ResizableContentTemplateSample(), Margin = new Thickness(200, 100, 0, 0)};
            GridPanel.Children.Add(resizingControl);

            var resizingControl2 = new ResizingControl
                { Content = new ResizableSample(), Margin = new Thickness(20, 10, 0, 0), ToolTip = "No Width/Height" };
            GridPanel.Children.Add(resizingControl2);

            var resizingControl3 = new ResizingControl
            {
                Content = new ResizableSample{Width = double.NaN, Height = double.NaN}, Margin = new Thickness(200, 200, 0, 0),
                Width = 150, Height = 150, LimitPositionToPanelBounds = true, ToolTip="Width/Height=150"
            };
            GridPanel.Children.Add(resizingControl3);
        }

        private void AddContent_OnClick(object sender, RoutedEventArgs e)
        {
            var content = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 150,
                Height = 150,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=150"
            };
            new DialogAdorner(GridPanel) {CloseOnClickBackground = true}.ShowContent(content);
        }

        private void AddWindowPanelSync_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new DialogAdorner { CloseOnClickBackground = true };

            var content = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 250,
                Height = 250,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=250"
            };
            a1.ShowContent(content);

            var content2 = new ResizingControl
            {
                Name = "Test",
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 150,
                Height = 150,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=150"
            };
            a1.ShowContent(content2);
            Debug.Print($"AddWindowPanelSync_OnClick method finished");
        }

        private async void AddWindowPanelAsync_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new DialogAdorner { CloseOnClickBackground = true };

            var content1 = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 250,
                Height = 250,
                LimitPositionToPanelBounds = true,
                ToolTip = "Content1 Width/Height=250"
            };
            await a1.ShowContentAsync(content1);

            var content2 = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 150,
                Height = 150,
                LimitPositionToPanelBounds = true,
                ToolTip = "Content2 Width/Height=150"
            };
            await a1.ShowContentAsync(content2);
            await a1.WaitUntilClosed();

            var content3 = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 200,
                Height = 250,
                LimitPositionToPanelBounds = true,
                ToolTip = "Content3 Width/Height=200/250"
            };

            await a1.ShowContentAsync(content1);
            a1.ShowContent(content3);
            a1.ShowContent(content2);
            await a1.WaitUntilClosed();

            Debug.Print($"AddWindowPanelAsync_OnClick method finished");
        }

        private void AddWindowPanelDialog_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new DialogAdorner { CloseOnClickBackground = true };

            var content1 = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 250,
                Height = 250,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=250"
            };
            a1.ShowContentDialog(content1);
            Debug.Print($"AddWindowPanelDialog_OnClick method finished");
        }

        private void MessageSync_OnClick(object sender, RoutedEventArgs e)
        {
            //DialogMessage.Show("Test message", "Caption",
            //   DialogMessage.DialogMessageIcon.Question, new[] { "OK", "Cancel", "Right", "Left" });
            DialogMessage.Show("Test message 0 1 2 3 4", "Caption", DialogMessage.DialogMessageIcon.Question);
            Debug.Print($"Message Sync");
        }

        private async void MessageAsync_OnClick(object sender, RoutedEventArgs e)
        {
            var result = await DialogMessage.ShowAsync("Test message", "Caption",
                DialogMessage.DialogMessageIcon.Question, new[] { "OK", "CancelX", "Right", "Left" });
            Debug.Print($"MessageAsync: {result}");
        }

        private void MessageDialog_OnClick(object sender, RoutedEventArgs e)
        {
            var result = DialogMessage.ShowDialog("Test message", "Caption",
                DialogMessage.DialogMessageIcon.Question, new[] {"OK", "Cancel", "Right", "Left"});
            Debug.Print($"MessageDialog: {result}");
        }

        private async void LongMessage_OnClick(object sender, RoutedEventArgs e)
        {
            var result = await DialogMessage.ShowAsync("Message text Message text Message text Message text Message text Message text",
                "Caption", DialogMessage.DialogMessageIcon.Question, new[] { "OK", "Cancel", "Right", "Left" });
        }

        private async void VeryLongMessage_OnClick(object sender, RoutedEventArgs e)
        {
            var result = await DialogMessage.ShowAsync("Message text Message text Message text Message text Message text Message textMessage text Message text Message text Message text Message text Message textMessage text Message text Message text Message text Message text Message textMessage text Message text Message text Message text Message text Message text",
                "Caption", DialogMessage.DialogMessageIcon.Question, new[] { "OK", "Cancel", "Right", "Left" });
        }

        //===============================
        private void OnClickSyncMessage(object sender, RoutedEventArgs e)
        {
            DialogMessage.Show("Message text Message text Message text Message text Message text Message text",
                "Show Sync", null, new[] { "OK", "Cancel", "Right", "Left" });
        }
        private async void OnClickAsyncMessage(object sender, RoutedEventArgs e)
        {
            var a1 = await DialogMessage.ShowAsync("Message text Message text Message text Message text Message text Message text",
                "Show Async", null, new[] { "OK", "Cancel", "Right", "Left" });
        }
        private void OnClickDialogMessage(object sender, RoutedEventArgs e)
        {
            var aa = DialogMessage.ShowDialog("Message text Message text Message text Message text Message text Message text",
                "Show Dialog", null, new[] { "OK", "Cancel", "Right", "Left" });
        }
        private void OnClickQuestionMessage(object sender, RoutedEventArgs e)
        {
            DialogMessage.ShowDialog("Message text Message text Message text Message text Message text Message text",
                "Caption of Message block", DialogMessage.DialogMessageIcon.Question);
        }
        private void OnClickStopMessage(object sender, RoutedEventArgs e)
        {
            var aa = DialogMessage.ShowDialog("Message text Message text Message text Message text Message text Message text",
                "Caption of Message block", DialogMessage.DialogMessageIcon.Stop);
        }
        private void OnClickErrorMessage(object sender, RoutedEventArgs e)
        {
            var aa = DialogMessage.ShowDialog("Message text Message text Message text Message text Message",
                "Caption of Message block", DialogMessage.DialogMessageIcon.Error);
        }
        private async void OnClickWarningMessage(object sender, RoutedEventArgs e)
        {
            var aa = await DialogMessage.ShowAsync("Message (Show Async)", "Caption of Message block",
                DialogMessage.DialogMessageIcon.Warning, new[] { "OK", "Cancel", "Right", "Left" });

            await DialogMessage.ShowAsync($"You pressed '{aa ?? "X"}' button", null, DialogMessage.DialogMessageIcon.Info, new[] { "OK" });
        }
        private void OnClickInformationMessage(object sender, RoutedEventArgs e)
        {
            var aa = DialogMessage.ShowDialog("Message text Message text Message text Message text Message text Message text",
                "Caption of Message block", DialogMessage.DialogMessageIcon.Info, new[] { "OK" });
        }
        private void OnClickSuccessMessage(object sender, RoutedEventArgs e)
        {
            var aa = DialogMessage.ShowDialog("Message (Show) ", "Caption of Message block",
                DialogMessage.DialogMessageIcon.Success, new[] { "OK", "Cancel" }, false);

            DialogMessage.ShowDialog($"You pressed '{aa ?? "X" }' button", null, DialogMessage.DialogMessageIcon.Info, new[] { "OK" });
        }
        private void OnClickShortMessage(object sender, RoutedEventArgs e)
        {
            var aa = DialogMessage.ShowDialog("Test message 0 1 2 3 4", "Show Dialog", DialogMessage.DialogMessageIcon.Question);
        }

        private void AddWindowedContent_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Style = (Style)FindResource("HeadlessWindow"),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                // Opacity = 0
            };
            var control = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 250,
                Height = 250,
                LimitPositionToPanelBounds = false,
                ToolTip = "Width/Height=250"
            };
            window.Content = control;
            window.Show();

        }
    }
}
