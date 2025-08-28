using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AppCore5.Helpers
{
    public static class DataGridHelper
    {
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
