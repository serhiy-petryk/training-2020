using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public partial class MwiContainer
    {
        public static readonly DependencyProperty LeftPanelProperty = DependencyProperty.Register("LeftPanel", typeof(FrameworkElement), typeof(MwiContainer), new FrameworkPropertyMetadata(null, LeftPanel_OnPropertyChanged));

        private Grid LeftPanelContainer;
        private ToggleButton LeftPanelButton;

        public FrameworkElement LeftPanel
        {
            get => (FrameworkElement)GetValue(LeftPanelProperty);
            set => SetValue(LeftPanelProperty, value);
        }
        private static void LeftPanel_OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public void HideLeftPanel()
        {
            if (LeftPanelButton != null)
                LeftPanelButton.IsChecked = false;
        }

        private void LeftPanel_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var newWidth = LeftPanelContainer.ActualWidth + e.HorizontalChange;
            if (newWidth >= 0)
                LeftPanelContainer.Width = newWidth;
            e.Handled = true;
        }

        private async void LeftPanelButton_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            if (button.IsChecked == true)
            {
                LeftPanelContainer.Visibility = Visibility.Visible;
                LeftPanelContainer.BeginAnimation(WidthProperty, new DoubleAnimation(0, LeftPanelContainer.ActualWidth, AnimationHelper.AnimationDuration, FillBehavior.Stop));
                LeftPanelContainer.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, AnimationHelper.AnimationDuration));
                MwiPanel.BeginAnimation(OpacityProperty, new DoubleAnimation(1.0, 0.75, AnimationHelper.AnimationDuration));
            }
            else
            {
                var lastLeftPanelWidth = LeftPanelContainer.ActualWidth;
                await Task.WhenAll(
                    LeftPanelContainer.BeginAnimationAsync(WidthProperty, LeftPanelContainer.ActualWidth, 0.0),
                    LeftPanelContainer.BeginAnimationAsync(OpacityProperty,  0.0),
                    MwiPanel.BeginAnimationAsync(OpacityProperty, 1.0));

                LeftPanelContainer.Visibility = Visibility.Hidden;
                LeftPanelContainer.Width = lastLeftPanelWidth;
            }
        }

    }
}
