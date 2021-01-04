using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
                { Content = new ResizableSample(), Margin = new Thickness(20, 10, 0, 0) };
            GridPanel.Children.Add(resizingControl2);

            var resizingControl3 = new ResizingControl
            {
                Content = new ResizableSample{Width = double.NaN, Height = double.NaN}, Margin = new Thickness(200, 200, 0, 0),
                Width = 150, Height = 150, LimitPositionToPanelBounds = true
            };
            resizingControl3.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) => GridPanel.Children.Remove(resizingControl3)));
            GridPanel.Children.Add(resizingControl3);

        }

        private void AddPanel_OnClick(object sender, RoutedEventArgs e)
        {
            CreateAdornerCore(this);
        }

        private static AdornerControl CreateAdornerCore(UIElement host)
        {
            var panel = new Grid
            {
                Background = new SolidColorBrush(Common.ColorSpaces.ColorUtils.StringToColor("#77777777")),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // If it is a Window class, use the Content property.
            var win = host as Window;
            var target = win?.Content as UIElement ?? host;
            if (target == null)
                return null;

            var layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null)
                return null;

            // Since there is no Adorner for the dialog, create a new one and set and return it.
            var adorner = new AdornerControl(target);
            adorner.Child = panel;

            // If Adorner is set for Window, set margin to cancel Margin of Content element.
            if (win != null)
            {
                var content = win.Content as FrameworkElement;
                var margin = content.Margin;
                adorner.Margin = new Thickness(-margin.Left, -margin.Top, margin.Right, margin.Bottom);
                adorner.AdornerSize = AdornerControl.AdornerSizeType.Container;
            }

            // If the target is Enable when the dialog is displayed, disable it only while the dialog is displayed.
            if (target.IsEnabled)
            {
                // target.IsEnabled = false;
                // AllDialogClosed += (s, e) => target.IsEnabled = true;
            }
            // Added a process to remove Adorner when all dialogs are cleared
            // AllDialogClosed += (s, e) => layer.Remove(adorner);
            layer.Add(adorner);
            return adorner;
        }

    }
}
