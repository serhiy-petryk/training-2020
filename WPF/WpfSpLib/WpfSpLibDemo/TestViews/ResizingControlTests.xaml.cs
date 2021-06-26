using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfSpLib.Controls;
using WpfSpLib.Helpers;
using WpfSpLibDemo.Samples;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for ResizingControl.xaml
    /// </summary>
    public partial class ResizingControlTests : Window
    {
        public ResizingControlTests()
        {
            InitializeComponent();

            var resizingControl = new ResizingControl {Content = new ResizableContentTemplateSample{Content = "Content"}, Position = new Point(110, 110)};
            GridPanel.Children.Add(resizingControl);

            var resizingControl2 = new ResizingControl {Content = new ResizableSample(), Position = new Point(5, 390), ToolTip = "No Width/Height"};
            GridPanel.Children.Add(resizingControl2);

            var resizingControl3 = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Position = new Point(250,140),
                Width = 150,
                Height = 150,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=150"
            };
            resizingControl3.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) => GridPanel.Children.Remove(resizingControl3)));
            GridPanel.Children.Add(resizingControl3);
        }

        public async Task AutomateAsync(int numberOfTestSteps)
        {
            for (var k = 0; k < numberOfTestSteps; k++)
                await Automate_Step(k);
        }
        private async void Automate_OnClick(object sender, RoutedEventArgs e)
        {
            for (var k = 0; k < 5; k++)
                await Automate_Step(k);
        }

        private async Task Automate_Step(int step)
        {
            var control = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 150,
                Height = 150,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=150"
            };
            control.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e1) => GridPanel.Children.Remove(control)));
            GridPanel.Children.Add(control);
            ControlHelper.SetFocus(control);

            await Task.Delay(1000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var a11 = GC.GetTotalMemory(true);

            // control.CommandBindings[0].Command.Execute(null);
            GridPanel.Children.Remove(control);

            await Task.Delay(1000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var a12 = GC.GetTotalMemory(true);

            Debug.Print($"Test{step}: {a12:N0}");
        }

        private void AddContent_OnClick(object sender, RoutedEventArgs e)
        {
            var control = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 150,
                Height = 150,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=150"
            };
            control.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e1) => GridPanel.Children.Remove(control)));
            GridPanel.Children.Add(control);
            ControlHelper.SetFocus(control);
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

            a1.ShowContent(content3);
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
                DialogMessage.DialogMessageIcon.Question, new[] { "OK", "Cancel", "Right", "Left" });
            Debug.Print($"MessageAsync: {result}");
        }

        private void MessageDialog_OnClick(object sender, RoutedEventArgs e)
        {
            var result = DialogMessage.ShowDialog("Test message", "Caption",
                DialogMessage.DialogMessageIcon.Question, new[] { "OK", "Cancel", "Right", "Left" });
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
                Style = FindResource("HeadlessWindow") as Style,
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
            control.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e1) => ((Window)control.Parent).Close()));
            window.Content = control;
            window.Show();

        }
    }
}
