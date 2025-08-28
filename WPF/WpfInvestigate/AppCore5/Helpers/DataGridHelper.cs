using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppCore5.Helpers
{
    public static class DataGridHelper
    {
        private static readonly EventSetter MouseEnterEventSetter = new EventSetter
        {
            Event = UIElement.MouseEnterEvent,
            Handler = new MouseEventHandler(OnDataGridCellMouseEnter)
        };

        public static void AddCellToolTipWhenTrimming(this DataGrid dataGrid)
        {
            var style = dataGrid.CellStyle.StyleClone() ?? new Style(typeof(DataGridCell));
            style.Setters.Add(MouseEnterEventSetter);
            style.Seal();

            dataGrid.CellStyle = style;
        }

        private static void OnDataGridCellMouseEnter(object sender, MouseEventArgs e)
        {
            var cell = (DataGridCell)sender;
            var textBlock = cell.Content as TextBlock;
            if (textBlock == null) return;

            if (!string.IsNullOrEmpty(textBlock.Text) && IsTextTrimmed(textBlock))
                ToolTipService.SetToolTip(cell, textBlock.Text);
            else
                ToolTipService.SetToolTip(cell, null);
        }

        public static bool IsTextTrimmed(TextBlock textBlock)
        {
            const double delta = 0.00001d;
            if (textBlock.TextTrimming == TextTrimming.None)
                return false;
            if (textBlock.TextWrapping == TextWrapping.NoWrap)
            {
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var desiredWidth = textBlock.DesiredSize.Width;
                var actualWidth = textBlock.ActualWidth + textBlock.Margin.Left + textBlock.Margin.Right + delta;
                return desiredWidth > actualWidth;
            }
            else
            {
                textBlock.Measure(new Size(
                    textBlock.ActualWidth + textBlock.Margin.Left + textBlock.Margin.Right + 0.0001d,
                    double.PositiveInfinity));
                var desiredHeight1 = textBlock.DesiredSize.Height;
                var actualHeight1 = textBlock.ActualHeight + textBlock.Margin.Top + textBlock.Margin.Bottom + delta;
                return desiredHeight1 > actualHeight1;
            }
        }

        public static void SetTextTrimming(this DataGrid dataGrid)
        {
            var styles = dataGrid.Columns.Where(a => a is DataGridTextColumn)
                .Select(a => ((DataGridTextColumn)a).ElementStyle)
                .Where(a => a.Setters.FirstOrDefault(b =>
                    b is Setter setter && setter.Property == TextBlock.TextTrimmingProperty) == null).Distinct()
                .ToDictionary(a => a, a => a.StyleClone());

            foreach (var style in styles.Values)
            {
                style.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
                style.Seal();
            }

            foreach (var dataGridColumn in dataGrid.Columns.Where(c => c is DataGridTextColumn))
            {
                var c = (DataGridTextColumn)dataGridColumn;
                var style = c.ElementStyle;
                if (styles.ContainsKey(style))
                    c.ElementStyle = styles[style];
            }

        }
    }
}
