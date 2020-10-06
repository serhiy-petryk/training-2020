using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls.Helpers
{
    public static class ControlHelper
    {
        public static IEnumerable<Border> GetMainBorders(FrameworkElement element) =>
            Tips.GetVisualChildren(element).OfType<Border>().Where(b =>
                Math.Abs(b.ActualWidth - element.ActualWidth) < 1.1 &&
                Math.Abs(b.ActualHeight - element.ActualHeight) < 1.1);

        public static CornerRadius? GetCornerRadius(FrameworkElement element) => 
            GetMainBorders(element).FirstOrDefault(b => b.CornerRadius != new CornerRadius())?.CornerRadius;

        public static void HideBorderOfDatePickerTextBox(DatePickerTextBox textBox)
        {
            // Hide border of DatePicker textbox
            const string name1 = "watermark_decorator", name2 = "ContentElement";
            var borders = Tips.GetVisualChildren(textBox).OfType<Border>().Where(c => c.Name == name1 || c.Name == name2);
            foreach (var x in borders)
                x.BorderBrush = Brushes.Transparent;
        }

        public static void SetBorderOfToolbarComboBoxes(ToolBar toolBar)
        {
            // Set border of comboboxes inside of toolbar (default is white)
            foreach (var comboBox in toolBar.Items.OfType<ComboBox>())
            {
                var toggleButton = Tips.GetVisualChildren(comboBox).OfType<ToggleButton>().FirstOrDefault();
                if (toggleButton != null)
                {
                    var b = new Binding("BorderBrush")
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ComboBox), 1)
                    };
                    toggleButton.SetBinding(Control.BorderBrushProperty, b);
                    // toggleButton.BorderBrush = new SolidColorBrush(Common.ColorSpaces.ColorUtils.StringToColor("#FF0069D6"));
                }
            }
        }
    }
}
