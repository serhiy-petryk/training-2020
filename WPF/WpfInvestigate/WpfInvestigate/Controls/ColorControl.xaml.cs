﻿// ToDo:
// 1. ColorBox for Color/CurrentColor.
// 2. ToggleButton + popup
//      may change layout to: ToggleButton -> Grid -> (ContentPresenter + Popup)
//      зовнішній вигляд toggleButton = зовнішній вигляд Content
// 3. Control with 4 tab:
//      - mini size: RGB sliders
//      - mini size: HSL sliders
//      - middle size: current view
//      - middle size with scroll: color boxes of known colors (140 items)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Common.ColorSpaces;
using WpfInvestigate.Effects;

namespace WpfInvestigate.Controls
{
    public partial class ColorControl : UserControl, INotifyPropertyChanged
    {
        private ColorControlViewModel VM => (ColorControlViewModel)DataContext;

        // Constructor
        public ColorControl()
        {
            InitializeComponent();
            SaveColor();
        }

        #region  ==============  User UI  ===========
        /// <summary>Original color</summary>
        public Color Color
        {
            get => VM.Color;
            set
            {
                VM.Color = value;
                OnPropertiesChanged(nameof(Color));
            }
        }

        public void SaveColor()
        {
            VM.SaveColor();
            OnPropertiesChanged(nameof(Color));
        }

        public void RestoreColor()
        {
            VM.RestoreColor();
            OnPropertiesChanged(nameof(Color));
        }
        #endregion

        #region ==============  Event handlers  ====================

        private void Control_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            foreach (var cc in fe.GetVisualChildren().OfType<Canvas>().Where(cc => cc.DataContext is ColorControlViewModel.XYSlider))
                ((ColorControlViewModel.XYSlider)cc.DataContext).SetSizeOfControl(cc);

            VM.UpdateUI();
        }
        #endregion

        #region ================  Event Handlers for Value Editor  ===============
        private void ValueEditor_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            if (e.Text == "\t") // tab key
                return;
            if (string.IsNullOrWhiteSpace(e.Text) || e.Text.Length != 1)
            {
                Tips.Beep();
                return;
            }

            var valueEditor = (TextBox)sender;
            var newText = valueEditor.Text.Substring(0, valueEditor.SelectionStart) + e.Text +
                          valueEditor.Text.Substring(valueEditor.SelectionStart + valueEditor.SelectionLength);
            if (VM.CurrentCulture.NumberFormat.NativeDigits.Contains(e.Text))
                e.Handled = false;
            if (VM.CurrentCulture.NumberFormat.NumberDecimalSeparator == e.Text)
                e.Handled = valueEditor.Text.Contains(VM.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            else if (VM.CurrentCulture.NumberFormat.NegativeSign == e.Text)
                e.Handled = (newText.Length > 1 && newText.Substring(1, newText.Length - 2)
                                 .Contains(VM.CurrentCulture.NumberFormat.NegativeSign)) ||
                            !(newText.StartsWith(e.Text) || newText.EndsWith(e.Text));

            if (e.Handled)
                Tips.Beep();
        }
        #endregion

        #region ===============  Color box event handlers  ===============
        private void ColorBox_OnSetColor(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var toggleButton = Tips.GetVisualParents(element).OfType<Grid>().SelectMany(grid => grid.Children.OfType<ToggleButton>()).FirstOrDefault();
            toggleButton.IsChecked = false;

            (element.DataContext as ColorControlViewModel.ColorToneBox).SetCurrentColor();
        }
        #endregion

