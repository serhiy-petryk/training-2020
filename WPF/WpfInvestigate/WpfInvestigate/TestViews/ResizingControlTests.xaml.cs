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
            var adorner = CreateAdornerCore(this);
            var resizingControl3 = new ResizingControl
            {
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Margin = new Thickness(200, 200, 0, 0),
                Width = 150,
                Height = 150,
                LimitPositionToPanelBounds = true,
                ToolTip = "Width/Height=150"
            };
            resizingControl3.MouseLeftButtonDown += (s, e1) => e1.Handled = true;

            resizingControl3.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e1) =>
            {
                RemoveAdorner(this);
            }));
            ((Grid)adorner.Child).Children.Add(resizingControl3);
        }

        private static void RemoveAdorner(FrameworkElement host)
        {
            if (((host as Window)?.Content as FrameworkElement ?? host) is FrameworkElement target)
            {
                var layer = AdornerLayer.GetAdornerLayer(target);
                if (layer == null) return;

                foreach (var a in layer.GetAdorners(target) ?? new Adorner[0])
                    layer.Remove(a);
            }
        }

        private static AdornerControl CreateAdornerCore(FrameworkElement host)
        {
            if (((host as Window)?.Content as FrameworkElement ?? host) is FrameworkElement target && AdornerLayer.GetAdornerLayer(target) is AdornerLayer layer)
            {
                var panel = new Grid
                {
                    Background = new SolidColorBrush(Common.ColorSpaces.ColorUtils.StringToColor("#77777777")),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                panel.MouseLeftButtonDown += (sender, args) => RemoveAdorner(host);

                // Since there is no Adorner for the dialog, create a new one and set and return it.
                var adorner = new AdornerControl(target);
                adorner.Child = panel;

                // If Adorner is set for Window, set margin to cancel Margin of Content element.
                if (host is Window)
                {
                    adorner.Margin = new Thickness(-target.Margin.Left, -target.Margin.Top, target.Margin.Right, target.Margin.Bottom);
                    adorner.AdornerSize = AdornerControl.AdornerSizeType.Container;
                }

                // If the target is Enable when the dialog is displayed, disable it only while the dialog is displayed.
                /*if (target.IsEnabled)
                {
                    target.IsEnabled = false;
                    // AllDialogClosed += (s, e) => target.IsEnabled = true;
                }*/
                // Added a process to remove Adorner when all dialogs are cleared
                // AllDialogClosed += (s, e) => layer.Remove(adorner);
                layer.Add(adorner);
                return adorner;
            }

            return null;
        }

        private void RemovePanel_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveAdorner(this);
        }
    }
}
