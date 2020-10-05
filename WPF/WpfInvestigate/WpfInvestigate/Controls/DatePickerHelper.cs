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
        public static void HideBorderOfTextBox(DatePickerTextBox textBox)
        {
            // Hide border of DatePicker textbox
            var borderNames = new[] { "watermark_decorator", "ContentElement" };
            var borders = Tips.GetVisualChildren(textBox).OfType<Border>().Where(c => borderNames.Contains(c.Name));
            foreach (var x in borders)
                x.BorderBrush = Brushes.Transparent;

        }
    }
}
