using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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

        private const string GridColumnPrefix = "TextBoxButtonsColumn";
        private const string ElementPrefix = "TextBoxEffects";

        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.RegisterAttached("Buttons",
            typeof(Buttons?), typeof(TextBoxEffects), new PropertyMetadata(null, propertyChangedCallback: OnButtonsChanged));
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetButtons(DependencyObject d, Buttons? value) => d.SetValue(ButtonsProperty, value);
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static Buttons? GetButtons(DependencyObject d) => (Buttons?)d.GetValue(ButtonsProperty);

        private static void OnButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textBox))
            {
                Debug.Print($"TextBoxEffects.Buttons is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                OnUnloadedForButtons(textBox, null);
                textBox.Unloaded += OnUnloadedForButtons;
                AddButtons(textBox);
            }));

        }

        private static void OnUnloadedForButtons(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Unloaded -= OnUnloadedForButtons;
            RemoveButtons(textBox);
        }

        private static void RemoveButtons(TextBox textBox)
        {
            var grid = Tips.GetVisualChildren(textBox).OfType<Grid>().FirstOrDefault();
            if (grid != null)
            {
                foreach (var element in Tips.GetVisualChildren(grid).OfType<FrameworkElement>().Where(c => c.Name.StartsWith(ElementPrefix)).ToArray())
                {
                    if (element.Name.Contains("Clear"))
                        element.PreviewMouseLeftButtonDown -= ClearButton_Click;
                    else if (element.Name.Contains("Keyboard"))
                        element.PreviewMouseLeftButtonDown -= KeyboardButton_Click;

                    grid.Children.Remove(element);
                }

                foreach (var cd in grid.ColumnDefinitions.Where(c => c.Name.StartsWith(GridColumnPrefix)).ToArray())
                    grid.ColumnDefinitions.Remove(cd);
            }
        }
        private static void AddButtons(TextBox textBox)
        {
            var buttons = GetButtons(textBox);
            if (!buttons.HasValue) return;

            var grid = Tips.GetVisualChildren(textBox).OfType<Grid>().FirstOrDefault();
            if (grid != null)
            {
                if ((buttons.Value & Buttons.Separator1px) == Buttons.Separator1px || (buttons.Value & Buttons.Separator) == Buttons.Separator)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, Name = GridColumnPrefix + "Separator" });

                    var separator = new Rectangle {Name = ElementPrefix + "Separator"};
                    if ((buttons.Value & Buttons.Separator1px) == Buttons.Separator1px)
                        separator.Width = 1;
                    else
                        separator.SetBinding(FrameworkElement.WidthProperty, new Binding
                        {
                            Source = textBox,
                            Path = new PropertyPath("BorderThickness.Right"),
                        });

                    separator.SetBinding(Shape.FillProperty, new Binding
                    {
                        Source = textBox,
                        Path = new PropertyPath(Control.BorderBrushProperty),
                    });

                    grid.Children.Add(separator);
                    Grid.SetColumn(separator, grid.ColumnDefinitions.Count - 1);
                }

                if ((buttons.Value & Buttons.Keyboard) == Buttons.Keyboard)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, Name = GridColumnPrefix + "Keyboard" });
                    var keyboardButton = new Button
                    {
                        Name = ElementPrefix + "Keyboard",
                        Width = 16, Focusable = false, IsTabStop = false, 
                        Margin = new Thickness(0),
                        Padding = new Thickness(0),
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    keyboardButton.SetBinding(ChromeEffect.BichromeAnimatedBackgroundProperty, new Binding
                    {
                        Source = textBox,
                        Path = new PropertyPath(Control.BackgroundProperty),
                        Converter = ColorHslBrush.Instance
                    });
                    keyboardButton.SetBinding(ChromeEffect.BichromeAnimatedForegroundProperty, new Binding
                    {
                        Source = textBox,
                        Path = new PropertyPath(Control.ForegroundProperty),
                        Converter = ColorHslBrush.Instance
                    });

                    IconEffect.SetGeometry(keyboardButton, Application.Current.FindResource("KeyboardGeometry") as Geometry);
                    keyboardButton.PreviewMouseLeftButtonDown += KeyboardButton_Click;
                    grid.Children.Add(keyboardButton);
                    Grid.SetColumn(keyboardButton, grid.ColumnDefinitions.Count - 1);
                }

                if ((buttons.Value & Buttons.Clear) == Buttons.Clear)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, Name = GridColumnPrefix + "Clear" });
                    var clearButton = new Button
                    {
                        Name = ElementPrefix + "Clear",
                        Style = Application.Current.FindResource("ClearBichromeAnimatedButtonStyle") as Style,
                        Width = 14, Focusable = false, IsTabStop = false,
                        Margin = new Thickness(0),
                        Padding = new Thickness(1),
                        VerticalAlignment = VerticalAlignment.Stretch
                    };

                    clearButton.PreviewMouseLeftButtonDown += ClearButton_Click;
                    grid.Children.Add(clearButton);
                    Grid.SetColumn(clearButton, grid.ColumnDefinitions.Count - 1);
                }
            }
        }

        private static void KeyboardButton_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private static void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var current = (DependencyObject)sender;
            while (current != null && !(current is TextBox))
                current = VisualTreeHelper.GetParent(current) ?? (current as FrameworkElement)?.Parent;

            if (current != null)
            {
                ((TextBox)current).Text = string.Empty;
                ((TextBox)current).Focus();
            }
        }

        #endregion
    }
}