        #region  =============  Slider event handlers  =====================
        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).CaptureMouse();
            Keyboard.ClearFocus();
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).ReleaseMouseCapture();
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = sender as Panel;
                var thumb = canvas.Children[0] as FrameworkElement;
                var isVertical = thumb is Grid;
                var x = Math.Max(0, Math.Min(1.0, isVertical
                    ? e.GetPosition(canvas).X / canvas.ActualWidth
                    : (e.GetPosition(canvas).X - thumb.ActualWidth / 2) / (canvas.ActualWidth - thumb.ActualWidth)));
                var y = Math.Max(0, Math.Min(1.0, e.GetPosition(canvas).Y / canvas.ActualHeight));
                ((ColorControlViewModel.XYSlider)canvas.DataContext).SetProperties(x, y);
            }
        }
        #endregion

        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tc = (TabControl) sender;
            var selectedItem = tc.SelectedItem as TabItem;
            if ((selectedItem?.Content as ScrollViewer)?.Content is WrapPanel panel && panel.Children.Count == 0)
            {
                IEnumerable<KeyValuePair<string, Color>> data = null;
                if (Equals(tc.SelectedItem, BootstrapItem))
                    data = GetBootstrapColors();
                else if (Equals(tc.SelectedItem, KnownColorsByColorItem))
                    data = ColorUtils.GetKnownColors(false).OrderBy(kvp => GetSortNumberForColor(kvp.Value));
                else if (Equals(tc.SelectedItem, KnownColorsByNameItem))
                    data = ColorUtils.GetKnownColors(false).OrderBy(kvp => kvp.Key);

                if (data != null)
                    foreach (var kvp in data.Where(kvp=> kvp.Value != Colors.Transparent))
                    {
                        var content = new TextBox
                        {
                            BorderThickness = new Thickness(0),
                            Margin = new Thickness(0),
                            Padding = new Thickness(0),
                            IsReadOnly = true,
                            Text = GetColorLabel(kvp),
                            Background = Brushes.Transparent,
                            Foreground = new SolidColorBrush(ColorUtils.GetForegroundColor(kvp.Value)),
                            IsHitTestVisible = false
                        };
                        var btn = new Button
                        {
                            Width = 140,
                            Padding = new Thickness(2),
                            Margin = new Thickness(2),
                            BorderThickness = new Thickness(2),
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Content = content
                        };
                        CornerRadiusEffect.SetCornerRadius(btn, new CornerRadius(2));
                        ChromeEffect.SetMonochrome(btn, kvp.Value);
                        ChromeEffect.SetChromeMatrix(btn, "+0%,+70%,+0%,40, +0%,+75%,+0%,100, +0%,+75%,+35%,100");
                        btn.Click += (o, args) => VM.CurrentColor = ((SolidColorBrush)((Button)o).Background).Color;
                        panel.Children.Add(btn);
                    }
            }
        }

        private static string GetColorLabel(KeyValuePair<string, Color> kvp)
        {
            var s1 = string.Concat(kvp.Key.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            var rgb = new RGB(kvp.Value);
            var hsl = new HSL(rgb);

            return $"{s1}{Environment.NewLine}{rgb.Color}{Environment.NewLine}HSL: {Math.Round(hsl.Hue)}, {Math.Round(hsl.Saturation)}, {Math.Round(hsl.Lightness)}";
        }

        private static int GetSortNumberForColor(Color color)
        {
            var hsl = new HSL(new RGB(color));
            return Convert.ToInt32(hsl.Hue) * 36000 + Convert.ToInt32(hsl.Saturation) * 100 + Convert.ToInt32(hsl.Lightness);
        }


        private static Dictionary<string, Color> GetBootstrapColors()
        {
            string[] bootstrapColorNames =
            {
                "PrimaryColor", "SecondaryColor", "SuccessColor", "DangerColor", "WarningColor", "InfoColor", "LightColor",
                "DarkColor", "BlueColor", "IndigoColor", "PurpleColor", "PinkColor", "RedColor", "OrangeColor",
                "YellowColor", "GreenColor", "TealColor", "CyanColor", "WhiteColor", "GrayColor", "GrayDarkColor"
            };
            var result = new Dictionary<string, Color>();
            foreach (var name in bootstrapColorNames)
                result.Add(name.Remove(name.Length - 5), (Color)Application.Current.Resources[name]);
            return result;
        }

        private void AAB_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.Print($"Click");
        }
    }
}
