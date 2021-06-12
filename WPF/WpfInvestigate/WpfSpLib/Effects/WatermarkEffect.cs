﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Controls;
using WpfSpLib.Helpers;

namespace WpfSpLib.Effects
{
    /// <summary>
    /// Supports textBox, passwordBox, editable combobox, numeric box, datetime/time picker
    /// Usage: <PasswordBox controls:WatermarkEffect.Watermark="AAAA" /> or
    /// Usage: <PasswordBox controls:WatermarkEffect.Watermark="AAAA" controls:WatermarkEffect.Foreground="Blue" />
    /// </summary>
    public class WatermarkEffect
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "Watermark", typeof(string), typeof(WatermarkEffect), new FrameworkPropertyMetadata(string.Empty, OnPropertiesChanged));
        public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);
        public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);
        
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
            "Foreground", typeof(Brush), typeof(WatermarkEffect), new FrameworkPropertyMetadata(null, OnPropertiesChanged));
        public static Brush GetForeground(DependencyObject obj) => (Brush)obj.GetValue(ForegroundProperty);
        public static void SetForeground(DependencyObject obj, Brush value) => obj.SetValue(ForegroundProperty, value);

        //=====================================
        private static void OnPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement element)) return;

            element.Unloaded -= Deactivate;
            element.Loaded -= Activate;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                element.Unloaded += Deactivate;
                element.Loaded += Activate;
                Activate(element, null);
            }, DispatcherPriority.Loaded);
        }

        private static void Activate(object sender, RoutedEventArgs e)
        {
            Deactivate(sender, e);
            var element = (FrameworkElement)sender;
            var txtBox = element as TextBox ?? element.GetVisualChildren().FirstOrDefault(c => c is TextBox) as TextBox;
            var pswBox = element as PasswordBox ?? element.GetVisualChildren().FirstOrDefault(c => c is PasswordBox) as PasswordBox;
            if (txtBox != null)
            {
                txtBox.TextChanged += TxtBox_TextChanged;
                txtBox.GotFocus += ControlBox_ChangeFocus;
                txtBox.LostFocus += ControlBox_ChangeFocus;
                TxtBox_TextChanged(txtBox, new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.None));
            }
            else if (pswBox != null)
            {
                pswBox.PasswordChanged += ControlBox_ChangeFocus;
                pswBox.GotFocus += ControlBox_ChangeFocus;
                pswBox.LostFocus += ControlBox_ChangeFocus;
                ControlBox_ChangeFocus(pswBox, new RoutedEventArgs());
            }
            else if (element.IsArrangeValid)
                Debug.Print($"WatermarkEffect.Watermark is not implemented for {element.GetType().Namespace}.{element.GetType().Name} type");
        }

        private static void Deactivate(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var txtBox = element as TextBox ?? element.GetVisualChildren().FirstOrDefault(c => c is TextBox) as TextBox;
            var pswBox = element as PasswordBox ?? element.GetVisualChildren().FirstOrDefault(c => c is PasswordBox) as PasswordBox;

            if (txtBox != null)
            {
                txtBox.TextChanged -= TxtBox_TextChanged;
                txtBox.GotFocus -= ControlBox_ChangeFocus;
                txtBox.LostFocus -= ControlBox_ChangeFocus;
            }
            else if (pswBox != null)
            {
                pswBox.PasswordChanged -= ControlBox_ChangeFocus;
                pswBox.GotFocus -= ControlBox_ChangeFocus;
                pswBox.LostFocus -= ControlBox_ChangeFocus;
            } 
            else if (element.IsArrangeValid)
                Debug.Print($"WatermarkEffect.Watermark is not implemented for {element.GetType().Namespace}.{element.GetType().Name} type");
        }

        private static void ControlBox_ChangeFocus(object sender, RoutedEventArgs e) => CheckWatermark((Control)sender);
        private static void TxtBox_TextChanged(object sender, TextChangedEventArgs e) => CheckWatermark((TextBox)sender);

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

            var boxContent = ctrlBox is TextBox textBox ? textBox.Text : ((PasswordBox)ctrlBox).Password;

            if (string.IsNullOrWhiteSpace(boxContent) && !string.IsNullOrEmpty(watermark))
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
                if (ctrlBox.Foreground is SolidColorBrush scb)
                {
                    var color = scb.Color;
                    foregroundBrush = new SolidColorBrush(Color.FromArgb(Convert.ToByte(color.A / 2), color.R, color.G, color.B));
                }
                else
                    foregroundBrush = Brushes.DarkGray;
            }

            var partWatermark = ctrlBox.Template.FindName("PART_Watermark", ctrlBox) as ContentControl;
            if (partWatermark == null)
            {
                var ctrlBoxView = ctrlBox.GetVisualChildren().FirstOrDefault(a => a.GetType().Name == "TextBoxView") as FrameworkElement;
                var layer = AdornerLayer.GetAdornerLayer(ctrlBoxView);
                if (layer == null) return;

                var adornerControl = layer.GetAdorners(ctrlBoxView)?.OfType<AdornerControl>().FirstOrDefault(a => a.Child.Name == "Watermark");

                if (adornerControl == null)
                {
                    var adorner = new Border
                    {
                        Name = "Watermark",
                        Focusable = false, IsHitTestVisible = false,
                        Padding = new Thickness(), Margin = new Thickness(),
                        BorderThickness = new Thickness(), Background = Brushes.Transparent,
                        Child = new TextBlock
                        {
                            Background = Brushes.Transparent, Margin = new Thickness(), Padding=new Thickness(),
                            Focusable = false, IsHitTestVisible = false
                        }
                    };
                    adornerControl = new AdornerControl(ctrlBoxView) {Child = adorner};
                    layer.Add(adornerControl);
                }
                else
                    adornerControl.Visibility = ctrlBox.Visibility;

                var textSize = ControlHelper.MeasureString(watermark, ctrlBox);
                ctrlBoxView.MinWidth = textSize.Width;

                var textBlock = ((Border)adornerControl.Child).Child as TextBlock;
                textBlock.Text = watermark;
                textBlock.Foreground = foregroundBrush;
                textBlock.FontSize = ctrlBox.FontSize;
                textBlock.VerticalAlignment = ctrlBox.VerticalContentAlignment;
                textBlock.HorizontalAlignment = ctrlBox.HorizontalContentAlignment;
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
                var ctrlBoxView = ctrlBox.GetVisualChildren().FirstOrDefault(a => a.GetType().Name == "TextBoxView") as FrameworkElement;
                var layer = AdornerLayer.GetAdornerLayer(ctrlBoxView);
                var adorners = layer?.GetAdorners(ctrlBoxView) ?? new Adorner[0];
                foreach (var adorner in adorners.OfType<AdornerControl>().Where(a => a.Child.Name == "Watermark"))
                    adorner.Visibility = Visibility.Collapsed;
            }
        }

    }
}
