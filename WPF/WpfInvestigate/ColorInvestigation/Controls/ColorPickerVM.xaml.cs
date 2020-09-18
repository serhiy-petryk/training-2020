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
        private enum UpdateMode { RGB, HSL, HSV, XYZ, LAB, YCbCr };

        // Color space values must be independent => we need to have separate object for each color space 
        private double _alpha = 1.0;
        private ColorSpaces.RGB _rgb { get; set; } = new ColorSpaces.RGB(0, 0, 0);
        private ColorSpaces.HSL _hsl = new ColorSpaces.HSL(0, 0, 0);
        private ColorSpaces.HSV _hsv = new ColorSpaces.HSV(0, 0, 0);
        private ColorSpaces.XYZ _xyz = new ColorSpaces.XYZ(0, 0, 0);
        private ColorSpaces.LAB _lab = new ColorSpaces.LAB(0, -127.5, -127.5);
        private ColorSpaces.YCbCr _yCbCr = new ColorSpaces.YCbCr(0, 0, 0);

        private ColorSpaces.RGB _oldRgb = new ColorSpaces.RGB(0, 0, 0);
        private double _oldAlpha = 1.0;

        private Color _savedColor;


        // Calculated properties
        private readonly SolidColorBrush _hueBrush = new SolidColorBrush();
        public Brush HueBrush
        {
            get
            {
                _hueBrush.Color = new ColorSpaces.HSV(_hsv.H, 1, 1).GetRGB().GetColor();
                return _hueBrush;
            }
        }
        private SolidColorBrush[] _brushesCache = { new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush() };
        public SolidColorBrush Color_ForegroundBrush
        {
            get
            {
                _brushesCache[0].Color = ColorSpaces.IsDarkColor(new ColorSpaces.RGB(Color)) ? Colors.White : Colors.Black;
                return _brushesCache[0];
            }
        }
        public SolidColorBrush CurrentColor_ForegroundBrush
        {
            get
            {
                _brushesCache[1].Color = ColorSpaces.IsDarkColor(new ColorSpaces.RGB(VM.CurrentColor)) ? Colors.White : Colors.Black;
                return _brushesCache[1];
            }
        }
        public SolidColorBrush CurrentColorWithoutAlphaBrush
        {
            get
            {
                _brushesCache[2].Color = _rgb.GetColor();
                return _brushesCache[2];
            }
        }

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

        #region ===========  Linear gradient brushes  ====================
        public LinearGradientBrush Brush_RGB_R { get; } = CreateLinearGradientBrush(1);
        public LinearGradientBrush Brush_RGB_G { get; } = CreateLinearGradientBrush(1);
        public LinearGradientBrush Brush_RGB_B { get; } = CreateLinearGradientBrush(1);
        public LinearGradientBrush Brush_HSL_H { get; } = CreateLinearGradientBrush(360);
        public LinearGradientBrush Brush_HSL_S { get; } = CreateLinearGradientBrush(100);
        public LinearGradientBrush Brush_HSL_L { get; } = CreateLinearGradientBrush(100);
        public LinearGradientBrush Brush_HSV_H { get; } = CreateLinearGradientBrush(360);
        public LinearGradientBrush Brush_HSV_S { get; } = CreateLinearGradientBrush(100);
        public LinearGradientBrush Brush_HSV_V { get; } = CreateLinearGradientBrush(100);
        public LinearGradientBrush Brush_LAB_L { get; } = CreateLinearGradientBrush(100);
        public LinearGradientBrush Brush_LAB_A { get; } = CreateLinearGradientBrush(255);
        public LinearGradientBrush Brush_LAB_B { get; } = CreateLinearGradientBrush(255);
        public LinearGradientBrush Brush_YCbCr_Y { get; } = CreateLinearGradientBrush(255);
        public LinearGradientBrush Brush_YCbCr_Cb { get; } = CreateLinearGradientBrush(255);
        public LinearGradientBrush Brush_YCbCr_Cr { get; } = CreateLinearGradientBrush(255);

        private static LinearGradientBrush CreateLinearGradientBrush(int gradientCount) => new LinearGradientBrush(
            new GradientStopCollection(Enumerable.Range(0, gradientCount + 1)
                .Select(n => new GradientStop(Colors.Transparent, 1.0 * n / gradientCount))));

        private void UpdateRgbBrushes()
        {
            Brush_RGB_R.GradientStops[0].Color = Color.FromRgb(0, VM.CurrentColor.G, VM.CurrentColor.B);
            Brush_RGB_R.GradientStops[1].Color = Color.FromRgb(0xFF, VM.CurrentColor.G, VM.CurrentColor.B);

            Brush_RGB_G.GradientStops[0].Color = Color.FromRgb(VM.CurrentColor.R, 0, VM.CurrentColor.B);
            Brush_RGB_G.GradientStops[1].Color = Color.FromRgb(VM.CurrentColor.R, 0xFF, VM.CurrentColor.B);

            Brush_RGB_B.GradientStops[0].Color = Color.FromRgb(VM.CurrentColor.R, VM.CurrentColor.G, 0);
            Brush_RGB_B.GradientStops[1].Color = Color.FromRgb(VM.CurrentColor.R, VM.CurrentColor.G, 0xFF);

            OnPropertiesChanged(nameof(Brush_RGB_R), nameof(Brush_RGB_G), nameof(Brush_RGB_B));
        }

        private void UpdateHslBrushes()
        {
            /*for (var i = 0; i <= 360; i++)
                Brush_HSL_H.GradientStops[i].Color = new ColorSpaces.HSL(i / 360.0, _hsl.S, _hsl.L).GetRGB().GetColor();
            for (var i = 0; i <= 100; i++)
            {
                Brush_HSL_S.GradientStops[i].Color = new ColorSpaces.HSL(_hsl.H, i / 100.0, _hsl.L).GetRGB().GetColor();
                Brush_HSL_L.GradientStops[i].Color = new ColorSpaces.HSL(_hsl.H, _hsl.S, i / 100.0).GetRGB().GetColor();
            }
            OnPropertiesChanged(nameof(Brush_HSL_H), nameof(Brush_HSL_S), nameof(Brush_HSL_L));*/
        }

        private void UpdateHsvBrushes()
        {
            for (var i = 0; i <= 360; i++)
                Brush_HSV_H.GradientStops[i].Color = new ColorSpaces.HSV(i / 360.0, _hsv.S, _hsv.V).GetRGB().GetColor();
            for (var i = 0; i <= 100; i++)
            {
                Brush_HSV_S.GradientStops[i].Color = new ColorSpaces.HSV(_hsv.H, i / 100.0, _hsv.V).GetRGB().GetColor();
                Brush_HSV_V.GradientStops[i].Color = new ColorSpaces.HSV(_hsv.H, _hsv.S, i / 100.0).GetRGB().GetColor();
            }
            OnPropertiesChanged(nameof(Brush_HSV_H), nameof(Brush_HSV_S), nameof(Brush_HSV_V));
        }

        private void UpdateLabBrushes()
        {
            for (var i = 0; i <= 100; i++)
                Brush_LAB_L.GradientStops[i].Color = new ColorSpaces.LAB(i, _lab.A, _lab.B).GetRGB().GetColor();
            for (var i = 0; i <= 255; i++)
            {
                Brush_LAB_A.GradientStops[i].Color = new ColorSpaces.LAB(_lab.L, i - 127.5, _lab.B).GetRGB().GetColor();
                Brush_LAB_B.GradientStops[i].Color = new ColorSpaces.LAB(_lab.L, _lab.A, i - 127.5).GetRGB().GetColor();
            }
            OnPropertiesChanged(nameof(Brush_LAB_L), nameof(Brush_LAB_A), nameof(Brush_LAB_B));
        }

        private void UpdateYCbCrBrushes()
        {
            for (var i = 0; i <= 255; i++)
            {
                Brush_YCbCr_Y.GradientStops[i].Color = new ColorSpaces.YCbCr(i / 255.0, _yCbCr.Cb, _yCbCr.Cr).GetRGB().GetColor();
                Brush_YCbCr_Cb.GradientStops[i].Color = new ColorSpaces.YCbCr(_yCbCr.Y, (i - 127.5) / 255, _yCbCr.Cr).GetRGB().GetColor();
                Brush_YCbCr_Cr.GradientStops[i].Color = new ColorSpaces.YCbCr(_yCbCr.Y, _yCbCr.Cb, (i - 127.5) / 255).GetRGB().GetColor();
            }
            OnPropertiesChanged(nameof(Brush_YCbCr_Y), nameof(Brush_YCbCr_Cb), nameof(Brush_YCbCr_Cr));
        }

        #endregion

        #region ===============  TextBox values  ===================

        private static readonly Dictionary<string, Tuple<double, double, UpdateMode>> ValueMetadata =
            new Dictionary<string, Tuple<double, double, UpdateMode>>
            {
                {nameof(Value_RGB_R), Tuple.Create(0.0, 255.0, UpdateMode.RGB)},
                {nameof(Value_RGB_G), Tuple.Create(0.0, 255.0, UpdateMode.RGB)},
                {nameof(Value_RGB_B), Tuple.Create(0.0, 255.0, UpdateMode.RGB)},
                {nameof(Value_HSL_H), Tuple.Create(0.0, 360.0, UpdateMode.HSL)},
                {nameof(Value_HSL_S), Tuple.Create(0.0, 100.0, UpdateMode.HSL)},
                {nameof(Value_HSL_L), Tuple.Create(0.0, 100.0, UpdateMode.HSL)},
                {nameof(Value_HSV_H), Tuple.Create(0.0, 360.0, UpdateMode.HSV)},
                {nameof(Value_HSV_S), Tuple.Create(0.0, 100.0, UpdateMode.HSV)},
                {nameof(Value_HSV_V), Tuple.Create(0.0, 100.0, UpdateMode.HSV)},
                {nameof(Value_LAB_L), Tuple.Create(0.0, 100.0, UpdateMode.LAB)},
                {nameof(Value_LAB_A), Tuple.Create(-127.5, 127.5, UpdateMode.LAB)},
                {nameof(Value_LAB_B), Tuple.Create(-127.5, 127.5, UpdateMode.LAB)},
                {nameof(Value_YCbCr_Y), Tuple.Create(0.0, 255.0, UpdateMode.YCbCr)},
                {nameof(Value_YCbCr_Cb), Tuple.Create(-127.5, 127.5, UpdateMode.YCbCr)},
                {nameof(Value_YCbCr_Cr), Tuple.Create(-127.5, 127.5, UpdateMode.YCbCr)}
            };

        // RGB
        public double Value_RGB_R
        {
            get => _rgb.R * 255;
            set => _rgb.R = value / 255.0;
        }
        public double Value_RGB_G
        {
            get => _rgb.G * 255;
            set => _rgb.G = value / 255.0;
        }
        public double Value_RGB_B
        {
            get => _rgb.B * 255;
            set => _rgb.B = value / 255.0;
        }

        // HSL
        public double Value_HSL_H
        {
            get => _hsl.H * 360;
            set => _hsl.H = value / 360.0;
        }
        public double Value_HSL_S
        {
            get => _hsl.S * 100;
            set => _hsl.S = value / 100.0;
        }
        public double Value_HSL_L
        {
            get => _hsl.L * 100;
            set => _hsl.L = value / 100.0;
        }

        // HSV
        public double Value_HSV_H
        {
            get => _hsv.H * 360;
            set => _hsv.H = value / 360.0;
        }
        public double Value_HSV_S
        {
            get => _hsv.S * 100;
            set => _hsv.S = value / 100.0;
        }
        public double Value_HSV_V
        {
            get => _hsv.V * 100;
            set => _hsv.V = value / 100.0;
        }

        // LAB
        public double Value_LAB_L
        {
            get => _lab.L;
            set => _lab.L = value;
        }
        public double Value_LAB_A
        {
            get => _lab.A;
            set => _lab.A = value;
        }
        public double Value_LAB_B
        {
            get => _lab.B;
            set => _lab.B = value;
        }

        // YCbCr
        public double Value_YCbCr_Y
        {
            get => _yCbCr.Y * 255.0;
            set => _yCbCr.Y = value / 255.0;
        }
        public double Value_YCbCr_Cb
        {
            get => _yCbCr.Cb * 255.0;
            set => _yCbCr.Cb = value / 255.0;
        }

        public double Value_YCbCr_Cr
        {
            get => _yCbCr.Cr * 255.0;
            set => _yCbCr.Cr = value / 255.0;
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
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                // ? need to check (after color changed) VM.UpdateUI();
            }));
        }

        // public Color CurrentColor => VM.CurrentColor;

        // Original color
        public Color Color
        {
            get => VM.Color;
            set => VM.Color = value;
        }

        public void SaveColor() => VM.SaveColor();
        public void RestoreColor() => VM.RestoreColor();

        #region  =============  Slider event handlers  =====================
        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).CaptureMouse();
            Keyboard.ClearFocus();
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e) => (sender as UIElement).ReleaseMouseCapture();

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
