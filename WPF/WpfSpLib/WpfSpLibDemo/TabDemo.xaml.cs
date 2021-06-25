using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using WpfSpLib.Common;
using WpfSpLib.Effects;

namespace WpfSpLibDemo
{
    /// <summary>
    /// Interaction logic for TabControlTests.xaml
    /// </summary>
    public partial class TabDemo : Window
    {
        public TabDemo()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            foreach (var btn in this.GetVisualChildren().OfType<ButtonBase>())
            {
                btn.Click -= ClearCombobox_OnClick;
                btn.Click -= DatePicker_ToggleVisibility;
                btn.Click -= DatePicker_ToggleClearButton;
                btn.Click -= DatePicker_ToggleIsNullbale;
                btn.Click -= DatePicker_ToggleHideInnerBorder;
                btn.Click -= DatePicker_ClearBackground;
                btn.Click -= OnControlChangeSizeClick;
                btn.Click -= ChangeWatermarkText_OnClick;
                btn.Click -= ChangeWatermarkForeground_OnClick;
                btn.Click -= FocusVisual_AlwaysShowFocus;
            }
        }

        private void ClearCombobox_OnClick(object sender, RoutedEventArgs e)
        {
            var cb = GetPreviousElement((FrameworkElement)sender) as ComboBox;
            cb.SelectedIndex = -1;
        }

        private void ChangeWatermarkText_OnClick(object sender, RoutedEventArgs e)
        {
            var element = GetPreviousElement((FrameworkElement)sender);
            var a = element.GetValue(WatermarkEffect.WatermarkProperty) as string;
            element.SetValue(WatermarkEffect.WatermarkProperty, a + a?.Length);
        }

        private void ChangeWatermarkForeground_OnClick(object sender, RoutedEventArgs e)
        {
            var element = GetPreviousElement((FrameworkElement)sender);
            var a = element.GetValue(WatermarkEffect.ForegroundProperty) as Brush;
            if (a != null && a is SolidColorBrush && ((SolidColorBrush)a).Color == Colors.Red)
                element.SetValue(WatermarkEffect.ForegroundProperty, Brushes.Green);
            else
                element.SetValue(WatermarkEffect.ForegroundProperty, Brushes.Red);
        }

        private void DatePicker_ToggleClearButton(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var target = GetPreviousElement(checkBox);
            DatePickerEffect.SetClearButton(target, checkBox.IsChecked.Value);
        }
        private void DatePicker_ToggleVisibility(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var target = GetPreviousElement(checkBox);
            var a1 = Equals(target.Tag, "Hidden") ? Visibility.Hidden : Visibility.Collapsed;
            target.Visibility = Equals(checkBox.IsChecked, true) ? a1 : Visibility.Visible;
        }
        private void DatePicker_ToggleIsNullbale(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var target = GetPreviousElement(checkBox);
            DatePickerEffect.SetIsNullable(target, checkBox.IsChecked.Value);
        }
        private void DatePicker_ToggleHideInnerBorder(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var target = GetPreviousElement(checkBox);
            DatePickerEffect.SetHideInnerBorder(target, checkBox.IsChecked.Value);
        }
        private void DatePicker_ClearBackground(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var target = GetPreviousElement(checkBox) as Control;
            if (checkBox.IsChecked.Value)
            {
                target.Background = Brushes.Transparent;
                target.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF333333");
            }
            else
            {
                target.Background = Brushes.GreenYellow;
                target.Foreground = Brushes.Blue;
            }
        }
        private void OnControlChangeSizeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var target = GetPreviousElement(button) as Control;
            target.Width = target.ActualWidth * 1.05;
            target.Height = target.ActualHeight * 1.05;
        }

        private FrameworkElement GetPreviousElement(FrameworkElement current)
        {
            var grid = current.Parent as Grid;
            var row = Grid.GetRow(current);
            return grid.Children.OfType<FrameworkElement>().FirstOrDefault(a => Grid.GetColumn(a) == 1 && Grid.GetRow(a) == row);
        }

        private void FocusVisual_AlwaysShowFocus(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var target = GetPreviousElement(checkBox);
            FocusVisualEffect.SetAlwaysShowFocus(target, checkBox.IsChecked.Value);
        }
    }
}
