using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using MyWpfMwi.Common;

namespace MyWpfMwi.Mwi
{
    public partial class MwiContainer
    {
        public static readonly DependencyProperty LeftPanelProperty = DependencyProperty.Register("LeftPanel", typeof(UIElement), typeof(MwiContainer), new FrameworkPropertyMetadata(null, OnLeftPanelPropertyChanged));
        public UIElement LeftPanel
        {
            get => (UIElement)GetValue(LeftPanelProperty);
            set => SetValue(LeftPanelProperty, value);
        }

        public void HideLeftPanel() => LeftPanelButton.IsChecked = false;

        private static void OnLeftPanelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is UIElement && d is MwiContainer)
                ((UIElement)e.NewValue).SetValue(MwiContainerProperty, d);
        }

        private void LeftPanel_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var newWidth = LeftPanelContainer.ActualWidth + e.HorizontalChange;
            if (newWidth >= 0)
                LeftPanelContainer.Width = newWidth;
        }

        private Storyboard _leftPanelAnimationOn;
        private Storyboard _leftPanelAnimationOff;
        private void LeftPanelButton_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            var startWidth = Math.Max(10, LeftPanelContainer.ActualWidth);
            if (_leftPanelAnimationOn == null)
            {
                _leftPanelAnimationOn = new Storyboard();
                _leftPanelAnimationOn.Children.Add(AnimationHelper.GetWidthAnimation(LeftPanelContainer, 0.0, startWidth, FillBehavior.Stop));
                _leftPanelAnimationOn.Children.Add(AnimationHelper.GetOpacityAnimation(LeftPanelContainer, 0.0, 1));
                _leftPanelAnimationOn.Children.Add(AnimationHelper.GetOpacityAnimation(MwiCanvas, 1.0, 0.75));

                _leftPanelAnimationOff = new Storyboard();
                _leftPanelAnimationOff.Children.Add(AnimationHelper.GetWidthAnimation(LeftPanelContainer, startWidth, 0.0, FillBehavior.Stop));
                _leftPanelAnimationOff.Children.Add(AnimationHelper.GetOpacityAnimation(LeftPanelContainer, 1.0, 0.0));
                _leftPanelAnimationOff.Children.Add(AnimationHelper.GetOpacityAnimation(MwiCanvas, 0.75, 1.0));
                _leftPanelAnimationOff.Children[0].Completed += (o, args) => LeftPanelContainer.Visibility = Visibility.Collapsed;
            }

            if (button.IsChecked == true)
            {
                LeftPanelContainer.Visibility = Visibility.Visible;
                _leftPanelAnimationOn.Begin();
            }
            else
            {
                ((DoubleAnimation)_leftPanelAnimationOn.Children[0]).To = LeftPanelContainer.ActualWidth;
                ((DoubleAnimation)_leftPanelAnimationOff.Children[0]).From = LeftPanelContainer.ActualWidth;
                _leftPanelAnimationOff.Begin();
            }
        }
    }
}
