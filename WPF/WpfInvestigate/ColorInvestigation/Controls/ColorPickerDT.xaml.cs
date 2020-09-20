// ToDo:
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
            VM.AfterUpdatedCallback = UpdateUI;
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
            foreach (var component in VM.Components)
            {
                var contentControl = Tips.GetVisualChildren(this).OfType<ContentControl>().FirstOrDefault(c => c.Content == component);
                if (contentControl != null)
                    SetSliderSizesInComponent(component, Tips.GetVisualChildren(contentControl).OfType<Canvas>().FirstOrDefault());
            }

            SetSliderSizesInComponent(VM.Components[15], AlphaSlider);
            SetSliderSizesInComponent(VM.Components[16], HueSlider);

            VM.UpdateProperties();
        }

        private void SetSliderSizesInComponent(ColorPickerVM.ColorComponent component, Panel panel)
        {
            var thumb = panel.Children[0] as FrameworkElement;
            if (thumb is Grid)
            {
                component.SliderControlSize = panel.ActualHeight;
                component.SliderControlOffset = thumb.ActualHeight / 2;
            }
            else
            {
                component.SliderControlSize = panel.ActualWidth - thumb.ActualWidth;
                component.SliderControlOffset = 0;
            }
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
            var bindingExpression = valueEditor.GetBindingExpression(TextBox.TextProperty);
            var propertyName = bindingExpression.ParentBinding.Path.Path;
            var metaData = ColorPickerVM.Metadata[propertyName];
            var newText = valueEditor.Text.Substring(0, valueEditor.SelectionStart) + e.Text +
                          valueEditor.Text.Substring(valueEditor.SelectionStart + valueEditor.SelectionLength);
            if (VM.CurrentCulture.NumberFormat.NativeDigits.Contains(e.Text))
                e.Handled = false;
            if (VM.CurrentCulture.NumberFormat.NumberDecimalSeparator == e.Text)
                e.Handled = valueEditor.Text.Contains(VM.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            else if (VM.CurrentCulture.NumberFormat.NegativeSign == e.Text)
                e.Handled = metaData.Min >= 0 ||
                            valueEditor.Text.Contains(VM.CurrentCulture.NumberFormat.NegativeSign) ||
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
            VM.CurrentColor = hsl.GetRGB().GetColor();

            VM.SetCC_Old(3, hsl.H, hsl.S, hsl.L);

            VM.SetProperty(hsl.H * 360, "HSL_H");
        }
        #endregion


        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            while (element != null && !(element is Popup))
                element = element.Parent as FrameworkElement;
            if (element is Popup popup)
                popup.IsOpen = false;
        }

        private void ColorBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = (Grid)((FrameworkElement)sender).Parent;
            var popup = (Popup)grid.Children[1];
            // popup.PlacementTarget = grid.Children[0];
            popup.IsOpen = true;
        }

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

        private void xxSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = sender as Panel;
                var thumb = canvas.Children[0] as FrameworkElement;
                var isVertical = thumb is Grid;
                var sliderName = (canvas.TemplatedParent as FrameworkElement)?.Name ?? canvas.Name;

                var offset = isVertical ? e.GetPosition(canvas).Y : e.GetPosition(canvas).X;
                var value = isVertical ? offset / canvas.ActualHeight : (offset - thumb.ActualWidth / 2) / (canvas.ActualWidth - thumb.ActualWidth);
                value = Math.Max(0, Math.Min(1, value));

                if (sliderName == nameof(HueSlider))
                    VM.HSV_H = GetModelValueBySlider("HSV_H", value);
                else if (canvas.Name == nameof(AlphaSlider))
                    VM.Alpha = 1.0 - value;
                else
                {
                    var valueId = sliderName.Replace("Slider_", "");
                    VM.SetProperty(GetModelValueBySlider(valueId, value), valueId);
                }
            }
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = sender as Panel;
                var thumb = canvas.Children[0] as FrameworkElement;
                var isVertical = thumb is Grid;
                var offset = isVertical ? e.GetPosition(canvas).Y : e.GetPosition(canvas).X;
                var value = isVertical ? offset / canvas.ActualHeight : (offset - thumb.ActualWidth / 2) / (canvas.ActualWidth - thumb.ActualWidth);
                value = Math.Max(0, Math.Min(1, value));
                ((ColorPickerVM.ColorComponent) canvas.DataContext).SetSliderValue(value);
            }
        }

        private void SaturationAndValueSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = sender as Canvas;
                var x = e.GetPosition(canvas).X;
                var y = e.GetPosition(canvas).Y;
                x = Math.Max(0, Math.Min(x, canvas.ActualWidth));
                y = Math.Max(0, Math.Min(y, canvas.ActualHeight));

                VM.HSV_S_And_V = new Tuple<double, double>(GetModelValueBySlider("HSV_S", x / canvas.ActualWidth),
                    GetModelValueBySlider("HSV_V", 1 - y / canvas.ActualHeight));
            }
        }
        #endregion

        #region =================  UpdateUI  =================
        private void UpdateUI()
        {
            // UpdateSlider(SaturationAndValueSlider, GetSliderValueByModel("HSV_S"), 1.0 - GetSliderValueByModel("HSV_V"));
            // UpdateSlider(HueSlider, null, GetSliderValueByModel("HSV_H"));
            // UpdateSlider(AlphaSlider, null, 1.0 - VM.Alpha);

            // foreach (var kvp in ColorPickerVM.Metadata)
               //  UpdateSlider(FindName("Slider_" + kvp.Key) as FrameworkElement, GetSliderValueByModel(kvp.Key), null);
        }
        #endregion

        #region =============  Update sliders  =============
        private double GetModelValueBySlider(string componentName, double sliderValue)
        {
            var meta = ColorPickerVM.Metadata[componentName];
            return (meta.Max - meta.Min) * sliderValue + meta.Min;
        }
        private double GetSliderValueByModel(string componentName)
        {
            var meta = ColorPickerVM.Metadata[componentName];
            return (meta.GetValue(VM) - meta.Min) / (meta.Max - meta.Min);
        }
        private void UpdateSlider(FrameworkElement element, double? xValue, double? yValue)
        {
            var panel = element as Panel;
            if (panel == null)
            {
                if (VisualTreeHelper.GetChildrenCount(element) > 0)
                    panel = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(element, 0), 0) as Panel;
                else
                    return;
            }

            var thumb = panel.Children[0] as FrameworkElement;
            if (xValue.HasValue)
            {
                var x = thumb is Grid
                    ? panel.ActualWidth * xValue.Value - thumb.ActualWidth / 2
                    : (panel.ActualWidth - thumb.ActualWidth) * xValue.Value;
                Canvas.SetLeft(thumb, x);
            }
            if (yValue.HasValue)
            {
                var y = panel.ActualHeight * yValue.Value - thumb.ActualHeight / 2;
                Canvas.SetTop(thumb, y);
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
