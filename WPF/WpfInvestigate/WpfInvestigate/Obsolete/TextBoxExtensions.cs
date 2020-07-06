// see comment in https://stackoverflow.com/questions/11873378/adding-placeholder-text-to-textbox

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfInvestigate.Obsolete
{
    public static class TextBoxExtensions
    {
        private static readonly string PlaceholderMarker = ((char)1).ToString();

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.RegisterAttached("Placeholder", typeof(string),
            typeof(TextBoxExtensions), new PropertyMetadata(default(string), propertyChangedCallback: OnPlaceholderChanged));

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetPlaceholder(DependencyObject d, string value) => d.SetValue(PlaceholderProperty, value);
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static string GetPlaceholder(DependencyObject d) => (string)d.GetValue(PlaceholderProperty);

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox tb))
            {
                Debug.Print($"TextBoxExtensions.Placeholder is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            OnUnloaded(tb, null);
            if (e.NewValue != null)
            {
                tb.GotFocus += OnGotFocus;
                tb.LostFocus += OnLostFocus;
                tb.Unloaded += OnUnloaded;
            }

            SetPlaceholder(d, e.NewValue as string);

            if (!tb.IsFocused)
                ShowPlaceholder(tb);
        }

        private static void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.LostFocus -= OnLostFocus;
            tb.GotFocus -= OnGotFocus;
            tb.Unloaded -= OnUnloaded;
        }

        private static void OnLostFocus(object sender, RoutedEventArgs routedEventArgs) => ShowPlaceholder(sender as TextBox);
        private static void OnGotFocus(object sender, RoutedEventArgs routedEventArgs) => HidePlaceholder(sender as TextBox);

        private static void ShowPlaceholder(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = PlaceholderMarker + GetPlaceholder(textBox);
                if (textBox.Foreground is SolidColorBrush)
                {
                    var color = ((SolidColorBrush) textBox.Foreground).Color;
                    textBox.Foreground = new SolidColorBrush(Color.FromArgb(Convert.ToByte(color.A/2), color.R, color.G, color.B ));
                }
            }
        }

        private static void HidePlaceholder(TextBox textBox)
        {
            if (textBox.Text.StartsWith(PlaceholderMarker))
            {
                textBox.Text = string.Empty;
                if (textBox.Foreground is SolidColorBrush)
                {
                    var color = ((SolidColorBrush)textBox.Foreground).Color;
                    textBox.Foreground = new SolidColorBrush(Color.FromArgb(Convert.ToByte(color.A * 2), color.R, color.G, color.B));
                }
            }
        }
    }
}
