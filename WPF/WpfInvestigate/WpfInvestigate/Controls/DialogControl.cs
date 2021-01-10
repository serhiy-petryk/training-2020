using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{

    public class DialogAdorner : AdornerControl
    {
        //===============================
        private static FrameworkElement GetAdornedElement(FrameworkElement host)
        {
            if (host == null)
                host = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            host = (host as Window)?.Content as FrameworkElement ?? host;

            if (host == null)
                throw new Exception("AdornerControl error! AdornedElement can't be null");
            return host;
        }

        //===============================
        public bool CloseOnClickBackground { get; set; } = true;

        private FrameworkElement _host;

        public DialogAdorner(FrameworkElement content, FrameworkElement host = null) : base(GetAdornedElement(host))
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            _host = host ?? Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            var layer = AdornerLayer.GetAdornerLayer(AdornedElement);
            if (layer == null)
                throw new Exception("DialogAdorner constructor error! AdornerLevel of host can't be null");

            var panel = new Grid
            {
                Background = new SolidColorBrush(Common.ColorSpaces.ColorUtils.StringToColor("#77777777")),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            panel.MouseLeftButtonDown += Panel_MouseLeftButtonDown;

            // Since there is no Adorner for the dialog, create a new one and set and return it.
            Child = panel;
            var aa1 = Tips.GetVisualParents(AdornedElement);

            // If Adorner is set for Window, set margin to cancel Margin of Content element.
            if (_host is Window)
            {
                var hostMargin = (AdornedElement as FrameworkElement).Margin;
                Margin = new Thickness(-hostMargin.Left, -hostMargin.Top, hostMargin.Right, hostMargin.Bottom);
                AdornerSize = AdornerSizeType.Container;
            }

            // If the target is Enable when the dialog is displayed, disable it only while the dialog is displayed.
            /*if (target.IsEnabled)
            {
                target.IsEnabled = false;
                // AllDialogClosed += (s, e) => target.IsEnabled = true;
            }*/
            // Added a process to remove Adorner when all dialogs are cleared
            // AllDialogClosed += (s, e) => layer.Remove(adorner);
            layer.Add(this);

            //==========================
            content.MouseLeftButtonDown += (s, e1) => e1.Handled = true;
            content.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e1) => RemoveAdorner(_host)));

            panel.Children.Add(content);

            content.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                content.Margin = new Thickness
                {
                    Left = Math.Max(0, (panel.ActualWidth - content.ActualWidth) / 2),
                    Top = Math.Max(0, (panel.ActualHeight - content.ActualHeight) / 2)
                };
            }));


        }

        private static void Panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var adorner = Tips.GetVisualParents((FrameworkElement)sender).OfType<DialogAdorner>().FirstOrDefault();
            if (adorner?.AdornedElement is FrameworkElement target)
                RemoveAdorner(target);
        }

        private static void RemoveAdorner(FrameworkElement host)
        {
            if (((host as Window)?.Content as FrameworkElement ?? host) is FrameworkElement target &&
                AdornerLayer.GetAdornerLayer(target) is AdornerLayer layer)
            {
                foreach (var a in (layer.GetAdorners(target) ?? new Adorner[0]).ToArray().OfType<AdornerControl>())
                {
                    if (a.Child is Grid panel)
                    {
                        panel.Children.RemoveRange(0, panel.Children.Count);
                        panel.MouseLeftButtonDown -= Panel_MouseLeftButtonDown;
                    }

                    layer.Remove(a);
                }
            }
        }

    }

    public class DialogControl
    {
        private FrameworkElement _host;
        private AdornerControl _adorner;
        public bool CloseOnClickBackground { get; set; } = true;
        // private FrameworkElement _content => this;
        public DialogControl(FrameworkElement content, FrameworkElement host = null)// : base(host)
        {
            // Content = content;

            if (host == null)
                host = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            _host = (host as Window)?.Content as FrameworkElement ?? host;

            var layer = AdornerLayer.GetAdornerLayer(_host);

            if (content == null)
                throw new ArgumentNullException(nameof(content));
            if (host == null || layer == null)
                throw new Exception("DialogControl constructor error! Host or AdornerLevel of host is null");

            var panel = new Grid
            {
                Background = new SolidColorBrush(Common.ColorSpaces.ColorUtils.StringToColor("#77777777")),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            panel.MouseLeftButtonDown += Panel_MouseLeftButtonDown;

            // Since there is no Adorner for the dialog, create a new one and set and return it.
            _adorner = new AdornerControl(_host);
            _adorner.Child = panel;

            // If Adorner is set for Window, set margin to cancel Margin of Content element.
            if (host is Window)
            {
                _adorner.Margin = new Thickness(-_host.Margin.Left, -_host.Margin.Top, host.Margin.Right, host.Margin.Bottom);
                _adorner.AdornerSize = AdornerControl.AdornerSizeType.Container;
            }

            // If the target is Enable when the dialog is displayed, disable it only while the dialog is displayed.
            /*if (target.IsEnabled)
            {
                target.IsEnabled = false;
                // AllDialogClosed += (s, e) => target.IsEnabled = true;
            }*/
            // Added a process to remove Adorner when all dialogs are cleared
            // AllDialogClosed += (s, e) => layer.Remove(adorner);
            layer.Add(_adorner);

            //==========================
            content.MouseLeftButtonDown += (s, e1) => e1.Handled = true;
            content.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e1) => RemoveAdorner(_host)));

            panel.Children.Add(content);

            content.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                content.Margin = new Thickness
                {
                    Left = Math.Max(0, (panel.ActualWidth - content.ActualWidth) / 2),
                    Top = Math.Max(0, (panel.ActualHeight - content.ActualHeight) / 2)
                };
            }));

        }

        private static void Panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var adorner = Tips.GetVisualParents((FrameworkElement)sender).OfType<AdornerControl>().FirstOrDefault();
            if (adorner?.AdornedElement is FrameworkElement target)
                RemoveAdorner(target);
        }

        private static void RemoveAdorner(FrameworkElement host)
        {
            if (((host as Window)?.Content as FrameworkElement ?? host) is FrameworkElement target &&
                AdornerLayer.GetAdornerLayer(target) is AdornerLayer layer)
            {
                foreach (var a in (layer.GetAdorners(target) ?? new Adorner[0]).ToArray().OfType<AdornerControl>())
                {
                    if (a.Child is Grid panel)
                    {
                        panel.Children.RemoveRange(0, panel.Children.Count);
                        panel.MouseLeftButtonDown -= Panel_MouseLeftButtonDown;
                    }

                    layer.Remove(a);
                }
            }
        }
    }
}
