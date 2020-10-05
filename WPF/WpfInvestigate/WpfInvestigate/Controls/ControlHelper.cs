using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public static class ControlHelper
    {
        public static void HideBorderOfTextBox(DatePickerTextBox textBox)
        {
            // Hide border of DatePicker textbox
            var borderNames = new[] { "watermark_decorator", "ContentElement" };
            var borders = Tips.GetVisualChildren(textBox).OfType<Border>().Where(c => borderNames.Contains(c.Name));
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
