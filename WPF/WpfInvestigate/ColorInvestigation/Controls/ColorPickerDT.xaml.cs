﻿// ToDo:
// +1. fixed mouse move on HSV area
// +2. Layout
// +3. Remove all dependency properties + use calculated properties
//      in setter set variable _isChanging=true
//      public properties are Color type (not Brush)
//      ? old/current Color
//      add rgb tuple to ColorUtilities
// ?-4. Separate component: ColorComponentSlider
//      try use calculated properties
// +5. StartUp color
// +6. Remove IsMouseDown property (only Capture)
// ?7. Process value when mouse is clicked
// +8. Size changed => how Hue don't change
// 9. Known color: show name + hex (as popup)
// +10. Tones
// +11. Binding to dictionary
// +12. Edited text box for CurrentColor
// +13. Degree label for Hue
// -14. Control for slider
// -?15. ViewModel
// +16. Click on ColorBox
// 17. Alpha for old Color in ColorBox
// +18. Popup for ColorBox info
// +19. Decimal places for ValueEditor
// - performance 20. Component Slider - change black/white color
// - performance21. Component slider - like triangle
// +22. Производительность - sliders:
//     + - increase step (try ~20 gradient) 
//      -or/and process only last mouse move / skip late mouse moves
// 23. ViewModel + з прямими get/set (без формул) - ? propertyUpdate + getter caller name => CallerMemberName
// 24. Remove Components, inline initilize variables RBR_R, ...
// 25. ColorComponent as subclass of XYSlider

using ColorInvestigation.Common;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorInvestigation.Controls
{
    /// <summary>
    /// Interaction logic ColorPickerDT.xaml
    /// </summary>
    public partial class ColorPickerDT : UserControl, INotifyPropertyChanged
    {
        private ColorPickerVM VM => (ColorPickerVM)DataContext;

        // Constructor
        public ColorPickerDT()
        {
            InitializeComponent();
            VM.Owner = this;
        }

        #region  ==============  User UI  ===========
        public Color Color  // Original color
        {
            get => VM.Color;
            set => VM.Color = value;
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
            if (fe.Name == "LeftPanel")
            {
                VM.AlphaSlider.SetSizeOfControl(AlphaSlider);
                VM.HueSlider.SetSizeOfControl(HueSlider);
                VM.SaturationAndValueSlider.SetSizeOfControl(SaturationAndValueSlider);
            }
            else
            {
                foreach (var cc in Tips.GetVisualChildren(this).OfType<ContentControl>()
                    .Where(cc => cc.Content is ColorPickerVM.ColorComponent))
                    ((ColorPickerVM.ColorComponent) cc.Content).SetSizeOfControl(Tips.GetVisualChildren(cc).OfType<Canvas>().FirstOrDefault());
            }

            VM.UpdateUI();
        }
        #endregion

        #region ================  SelectAll Event Handlers on Focus event  ===============
        private void ValueEditor_OnGotFocus(object sender, RoutedEventArgs e) => ((TextBox)sender).SelectAll();

        private void ValueEditor_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // select all text on got focus: see BillBR comment in https://stackoverflow.com/questions/660554/how-to-automatically-select-all-text-on-focus-in-wpf-textbox
            var textBox = (TextBox)sender;
            if (!textBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                textBox.Focus();
            }
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
                e.Handled = ((ColorPickerVM.ColorComponent) valueEditor.DataContext).Min >= 0 ||
                            (newText.Length > 1 && newText.Substring(1, newText.Length - 2)
                                 .Contains(VM.CurrentCulture.NumberFormat.NegativeSign)) ||
                            !(newText.StartsWith(e.Text) || newText.EndsWith(e.Text));

            if (e.Handled)
                Tips.Beep();
        }
        #endregion

        #region ===============  Color box event handlers  ===============
        private void ColorBoxPopup_OnOpened(object sender, EventArgs e)
        {
            var textBox = Tips.GetVisualChildren(((Popup)sender).Child).OfType<TextBox>().FirstOrDefault();
            textBox.Text = (textBox.DataContext as ColorPickerVM.ColorToneBox).Info;
            textBox.Focus();
        }

        private void ColorBox_OnSetColor(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var toggleButton = Tips.GetVisualParents(element).OfType<Grid>().SelectMany(grid => grid.Children.OfType<ToggleButton>()).FirstOrDefault();
            toggleButton.IsChecked = false;

            var hsl = (element.DataContext as ColorPickerVM.ColorToneBox).GetBackgroundHSL();
            VM.SetCC(3, hsl.H, hsl.S, hsl.L);
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
                if (canvas.DataContext is ColorPickerVM.XYSlider)
                {
                    var x = Math.Max(0, Math.Min(1.0, e.GetPosition(canvas).X / canvas.ActualWidth));
                    var y = Math.Max(0, Math.Min(1.0, e.GetPosition(canvas).Y / canvas.ActualHeight));
                    ((ColorPickerVM.XYSlider) canvas.DataContext).SetValuesAction?.Invoke(x, y);
                }
                else
                {
                    var isVertical = thumb is Grid;
                    var value = isVertical
                        ? e.GetPosition(canvas).Y / canvas.ActualHeight
                        : (e.GetPosition(canvas).X - thumb.ActualWidth / 2) / (canvas.ActualWidth - thumb.ActualWidth);
                    value = Math.Max(0, Math.Min(1, value));
                    // ((ColorPickerVM.ColorComponent)canvas.DataContext).SetSliderValue(value);
                    ((ColorPickerVM.ColorComponent)canvas.DataContext).SetProperties(value);
                }
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            // Debug.Print($"ButtonBase_OnClick");
        }
    }
}