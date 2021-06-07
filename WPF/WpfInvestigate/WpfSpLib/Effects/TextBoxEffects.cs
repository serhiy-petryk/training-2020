using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Controls;
using WpfSpLib.Helpers;

namespace WpfSpLib.Effects
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
            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                if (!textBox.IsElementDisposing())
                    AddButtons(textBox);
            }, DispatcherPriority.Loaded);

        }

        private static void RemoveButtons(TextBox textBox)
        {
            var grid = Tips.GetVisualChildren(textBox).OfType<Grid>().FirstOrDefault();
            if (grid != null)
            {
                foreach (var element in Tips.GetVisualChildren(grid).OfType<FrameworkElement>().Where(c => c.Name.StartsWith(ElementPrefix)).ToArray())
                {
                    if (element.Name.Contains("Clear"))
                        element.PreviewMouseLeftButtonDown -= ClearButton_OnClick;
                    else if (element.Name.Contains("Popup"))
                        ((Popup)element).Opened -= Popup_OnOpened;
                    else if (element.Name.Contains("KeyboardControl"))
                        ((VirtualKeyboard)element).OnReturnKeyClick -= KeyboardControl_OnReturnKeyClick;

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

                    var separator = new Rectangle { Name = ElementPrefix + "Separator" };
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
                        Width = 16,
                        Focusable = false,
                        IsThreeState = false,
                        Margin = new Thickness(0),
                        Padding = new Thickness(0),
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    keyboardButton.SetBinding(ChromeEffect.BichromeBackgroundProperty, new Binding
                    {
                        Source = textBox,
                        Path = new PropertyPath(Control.BackgroundProperty),
                        Converter = ColorHslBrush.Instance
                    });
                    keyboardButton.SetBinding(ChromeEffect.BichromeForegroundProperty, new Binding
                    {
                        Source = textBox,
                        Path = new PropertyPath(Control.ForegroundProperty),
                        Converter = ColorHslBrush.Instance
                    });

                    IconEffect.SetGeometry(keyboardButton, Application.Current.FindResource("KeyboardGeometry") as Geometry);
                    grid.Children.Add(keyboardButton);
                    Grid.SetColumn(keyboardButton, grid.ColumnDefinitions.Count - 1);

                    // Add popup
                    var keyboardControl = new VirtualKeyboard { Name = ElementPrefix + "KeyboardControl", Focusable = false };
                    keyboardControl.OnReturnKeyClick += KeyboardControl_OnReturnKeyClick;
                    var shellControl = new PopupResizeControl { DoesContentSupportElasticLayout = true, Content = keyboardControl, Focusable = false };
                    CornerRadiusEffect.SetCornerRadius(shellControl, new CornerRadius(3));
                    var popup = new Popup
                    {
                        Name = ElementPrefix + "Popup",
                        Width = 700,
                        Height = 250,
                        Focusable = false,
                        AllowsTransparency = true,
                        StaysOpen = false,
                        Placement = PlacementMode.Bottom,
                        PlacementTarget = textBox,
                        PopupAnimation = PopupAnimation.Fade,
                        Child = shellControl
                    };
                    popup.Opened += Popup_OnOpened;

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
                            Source = popup,
                            Path = new PropertyPath(Popup.IsOpenProperty),
                            Converter = MathConverter.Instance,
                            ConverterParameter = "!"
                        });
                }

                if ((buttons.Value & Buttons.Clear) == Buttons.Clear)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, Name = GridColumnPrefix + "Clear" });
                    var clearButton = new Button
                    {
                        Name = ElementPrefix + "Clear",
                        Style = Application.Current.FindResource("ClearBichromeButtonStyle") as Style,
                        Width = 14,
                        Focusable = false,
                        Margin = new Thickness(0),
                        Padding = new Thickness(1),
                        VerticalAlignment = VerticalAlignment.Stretch
                    };

                    clearButton.PreviewMouseLeftButtonDown += ClearButton_OnClick;
                    grid.Children.Add(clearButton);
                    Grid.SetColumn(clearButton, grid.ColumnDefinitions.Count - 1);
                }
            }
        }

        #region  ============  Event handlers  ============
        private static void KeyboardControl_OnReturnKeyClick(object sender, EventArgs e)
        {
            var vk = (VirtualKeyboard)sender;

            if (Tips.GetVisualParents(vk).OfType<Popup>().FirstOrDefault() is Popup popup)
                popup.IsOpen = false;

            if (Keyboard.FocusedElement is FrameworkElement element)
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private static void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            var current = (DependencyObject)sender;
            while (current != null && !(current is TextBox))
                current = VisualTreeHelper.GetParent(current) ?? (current as FrameworkElement)?.Parent;

            if (current != null)
                ((TextBox)current).Text = string.Empty;
        }

        private static void Popup_OnOpened(object sender, EventArgs e) => ((Popup)sender).PlacementTarget.Focus();
        #endregion
        #endregion
    }
}
