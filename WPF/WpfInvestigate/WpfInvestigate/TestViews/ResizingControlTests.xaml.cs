﻿using System.Diagnostics;
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
            resizingControl3.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) => GridPanel.Children.Remove(resizingControl3)));
            GridPanel.Children.Add(resizingControl3);
        }

        private void AddPanel_OnClick(object sender, RoutedEventArgs e)
        {
            var content = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 150,
                Height = 150,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=150"
            };
            new DialogAdorner(CanvasPanel) {CloseOnClickBackground = true}.ShowContent(content);
        }

        private void OnClickDialogMessageBlock(object sender, RoutedEventArgs e)
        {
            var aa = MessageBlock.ShowDialog("Message text Message text Message text Message text Message text Message text ",
                "Show Dialog", null, new[] { "OK", "Cancel", "Right", "Left" });
        }

        private void AddWindowPanelSync_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new DialogAdorner { CloseOnClickBackground = true };
            a1.ContentClosed += (o, element) =>
            {
                if (element.Name == "Test")
                {
                    var content3 = new ResizingControl
                    {
                        Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                        Width = 250,
                        Height = 250,
                        LimitPositionToPanelBounds = true,
                        ToolTip = "Width/Height=250"
                    };
                    ((DialogAdorner)o).ShowContent(content3);
                }
            };

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
                ToolTip = "Width/Height=250"
            };
            await a1.ShowContentAsync(content1);

            var content2 = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 150,
                Height = 150,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=150"
            };
            await a1.ShowContentAsync(content2);
            await a1.WaitUntilClosed();

            var content3 = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 200,
                Height = 250,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=200/250"
            };

            a1.ShowContent(content1);
            await a1.ShowContentAsync(content2);
            await a1.ShowContentAsync(content3);
            await a1.WaitUntilClosed();

            Debug.Print($"AddWindowPanelAsync_OnClick method finished");
        }

        private async void AddMessageContent_OnClick(object sender, RoutedEventArgs e)
        {
            var message = MessageContent.CreateMessageContent("Test message", "Caption",
                MessageContent.MessageContentIcon.Question, new[] {"OK", "Cancel", "Right", "Left"});
            var content = new ResizingControl
            {
                Content = message,
                LimitPositionToPanelBounds = true
            };
            var adorner = new DialogAdorner(CanvasPanel) {CloseOnClickBackground = true};
            adorner.ShowContent(content);
            await adorner.WaitUntilClosed();
            Debug.Print($"Message: {message.Result}");
        }

    }
}
