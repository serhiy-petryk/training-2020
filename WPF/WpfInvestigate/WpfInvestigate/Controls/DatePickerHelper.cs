using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public static class DatePickerHelper
    {
        public static void HideBorderOfTextBox(FrameworkElement picker)
        {
            var borderNames = new[] { "watermark_decorator", "ContentElement" };
            var textBox = picker is DatePickerTextBox
                ? picker as DatePickerTextBox
                : Tips.GetVisualChildren(picker).OfType<DatePickerTextBox>().FirstOrDefault();

            //  Remove textbox background
            if (textBox != null && textBox.Background != Brushes.Transparent)
                textBox.Background = Brushes.Transparent;

            // Hide border of DatePicker textbox
            var borders = Tips.GetVisualChildren(picker).OfType<Border>().Where(c => borderNames.Contains(c.Name));
            foreach (var x in borders)
                x.BorderBrush = Brushes.Transparent;

        }
    }
}
