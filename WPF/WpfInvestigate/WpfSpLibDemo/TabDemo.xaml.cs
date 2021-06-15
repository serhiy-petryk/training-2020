using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            /*var current = sender as FrameworkElement;
            var target = GetPreviousElement(current);
            var oldValue = DatePickerEffect.GetClearButton(target);
            DatePickerEffect.SetClearButton(target, !oldValue);*/
            var checkBox = sender as CheckBox;
            var target = GetPreviousElement(checkBox);
            DatePickerEffect.SetClearButton(target, checkBox.IsChecked.Value);
        }
        private void ToggleVisibility(object sender, RoutedEventArgs e)
        {
            /*var current = sender as FrameworkElement;
            var target = GetPreviousElement(current);
            var a1 = Equals(target.Tag, "Hidden") ? Visibility.Hidden : Visibility.Collapsed;
            target.Visibility = target.Visibility == Visibility.Visible ? a1 : Visibility.Visible;*/
            var checkBox = sender as CheckBox;
            var target = GetPreviousElement(checkBox);
            var a1 = Equals(target.Tag, "Hidden") ? Visibility.Hidden : Visibility.Collapsed;
            target.Visibility = Equals(checkBox.IsChecked, true) ? a1 : Visibility.Visible;
        }
        private FrameworkElement GetPreviousElement(FrameworkElement current)
        {
            var grid = current.Parent as Grid;
            var row = Grid.GetRow(current);
            return grid.Children.OfType<FrameworkElement>().FirstOrDefault(a => Grid.GetColumn(a) == 1 && Grid.GetRow(a) == row);
        }
    }
}
