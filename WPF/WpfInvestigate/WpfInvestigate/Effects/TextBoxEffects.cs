using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace WpfInvestigate.Effects
{
    public class TextBoxEffects
    {
        public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached(
            "SelectAllOnFocus", typeof(bool), typeof(TextBoxEffects), new UIPropertyMetadata(false, OnSelectAllOnFocusChanged));
        public static bool GetSelectAllOnFocus(DependencyObject obj) => (bool)obj.GetValue(SelectAllOnFocusProperty);
        public static void SetSelectAllOnFocus(DependencyObject obj, bool value) => obj.SetValue(SelectAllOnFocusProperty, value);

        private static void OnSelectAllOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBoxBase textBox)
            {
                TextBox_Unloaded(textBox, null);
                if (Equals(e.NewValue, true))
                {
                    textBox.GotFocus += TextBox_GotFocus;
                    textBox.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDown;
                    textBox.MouseDoubleClick += TextBox_PreviewMouseLeftButtonDown;
                    textBox.Unloaded += TextBox_Unloaded;
                }
            }
            else
                Debug.Print($"TextBoxEffects.SelectAllOnFocus is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
        }

        private static void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var textBox = (TextBoxBase)sender;
            if (!textBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                textBox.Focus();
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBoxBase)sender;
            textBox.SelectAll();
        }

        private static void TextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBoxBase)sender;
            textBox.GotFocus -= TextBox_GotFocus;
            textBox.PreviewMouseLeftButtonDown -= TextBox_PreviewMouseLeftButtonDown;
            textBox.Unloaded -= TextBox_Unloaded;
        }
    }
}
