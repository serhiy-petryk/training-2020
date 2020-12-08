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
using WpfInvestigate.Controls;

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

        public static readonly DependencyProperty VisibleButtonsProperty = DependencyProperty.RegisterAttached("VisibleButtons",
            typeof(Buttons?), typeof(TextBoxEffects), new PropertyMetadata(null, propertyChangedCallback: OnButtonsChanged));
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetVisibleButtons(DependencyObject d, Buttons? value) => d.SetValue(VisibleButtonsProperty, value);
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static Buttons? GetVisibleButtons(DependencyObject d) => (Buttons?)d.GetValue(VisibleButtonsProperty);

        private static void OnButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textBox))
            {
                Debug.Print($"TextBoxEffects.VisibleButtons is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            RemoveButtons(textBox);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                AddButtons(textBox);
            }));

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

                    grid.Children.Remove(element);
                }

                foreach (var cd in grid.ColumnDefinitions.Where(c => c.Name.StartsWith(GridColumnPrefix)).ToArray())
                    grid.ColumnDefinitions.Remove(cd);
            }
        }
        private static void AddButtons(TextBox textBox)
        {
            var buttons = GetVisibleButtons(textBox);
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
                    // Add keyboard button
                    var keyboardButton = new ToggleButton
                    {
                        Name = ElementPrefix + "Keyboard",
                        Width = 16, Focusable = false, IsTabStop = false, 
                        IsThreeState = false,
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
                    grid.Children.Add(keyboardButton);
                    Grid.SetColumn(keyboardButton, grid.ColumnDefinitions.Count - 1);

                    // Add popup
                    var keyboardControl = new VirtualKeyboard { LinkedTextBox = textBox };
                    var shellControl = new PopupResizeControl { DoesContentSupportElasticLayout = true, Content = keyboardControl };
                    CornerRadiusEffect.SetCornerRadius(shellControl, new CornerRadius(3));
                    var popup = new Popup
                    {
                        Name = ElementPrefix + "Popup",
                        Width = 700,
                        Height = 250,
                        AllowsTransparency = true,
                        Focusable = false,
                        Placement = PlacementMode.Bottom,
                        PlacementTarget = textBox,
                        PopupAnimation = PopupAnimation.Fade,
                        StaysOpen = false,
                        Child = shellControl
                    };

                    grid.Children.Add(popup);
                    Grid.SetColumn(popup, grid.ColumnDefinitions.Count - 1);

                    // Set bindings between button and popup
                    popup.SetBinding(Popup.IsOpenProperty, new Binding
                    {
                        Source = keyboardButton,
                        Path = new PropertyPath(ToggleButton.IsCheckedProperty)
                    });

                    keyboardButton.SetBinding(UIElement.IsHitTestVisibleProperty,
                        new Binding
                        {
                            Source = popup, Path = new PropertyPath(Popup.IsOpenProperty),
                            Converter = MathConverter.Instance, ConverterParameter = "!"
                        });
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
