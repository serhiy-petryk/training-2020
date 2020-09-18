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
// 15. ViewModel
// +16. Click on ColorBox
// 17. Alpha for old Color in ColorBox
// 18. Popup for ColorBox info
// +19. Decimal places for ValueEditor

using ColorInvestigation.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ColorInvestigation.Controls
{
    /// <summary>
    /// Interaction logic ColorPickerVM.xaml
    /// </summary>
    public partial class ColorPickerVM : UserControl, INotifyPropertyChanged
    {
        //=================================
        private CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;

        private ColorSpaces.HSL _hsl = new ColorSpaces.HSL(0, 0, 0);

        #region ==============  Event handlers  ====================
        private void Control_OnSizeChanged(object sender, SizeChangedEventArgs e) => VM.UpdateUI();
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
            var metaData = ColorPickerViewModel.Metadata[propertyName];
            var newText = valueEditor.Text.Substring(0, valueEditor.SelectionStart) + e.Text +
                          valueEditor.Text.Substring(valueEditor.SelectionStart + valueEditor.SelectionLength);
            if (CurrentCulture.NumberFormat.NativeDigits.Contains(e.Text))
                e.Handled = false;
            if (CurrentCulture.NumberFormat.NumberDecimalSeparator == e.Text)
                e.Handled = valueEditor.Text.Contains(CurrentCulture.NumberFormat.NumberDecimalSeparator);
            else if (CurrentCulture.NumberFormat.NegativeSign == e.Text)
                e.Handled = metaData.Min >= 0 ||
                            valueEditor.Text.Contains(CurrentCulture.NumberFormat.NegativeSign) ||
                            !(newText.StartsWith(e.Text) || newText.EndsWith(e.Text));

            if (e.Handled)
                Tips.Beep();
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

        #region ==============  Tones  =======================
        public class ColorToneBox
        {
            public int GridColumn { get; }
            public int GridRow { get; }
            public SolidColorBrush Background { get; } = new SolidColorBrush();
            public string BackgroundText => Background.Color.ToString();
            public SolidColorBrush Foreground { get; } = new SolidColorBrush();
            public string Info { get; set; }

            public ColorToneBox(int gridColumn, int gridRow)
            {
                GridColumn = gridColumn;
                GridRow = gridRow;
                Foreground.Color = gridColumn == 0 ? Colors.White : Colors.Black;
            }

            public ColorSpaces.HSL GetBackgroundHSL(ColorSpaces.HSL baseHSL)
            {
                if (GridColumn == 0)
                    return new ColorSpaces.HSL(baseHSL.H, baseHSL.S, 0.025 + 0.05 * GridRow);
                if (GridColumn == 1)
                    return new ColorSpaces.HSL(baseHSL.H, baseHSL.S, 0.975 - 0.05 * GridRow);
                return new ColorSpaces.HSL(baseHSL.H, 0.05 + 0.1 * GridRow, baseHSL.L);
            }
        }

        private const int NumberOfTones = 10;
        public ColorToneBox[] Tones { get; private set; }

        private void TonesGenerate()
        {
            if (Tones == null)
            {
                Tones = new ColorToneBox[3 * NumberOfTones];
                for (var k1 = 0; k1 < 3; k1++)
                {
                    for (var k2 = 0; k2 < NumberOfTones; k2++)
                        Tones[k2 + k1 * NumberOfTones] = new ColorToneBox(k1, k2);
                }
            }

            foreach (var tone in Tones)
            {
                tone.Background.Color = tone.GetBackgroundHSL(_hsl).GetRGB().GetColor();
                if (tone.GridColumn == 2)
                    tone.Foreground.Color = ColorSpaces.IsDarkColor(new ColorSpaces.RGB(tone.Background.Color))
                        ? Colors.White
                        : Colors.Black;
            }

            OnPropertiesChanged(nameof(Tones));
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

        #region ===============  NEW CODE  ================

        private ColorPickerViewModel VM => (ColorPickerViewModel)DataContext;
        // Constructor
        public ColorPickerVM()
        {
            InitializeComponent();
            VM.PropertiesUpdated += ViewModel_PropertiesUpdated;
        }

        // Original color
        public Color Color
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

        #region =================  Properties Updated  =================
        private void ViewModel_PropertiesUpdated(object sender, EventArgs e)
        {
            UpdateSlider(SaturationAndValueSlider, GetSliderValueByModel("HSV_S"), 1.0 - GetSliderValueByModel("HSV_V"));
            UpdateSlider(HueSlider, null, GetSliderValueByModel("HSV_H"));
            UpdateSlider(AlphaSlider, null, 1.0 - VM.Alpha);

            foreach (var kvp in ColorPickerViewModel.Metadata)
                UpdateSlider(FindName("Slider_" + kvp.Key) as FrameworkElement, GetSliderValueByModel(kvp.Key), null);

            // UpdateSliderBrushes();
        }


        private double GetModelValueBySlider(string componentName, double sliderValue)
        {
            var meta = ColorPickerViewModel.Metadata[componentName];
            return (meta.Max - meta.Min) * sliderValue + meta.Min;
        }
        private double GetSliderValueByModel(string componentName)
        {
            var meta = ColorPickerViewModel.Metadata[componentName];
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

        #endregion

    }
}
