using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Movable.Common;
using Movable.Controls;

namespace Movable
{
    /// <summary>
    /// Interaction logic for MainWindowAdorner.xaml
    /// </summary>
    public partial class MainWindowAdorner : Window
    {
        public MainWindowAdorner()
        {
            InitializeComponent();
        }

        private void AddAdorner_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = AdornerLayer.GetAdornerLayer(Canvas);
            if (a1 == null)
                return;
            var aa1 = a1.GetAdorners(Canvas);
            CreateAdornerCore(Canvas);
        }

        private static AdornerControl CreateAdornerCore(UIElement host)
        {
            var panel = new Grid
            {
                Background = new SolidColorBrush(Tips.StringToColor("#77777777")),
                HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch
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
                target.IsEnabled = false;
                // AllDialogClosed += (s, e) => target.IsEnabled = true;
            }
            // Added a process to remove Adorner when all dialogs are cleared
            // AllDialogClosed += (s, e) => layer.Remove(adorner);
            layer.Add(adorner);
            return adorner;
        }
    }
}
