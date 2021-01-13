﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfInvestigate.Common;

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

        private static CommandBinding _closeCommand = new CommandBinding(ApplicationCommands.Close, async (s, e1) =>
        {
            var content = s as FrameworkElement;
            var adorner = Tips.GetVisualParents(content).OfType<DialogAdorner>().FirstOrDefault();
            if (adorner == null || content == null) return;

            if (adorner.CloseContentAnimation != null)
                await adorner.CloseContentAnimation?.BeginAsync(content);

            adorner.Panel.Children.Remove(content);
            if (adorner.Panel.Children.Count == 0)
                RemoveAdorner(adorner);
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

        public Grid Panel => Child as Grid;

        public Storyboard OpenPanelAnimation { get; set; } = Application.Current.FindResource("FadeInAnimation") as Storyboard;
        public Storyboard ClosePanelAnimation { get; set; } = Application.Current.FindResource("FadeOutAnimation") as Storyboard;
        public Storyboard OpenContentAnimation { get; set; }// = Application.Current.FindResource("FadeInAnimation") as Storyboard;
        public Storyboard CloseContentAnimation { get; set; } = Application.Current.FindResource("FadeOutAnimation") as Storyboard;

        private FrameworkElement _host;

        public DialogAdorner(FrameworkElement host = null) : base(GetAdornedElement(host))
        {
            _host = host ?? Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (AdornerLayer == null)
                throw new Exception("DialogAdorner constructor error! AdornerLevel can't be null");

            AdornerLayer.Add(this);

            if (_host is Window)
            {
                var hostMargin = (AdornedElement as FrameworkElement).Margin;
                Margin = new Thickness(-hostMargin.Left, -hostMargin.Top, hostMargin.Left, hostMargin.Top);
                AdornerSize = AdornerSizeType.Container;
            }

            Child = new Grid
            {
                Background = _background,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Panel.MouseLeftButtonDown += Panel_MouseLeftButtonDown;
            OpenPanelAnimation?.Begin(Panel);
        }

        public async Task XShowContentAsync(FrameworkElement content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            Panel.Children.Add(content);

            content.CommandBindings.Add(_closeCommand);
            content.Visibility = Visibility.Hidden;

            await content.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render).Task;

            var left = Math.Max(0, (Child.ActualWidth - content.ActualWidth) / 2);
            var top = Math.Max(0, (Child.ActualHeight - content.ActualHeight) / 2);
            content.Margin = new Thickness(left, top, 0, 0);
            content.Visibility = Visibility.Visible;

            if (OpenContentAnimation == null)
            {
                var contentAnimation = new ThicknessAnimation(new Thickness(left, 0, 0, 0), new Thickness(left, top, 0, 0), AnimationHelper.AnimationDurationSlow);
                contentAnimation.FillBehavior = FillBehavior.Stop;
                contentAnimation.Freeze();
                await content.BeginAnimationAsync(MarginProperty, contentAnimation);
            }
            else
                OpenContentAnimation.Begin(content);
        }

        public async Task ShowContentAsync(FrameworkElement content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            Panel.Children.Add(content);

            content.CommandBindings.Add(_closeCommand);
            content.Visibility = Visibility.Hidden;

            await content.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render).Task;

            var left = Math.Max(0, (Child.ActualWidth - content.ActualWidth) / 2);
            var top = Math.Max(0, (Child.ActualHeight - content.ActualHeight) / 2);
            content.Margin = new Thickness(left, top, 0, 0);
            content.Visibility = Visibility.Visible;

            if (OpenContentAnimation == null)
            {
                var contentAnimation = new ThicknessAnimation(new Thickness(left, 0, 0, 0), new Thickness(left, top, 0, 0), AnimationHelper.AnimationDurationSlow);
                contentAnimation.FillBehavior = FillBehavior.Stop;
                contentAnimation.Freeze();
                await content.BeginAnimationAsync(MarginProperty, contentAnimation);
            }
            else
                OpenContentAnimation.Begin(content);
        }

        public async void ShowContent(FrameworkElement content) => await ShowContentAsync(content);

        private static void Panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var adorner = Tips.GetVisualParents((FrameworkElement)sender).OfType<DialogAdorner>().FirstOrDefault();
            if (adorner != null && adorner.CloseOnClickBackground)
            {
                foreach (var content in adorner.Panel.Children.OfType<FrameworkElement>().Where(c=>c.Visibility == Visibility.Visible))
                {
                    var mousePoint = e.GetPosition(content);
                    var isUnderContent = new Rect(0, 0, content.ActualWidth, content.ActualHeight).Contains(mousePoint);
                    if (isUnderContent) return;
                }

                RemoveAdorner(adorner);
            }
        }

        private static async void RemoveAdorner(DialogAdorner adorner)
        {
            adorner.Panel.MouseLeftButtonDown -= Panel_MouseLeftButtonDown;

            var tasks = new List<Task>();
            if (adorner.CloseContentAnimation != null)
                foreach (var content in adorner.Panel.Children.OfType<FrameworkElement>())
                    tasks.Add(adorner.CloseContentAnimation?.BeginAsync(content));
            await Task.WhenAll(tasks.ToArray());

            adorner.Panel.Children.RemoveRange(0, adorner.Panel.Children.Count);
            if (adorner.ClosePanelAnimation != null)
                await adorner.ClosePanelAnimation?.BeginAsync(adorner.Panel);

            adorner.AdornerLayer?.Remove(adorner);
        }
    }
}
