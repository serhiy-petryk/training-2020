using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Effects
{
    public class TextBoxEffects
    {
        #region ===============  Buttons attached property  ================
        [Flags]
        public enum Buttons
        {
            Keyboard = 1,
            Clear = 2,
            Separator1px = 4,
            Separator = 8
        }

        private const string GridColumnPrefix = "TextBoxButtons";

        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.RegisterAttached("Buttons",
            typeof(Buttons?), typeof(TextBoxEffects), new PropertyMetadata(null, propertyChangedCallback: OnButtonsChanged));
        [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
        public static void SetButtons(DependencyObject d, Buttons? value) => d.SetValue(ButtonsProperty, value);
        [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
        public static Buttons? GetButtons(DependencyObject d) => (Buttons?)d.GetValue(ButtonsProperty);

        private static void OnButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBoxBase textBox))
            {
                Debug.Print($"TextBoxEffects.Buttons is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                OnUnloadedForButtons(textBox, null);
                textBox.Unloaded += OnUnloadedForButtons;
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                    AddButtons(textBox)
                ));
            }));

        }

        private static void OnUnloadedForButtons(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBoxBase;
            textBox.Unloaded -= OnUnloadedForButtons;
            RemoveButtons(textBox);
        }

        private static void RemoveButtons(TextBoxBase textBox)
        {
            var grid = Tips.GetVisualChildren(textBox).OfType<Grid>().FirstOrDefault();
            if (grid != null)
            {
                var separator = Tips.GetVisualChildren(grid).OfType<Rectangle>().FirstOrDefault(a=> a.Name== "BorderSeparator");
                if (separator != null)
                    BindingOperations.ClearBinding(separator, FrameworkElement.WidthProperty);

                foreach (var cd in grid.ColumnDefinitions.Where(c => c.Name.StartsWith(GridColumnPrefix)).ToArray())
                    grid.ColumnDefinitions.Remove(cd);
            }
        }
        private static void AddButtons(TextBoxBase textBox)
        {
            var buttons = GetButtons(textBox);
            if (!buttons.HasValue) return;

            var grid = Tips.GetVisualChildren(textBox).OfType<Grid>().FirstOrDefault();
            if (grid != null)
            {
                if ((buttons.Value & Buttons.Separator1px) == Buttons.Separator1px || (buttons.Value & Buttons.Separator) == Buttons.Separator)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, Name = GridColumnPrefix + "Separator" });

                    var separator = new Rectangle{Name= "BorderSeparator" };
                    if ((buttons.Value & Buttons.Separator1px) == Buttons.Separator1px)
                        separator.Width = 1;
                    else
                    {
                        var bindingBorderWidth = new Binding
                        {
                            Source = textBox,
                            Path = new PropertyPath("BorderThickness.Right"),
                        };
                        separator.SetBinding(FrameworkElement.WidthProperty, bindingBorderWidth);
                    }

                    var bindingFillBorder = new Binding
                    {
                        Source = textBox,
                        Path = new PropertyPath(Control.BorderBrushProperty),
                    };
                    separator.SetBinding(Shape.FillProperty, bindingFillBorder);

                    grid.Children.Add(separator);
                    Grid.SetColumn(separator, grid.ColumnDefinitions.Count - 1);
                }

                if ((buttons.Value & Buttons.Keyboard) == Buttons.Keyboard)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, Name = GridColumnPrefix + "Keyboard" });
                    var keyboardButton = new Button
                    {
                        Width = 16,
                        Margin = new Thickness(0),
                        Padding = new Thickness(0)
                    };

                    var bindingBackground = new Binding
                    {
                        Source = textBox,
                        Path = new PropertyPath(Control.BackgroundProperty),
                        Converter = ColorHslBrush.Instance
                    };
                    keyboardButton.SetBinding(ChromeEffect.BichromeAnimatedBackgroundProperty, bindingBackground);

                    var bindingForeground = new Binding
                    {
                        Source = textBox,
                        Path = new PropertyPath(Control.ForegroundProperty),
                        Converter = ColorHslBrush.Instance
                    };
                    keyboardButton.SetBinding(ChromeEffect.BichromeAnimatedForegroundProperty, bindingForeground);

                    IconEffect.SetGeometry(keyboardButton, Application.Current.FindResource("KeyboardGeometry") as Geometry);
                    grid.Children.Add(keyboardButton);
                    Grid.SetColumn(keyboardButton, grid.ColumnDefinitions.Count - 1);
                }

                if ((buttons.Value & Buttons.Clear) == Buttons.Clear)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, Name = GridColumnPrefix + "Clear" });
                    var style = Application.Current.FindResource("ClearBichromeAnimatedButtonStyle") as Style;
                    var clearButton = new Button
                    {
                        Style = style,
                        Width = 14,
                        Margin = new Thickness(0),
                        Padding = new Thickness(1)
                    };

                    // clearButton.Click += ClearButton_Click;
                    grid.Children.Add(clearButton);
                    Grid.SetColumn(clearButton, grid.ColumnDefinitions.Count - 1);
                }
            }
        }
        #endregion
    }
}
