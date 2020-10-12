using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls.Helpers
{
    public static class ControlHelper
    {
        public static Viewbox AddIconToControl(ContentControl control, bool iconBeforeContent, Geometry icon, double? iconWidth = null, Thickness? iconMargin = null)
        {
            var path = new Path { Stretch = Stretch.Uniform, Margin = new Thickness(), Data = icon };
            path.SetBinding(Shape.FillProperty, new Binding("Foreground")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Control), 1)
            });
            var viewbox = new Viewbox { Child = path, VerticalAlignment = VerticalAlignment.Stretch };
            if (iconMargin.HasValue)
                viewbox.Margin = iconMargin.Value;
            if (iconWidth.HasValue)
                viewbox.Width = iconWidth.Value;

            if (control.HasContent)
            {
                var grid = new Grid { ClipToBounds = true, Margin = new Thickness(), SnapsToDevicePixels = true};
                grid.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = iconBeforeContent ? GridLength.Auto : new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = !iconBeforeContent ? GridLength.Auto : new GridLength(1, GridUnitType.Star) });

                grid.Children.Add(viewbox);
                Grid.SetColumn(viewbox, iconBeforeContent ? 0 : 1);

                var contentControl = new ContentPresenter
                {
                    Content = control.Content,
                    Margin = control.Padding,
                    VerticalAlignment = control.VerticalContentAlignment,
                    HorizontalAlignment = control.HorizontalContentAlignment
                };
                control.Content = null;
                // tb.VerticalContentAlignment = VerticalAlignment.Stretch;
                control.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                grid.Children.Add(contentControl);
                Grid.SetColumn(contentControl, iconBeforeContent ? 1 : 0);

                control.Content = grid;
            }
            else
                control.Content = viewbox;
            
            control.Padding = new Thickness();

            return viewbox;
        }

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
            if (DesignerProperties.GetIsInDesignMode(toolBar))
                return; // VS designer error

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
