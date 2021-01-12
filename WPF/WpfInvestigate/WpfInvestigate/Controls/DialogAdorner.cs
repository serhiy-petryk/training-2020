using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfInvestigate.Common;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.Controls
{

    public class DialogAdorner : AdornerControl
    {
        private static FrameworkElement GetAdornedElement(FrameworkElement host)
        {
            if (host == null)
                host = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            host = (host as Window)?.Content as FrameworkElement ?? host;

            if (host == null)
                throw new Exception("AdornerControl error! AdornedElement can't be null");
            return host;
        }

        private static CommandBinding _closeCommand = new CommandBinding(ApplicationCommands.Close, (s, e1) =>
        {
            RemoveAdorner(Tips.GetVisualParents(s as UIElement).OfType<DialogAdorner>().FirstOrDefault());
        });

        //===============================
        public bool CloseOnClickBackground { get; set; } = true;

        private Brush _background = new SolidColorBrush(Color.FromArgb(0x77, 0x77, 0x77, 0x77));
        public Brush Background
        {
            get => _background;
            set
            {
                _background = value;
                if (Child is Panel panel)
                    panel.Background = value;
            }
        }

        public Storyboard OpenPanelAnimation { get; set; } = Application.Current.FindResource("FadeInAnimation") as Storyboard;
        public Storyboard ClosePanelAnimation { get; set; } = Application.Current.FindResource("FadeOutAnimation") as Storyboard;
        public Storyboard OpenContentAnimation { get; set; }// = Application.Current.FindResource("FadeInAnimation") as Storyboard;
        public Storyboard CloseContentAnimation { get; set; } = Application.Current.FindResource("FadeOutAnimation") as Storyboard;

        private FrameworkElement _host;

        public DialogAdorner(FrameworkElement content, FrameworkElement host = null) : base(GetAdornedElement(host))
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            _host = host ?? Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            var layer = AdornerLayer.GetAdornerLayer(AdornedElement);
            if (layer == null)
                throw new Exception("DialogAdorner constructor error! AdornerLevel of host can't be null");

            layer.Add(this);

            var panel = new Grid
            {
                Background = _background,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            panel.MouseLeftButtonDown += Panel_MouseLeftButtonDown;
            panel.Children.Add(content);

            Child = panel;

            if (_host is Window)
            {
                var hostMargin = (AdornedElement as FrameworkElement).Margin;
                Margin = new Thickness(-hostMargin.Left, -hostMargin.Top, hostMargin.Left, hostMargin.Top);
                AdornerSize = AdornerSizeType.Container;
            }

            content.CommandBindings.Add(_closeCommand);

            //============  Animation  ==============
            OpenPanelAnimation?.BeginAsync(panel);

            content.Visibility = Visibility.Hidden;
            content.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                var left = Math.Max(0, (panel.ActualWidth - content.ActualWidth) / 2);
                var top = Math.Max(0, (panel.ActualHeight - content.ActualHeight) / 2);
                content.Margin = new Thickness(left, top, 0, 0);

                if (OpenContentAnimation == null)
                {
                    var contentAnimation = new ThicknessAnimation(new Thickness(left, 0, 0, 0), new Thickness(left, top, 0, 0), AnimationHelper.AnimationDurationSlow);
                    contentAnimation.FillBehavior = FillBehavior.Stop;
                    content.BeginAnimation(MarginProperty, contentAnimation);
                }
                else
                    OpenContentAnimation.BeginAsync(content);

                content.Visibility = Visibility.Visible;
            }));


        }

        private static void Panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var adorner = Tips.GetVisualParents((FrameworkElement)sender).OfType<DialogAdorner>().FirstOrDefault();
            if (adorner != null && adorner.CloseOnClickBackground)
            {
                foreach (var content in (adorner.Child as Grid).Children.OfType<FrameworkElement>().Where(c=>c.Visibility == Visibility.Visible))
                {
                    var mousePoint = e.GetPosition(content);
                    var isUnderContent = new Rect(0, 0, content.ActualWidth, content.ActualHeight).Contains(mousePoint);
                    if (isUnderContent) return;
                }

                // _closeCommand.Command.Execute(adorner.AdornedElement);
                RemoveAdorner(adorner);
            }
        }

        private static async void RemoveAdorner(DialogAdorner adorner)
        {
            if (adorner?.Child is Grid panel)
            {
                panel.MouseLeftButtonDown -= Panel_MouseLeftButtonDown;
                if (adorner.CloseContentAnimation != null)
                {
                    foreach (var content in panel.Children.OfType<FrameworkElement>())
                        await adorner.CloseContentAnimation?.BeginAsync(content);
                }
                panel.Children.RemoveRange(0, panel.Children.Count);
                if (adorner.ClosePanelAnimation != null) await adorner.ClosePanelAnimation?.BeginAsync(panel);

                AdornerLayer.GetAdornerLayer(adorner.AdornedElement)?.Remove(adorner);
            }
        }
    }
}
