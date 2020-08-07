using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Supports textBox, passwordBox, editable combobox, numeric box, datetime/time picker
    /// Usage: <PasswordBox controls:WatermarkEffect.Watermark="AAAA" /> or
    /// Usage: <PasswordBox controls:WatermarkEffect.Watermark="AAAA" controls:WatermarkEffect.Foreground="Blue" />
    /// </summary>
    public class WatermarkEffect
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "Watermark", typeof(string), typeof(WatermarkEffect), new UIPropertyMetadata(string.Empty, OnPropertiesChanged));
        public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);
        public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);
        
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
            "Foreground", typeof(Brush), typeof(WatermarkEffect), new FrameworkPropertyMetadata(null, OnPropertiesChanged));
        public static Brush GetForeground(DependencyObject obj) => (Brush)obj.GetValue(ForegroundProperty);
        public static void SetForeground(DependencyObject obj, Brush value) => obj.SetValue(ForegroundProperty, value);

        //=====================================
        private static void OnPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    var txtBox = fe as TextBox ?? Tips.GetVisualChildren(fe).FirstOrDefault(c => c is TextBox) as TextBox;
                    if (txtBox != null)
                    {
                        txtBox.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => TxtBox_TextChanged(txtBox, new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.None))));
                        TxtBox_Unloaded(txtBox, null);
                        txtBox.TextChanged += TxtBox_TextChanged;
                        txtBox.GotFocus += ControlBox_ChangeFocus;
                        txtBox.LostFocus += ControlBox_ChangeFocus;
                        txtBox.Unloaded += TxtBox_Unloaded;
                        return;
                    }

                    var pswBox = fe as PasswordBox ?? Tips.GetVisualChildren(fe).FirstOrDefault(c => c is PasswordBox) as PasswordBox;
                    if (pswBox != null)
                    {
                        pswBox.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ControlBox_ChangeFocus(pswBox, new RoutedEventArgs())));
                        PswBox_Unloaded(pswBox, null);
                        pswBox.PasswordChanged += ControlBox_ChangeFocus;
                        pswBox.GotFocus += ControlBox_ChangeFocus;
                        pswBox.LostFocus += ControlBox_ChangeFocus;
                        pswBox.Unloaded += PswBox_Unloaded;
                        return;
                    }

                    Debug.Print($"WatermarkEffect.Watermark is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                }));
            }
        }

        private static void TxtBox_Unloaded(object sender, RoutedEventArgs e)
        {
            var txtBox = (TextBox)sender;
            txtBox.TextChanged -= TxtBox_TextChanged;
            txtBox.GotFocus -= ControlBox_ChangeFocus;
            txtBox.LostFocus -= ControlBox_ChangeFocus;
            txtBox.Unloaded -= TxtBox_Unloaded;
        }

        private static void PswBox_Unloaded(object sender, RoutedEventArgs e)
        {
            var pswBox = (PasswordBox)sender;
            pswBox.PasswordChanged -= ControlBox_ChangeFocus;
            pswBox.GotFocus -= ControlBox_ChangeFocus;
            pswBox.LostFocus -= ControlBox_ChangeFocus;
            pswBox.Unloaded -= PswBox_Unloaded;
        }

        private static void ControlBox_ChangeFocus(object sender, RoutedEventArgs e)
        {
            CheckWatermark((Control)sender);
        }

        private static void TxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckWatermark((TextBox)sender);
        }

        private static void CheckWatermark(Control ctrlBox)
        {
            DependencyObject current = ctrlBox;
            string watermark = null;
            Brush foregroundBrush = null;
            while (current != null && string.IsNullOrEmpty(watermark))
            {
                watermark = GetWatermark(current);
                foregroundBrush = GetForeground(current);
                current = VisualTreeHelper.GetParent(current);
            }

            var boxContent = ctrlBox is TextBox ? ((TextBox)ctrlBox).Text : ((PasswordBox)ctrlBox).Password;

            if (string.IsNullOrWhiteSpace(boxContent) && !ctrlBox.IsFocused && !string.IsNullOrEmpty(watermark))
                ShowWatermark(ctrlBox, watermark, foregroundBrush);
            else
                HideWatermark(ctrlBox);
        }

        private static void ShowWatermark(Control ctrlBox, string watermark, Brush foregroundBrush)
        {
            if (ctrlBox == null)
                return;

            if (foregroundBrush == null)
            {
                if (ctrlBox.Foreground is SolidColorBrush)
                {
                    var color = ((SolidColorBrush)ctrlBox.Foreground).Color;
                    foregroundBrush = new SolidColorBrush(Color.FromArgb(Convert.ToByte(color.A / 2), color.R, color.G, color.B));
                }
                else
                    foregroundBrush = Brushes.DarkGray;
            }

            var partWatermark = ctrlBox?.Template.FindName("PART_Watermark", ctrlBox) as ContentControl;
            if (partWatermark == null)
            {
                var layer = AdornerLayer.GetAdornerLayer(ctrlBox);
                var adornerControl = layer.GetAdorners(ctrlBox)?.FirstOrDefault(a => a is AdornerControl && ((AdornerControl) a).Child.Name == "Watermark") as AdornerControl;

                if (adornerControl == null)
                {
                    var adorner = new TextBox
                    {
                        Name = "Watermark",
                        IsReadOnly = true,
                        Focusable = false,
                        IsHitTestVisible = false,
                        Margin = new Thickness(),
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent
                    };
                    adornerControl = new AdornerControl(ctrlBox) {Child = adorner};
                    layer.Add(adornerControl);
                }
                else
                    adornerControl.Visibility = Visibility.Visible;

                var child = adornerControl.Child as TextBox;
                child.Text = watermark ?? "";
                child.Foreground = foregroundBrush;
                child.BorderThickness = ctrlBox.BorderThickness;
                child.Padding = ctrlBox.Padding;
                child.Width = ctrlBox.ActualWidth;
                child.Height = ctrlBox.ActualHeight;
                child.VerticalAlignment = ctrlBox.VerticalAlignment;
                child.VerticalContentAlignment = ctrlBox.VerticalContentAlignment;
                child.HorizontalAlignment = ctrlBox.HorizontalAlignment;
                child.HorizontalContentAlignment = ctrlBox.HorizontalContentAlignment;
                child.Visibility = ctrlBox.Visibility;
            }
            else
            { // DatePickerTextBox
                partWatermark.Content = watermark ?? "";
                partWatermark.Foreground = foregroundBrush;
            }
        }

        private static void HideWatermark(Control ctrlBox)
        {
            var partWatermark = ctrlBox?.Template.FindName("PART_Watermark", ctrlBox) as ContentControl;
            if (partWatermark == null)
            {
                var layer = AdornerLayer.GetAdornerLayer(ctrlBox);
                var adorners = layer?.GetAdorners(ctrlBox) ?? new Adorner[0];
                foreach (var adorner in adorners.Where(a => a is AdornerControl && ((AdornerControl) a).Child.Name == "Watermark"))
                    adorner.Visibility = Visibility.Collapsed;
            }
        }

    }
}
