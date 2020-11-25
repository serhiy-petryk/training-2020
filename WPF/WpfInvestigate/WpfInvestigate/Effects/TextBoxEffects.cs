using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
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
            Clear = 2
        }

        private const string GridColumnPrefix = "TextBoxButtons";

        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.RegisterAttached("Buttons",
            typeof(Buttons?), typeof(TextBoxEffects), new PropertyMetadata(null, propertyChangedCallback: OnButtonsChanged));
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static void SetButtons(DependencyObject d, Buttons? value) => d.SetValue(ButtonsProperty, value);
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
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
                AddButtons(textBox);
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
                foreach (var cd in grid.ColumnDefinitions.Where(c => c.Name.StartsWith("GridColumnButtonName")).ToArray())
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
                if ((buttons.Value & Buttons.Keyboard) == Buttons.Keyboard)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, Name = GridColumnPrefix + "Keyboard" });
                    var keyboardButton = new Button
                    {
                        Width = 16,
                        Margin = new Thickness(1, 0, 0, 0),
                        Padding = new Thickness(0)
                    };

                    ChromeEffect.SetBichromeAnimatedBackground(keyboardButton, Tips.GetColorFromBrush(textBox.Background));
                    ChromeEffect.SetBichromeAnimatedForeground(keyboardButton, Tips.GetColorFromBrush(textBox.Foreground));
                    IconEffect.SetGeometry(keyboardButton, Application.Current.FindResource("KeyboardGeometry") as Geometry);
                    grid.Children.Add(keyboardButton);
                    Grid.SetColumn(keyboardButton, grid.ColumnDefinitions.Count - 1);
                }

                if ((buttons.Value & Buttons.Keyboard) == Buttons.Keyboard)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, Name = GridColumnPrefix + "Clear" });
                    var style = Application.Current.FindResource("ClearBichromeAnimatedButtonStyle") as Style;
                    var clearButton = new Button
                    {
                        Style = style,
                        Width = 14,
                        // Margin = new Thickness(-2, 0, 1 - dp.Padding.Right, 0),
                        Margin = new Thickness(1, 0, 0, 0),
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
