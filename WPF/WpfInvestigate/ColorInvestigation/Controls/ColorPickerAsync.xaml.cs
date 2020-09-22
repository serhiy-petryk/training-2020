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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
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
    /// Interaction logic ColorPickerAsync.xaml
    /// </summary>
    public partial class ColorPickerAsync : UserControl, INotifyPropertyChanged
    {
        // Constructor
        public ColorPickerAsync()
        {
            InitializeComponent();
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                UpdateValues(ColorSpace.RGB);
                _savedColor = _oldRgb.GetColor(_oldAlpha);
            }));
        }

        //=================================
        private CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;
        public enum ColorSpace { RGB, HSL, HSV, XYZ, LAB, YCbCr };

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
        public Color CurrentColor
        {
            get => _rgb.GetColor(_alpha);
            set
            {
                _alpha = value.A / 255.0;
                _rgb = new ColorSpaces.RGB(value);
                UpdateValues(ColorSpace.RGB);
            }
        }

        private SolidColorBrush[] _brushesCache = { new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush() };
        public SolidColorBrush Color_ForegroundBrush
        {
            get
            {
                _brushesCache[0].Color = ColorSpaces.GetForegroundColor(Color);
                return _brushesCache[0];
            }
        }
        public SolidColorBrush ColorWithoutAlphaBrush
        {
            get
            {
                _brushesCache[1].Color = _rgb.GetColor();
                return _brushesCache[1];
            }
        }
        public SolidColorBrush CurrentColor_ForegroundBrush
        {
            get
            {
                _brushesCache[2].Color = ColorSpaces.GetForegroundColor(CurrentColor);
                return _brushesCache[2];
            }
        }
        public SolidColorBrush CurrentColorWithoutAlphaBrush
        {
            get
            {
                _brushesCache[3].Color = _rgb.GetColor();
                return _brushesCache[3];
            }
        }

        // Original color
        public Color Color
        {
            get => _oldRgb.GetColor(_oldAlpha);
            set
            {
                _rgb = new ColorSpaces.RGB(value);
                _alpha = value.A / 255.0;
                SaveColor();
                UpdateUI();
            }
        }

        // =========================
        public void SaveColor()
        {
            _savedColor = _oldRgb.GetColor(_oldAlpha);
            _oldRgb = new ColorSpaces.RGB(_rgb.GetColor());
            _oldAlpha = _alpha;
            OnPropertiesChanged(nameof(Color), nameof(Color_ForegroundBrush), nameof(ColorWithoutAlphaBrush));
        }

        public void RestoreColor()
        {
            _oldRgb = new ColorSpaces.RGB(_savedColor);
            _oldAlpha = _savedColor.A / 255.0;
            _rgb = new ColorSpaces.RGB(_savedColor);
            _alpha = _savedColor.A / 255.0;
            UpdateValues(ColorSpace.RGB);
            OnPropertiesChanged(nameof(Color), nameof(Color_ForegroundBrush), nameof(ColorWithoutAlphaBrush));
        }

        private bool _isUpdating;
        private void UpdateValues(ColorSpace colorSpace)
        {
            if (_isUpdating) return;

            // Update rgb object
            if (colorSpace == ColorSpace.HSL)
            {
                _rgb = _hsl.GetRGB();
                _hsv = new ColorSpaces.HSV(_rgb);
                _hsv.H = _hsl.H;
            }
            else if (colorSpace == ColorSpace.HSV)
            {
                _rgb = _hsv.GetRGB();
                _hsl = new ColorSpaces.HSL(_rgb);
                _hsl.H = _hsv.H;
            }
            else if (colorSpace == ColorSpace.XYZ)
            {
                _rgb = _xyz.GetRGB();
                _lab = new ColorSpaces.LAB(_xyz);
            }
            else if (colorSpace == ColorSpace.LAB)
            {
                _rgb = _lab.GetRGB();
                _xyz = _lab.GetXYZ();
            }
            else if (colorSpace == ColorSpace.YCbCr)
                _rgb = _yCbCr.GetRGB();

            // Update other objects
            if (colorSpace != ColorSpace.HSL && colorSpace != ColorSpace.HSV)
            {
                _hsl = new ColorSpaces.HSL(_rgb);
                _hsv = new ColorSpaces.HSV(_rgb);
            }
            if (colorSpace != ColorSpace.XYZ && colorSpace != ColorSpace.LAB)
            {
                _xyz = new ColorSpaces.XYZ(_rgb);
                _lab = new ColorSpaces.LAB(_rgb);
            }
            if (colorSpace != ColorSpace.YCbCr)
                _yCbCr = new ColorSpaces.YCbCr(_rgb);

            _isUpdating = true;
            UpdateUI();
            _isUpdating = false;
        }

        private void UpdateUI()
        {
            TonesGenerate();
            UpdateSliderBrushes();

            OnPropertiesChanged(nameof(CurrentColor), nameof(CurrentColor_ForegroundBrush),
                nameof(CurrentColorWithoutAlphaBrush), nameof(HueBrush), nameof(Brushes),
                nameof(Value_RGB_R), nameof(Value_RGB_G), nameof(Value_RGB_B),
                nameof(Value_HSL_H), nameof(Value_HSL_S), nameof(Value_HSL_L),
                nameof(Value_HSV_H), nameof(Value_HSV_S), nameof(Value_HSV_V),
                nameof(Value_LAB_L), nameof(Value_LAB_A), nameof(Value_LAB_B),
                nameof(Value_YCbCr_Y), nameof(Value_YCbCr_Cb), nameof(Value_YCbCr_Cr));

            UpdateSlider(SaturationAndValueSlider, _hsv.S, 1.0 - _hsv.V);
            UpdateSlider(HueSlider, null, _hsv.H);
            UpdateSlider(AlphaSlider, null, 1.0 - _alpha);

            foreach (var kvp in Properties)
                UpdateSlider(FindName("Slider_" + kvp.Key) as FrameworkElement, kvp.Value.SliderValue(this), null);
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

        #region ==============  Event handlers  ====================

        private void ColorPicker_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateUI();
        private void RightColumn_OnSizeChanged(object sender, SizeChangedEventArgs e) => UpdateUI();

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
                var isVertical = canvas.ActualHeight > canvas.ActualWidth;
                var sliderName = (canvas.TemplatedParent as FrameworkElement)?.Name ?? canvas.Name;
                var offset = isVertical ? e.GetPosition(canvas).Y : e.GetPosition(canvas).X;

                var value = isVertical ? offset / canvas.ActualHeight : (offset - thumb.ActualWidth / 2) / (canvas.ActualWidth - thumb.ActualWidth);
                value = Math.Max(0, Math.Min(1, value));

                if (sliderName == nameof(HueSlider))
                {
                    _hsv.H = value;
                    UpdateValues(ColorSpace.HSV);
                }
                else if (canvas.Name == nameof(AlphaSlider))
                {
                    _alpha = 1.0 - value;
                    UpdateUI();
                }
                else
                {
                    var property = Properties[sliderName.Replace("Slider_", "")];
                    property.MouseMoveAction(this, value);
                    UpdateValues(property.ColorSpace);
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

                _hsv.S = x / canvas.ActualWidth;
                _hsv.V = 1 - y / canvas.ActualHeight;
                UpdateValues(ColorSpace.HSV);
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
            var colorProperty = Properties[propertyName.Replace("Value_", "")];
            var newText = valueEditor.Text.Substring(0, valueEditor.SelectionStart) + e.Text +
                          valueEditor.Text.Substring(valueEditor.SelectionStart + valueEditor.SelectionLength);

            if (CurrentCulture.NumberFormat.NativeDigits.Contains(e.Text))
                e.Handled = false;
            if (CurrentCulture.NumberFormat.NumberDecimalSeparator == e.Text)
                e.Handled = valueEditor.Text.Contains(CurrentCulture.NumberFormat.NumberDecimalSeparator);
            else if (CurrentCulture.NumberFormat.NegativeSign == e.Text)
                e.Handled = colorProperty.Min >= 0 ||
                            valueEditor.Text.Contains(CurrentCulture.NumberFormat.NegativeSign) ||
                            !(newText.StartsWith(e.Text) || newText.EndsWith(e.Text));

            if (e.Handled)
                Tips.Beep();
        }

        private void ValueEditor_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var valueEditor = (TextBox)sender;
            var bindingExpression = valueEditor.GetBindingExpression(TextBox.TextProperty);
            var propertyName = bindingExpression.ParentBinding.Path.Path;
            var colorProperty = Properties[propertyName.Replace("Value_", "")];

            if (double.TryParse(valueEditor.Text, NumberStyles.Any, CurrentCulture, out var value))
            {
                if (value < colorProperty.Min) valueEditor.Text = colorProperty.Min.ToString(CurrentCulture);
                else if (value > colorProperty.Max) valueEditor.Text = colorProperty.Max.ToString(CurrentCulture);
            }
            else valueEditor.Text = "0";
            UpdateValues(colorProperty.ColorSpace);
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
            private readonly ColorPickerAsync _owner;
            public int GridColumn { get; }
            public int GridRow { get; }
            public SolidColorBrush Background { get; } = new SolidColorBrush();
            public SolidColorBrush Foreground { get; } = new SolidColorBrush();
            public string Info
            {
                get
                {
                    var rgb = GetBackgroundHSL().GetRGB();
                    var hsl = GetBackgroundHSL();
                    var hsv = new ColorSpaces.HSV(rgb) {H = hsl.H};
                    var lab = new ColorSpaces.LAB(rgb);
                    var yCbCr = new ColorSpaces.YCbCr(rgb);

                    var sb = new StringBuilder();
                    sb.AppendLine("HEX:".PadRight(5) + rgb.Color);
                    sb.AppendLine(FormatInfoString("RGB", rgb.R * 255, rgb.G * 255, rgb.B * 255));
                    sb.AppendLine(FormatInfoString("HSL", hsl.H * 360, hsl.S * 100, hsl.L * 100));
                    sb.AppendLine(FormatInfoString("HSV", hsv.H * 360, hsv.S * 100, hsv.V * 100));
                    sb.AppendLine(FormatInfoString("LAB", lab.L, lab.A, lab.B));
                    sb.Append(FormatInfoString("YCbCr", yCbCr.Y * 255, yCbCr.Cb * 255, yCbCr.Cr * 255));
                    return sb.ToString();
                }
            }

            public ColorToneBox(ColorPickerAsync owner, int gridColumn, int gridRow)
            {
                _owner = owner;
                GridColumn = gridColumn;
                GridRow = gridRow;
                Foreground.Color = gridColumn == 0 ? Colors.White : Colors.Black;
            }

            internal ColorSpaces.HSL GetBackgroundHSL()
            {
                if (GridColumn == 0)
                    return new ColorSpaces.HSL(_owner._hsl.H, _owner._hsl.S, 0.025 + 0.05 * GridRow);
                if (GridColumn == 1)
                    return new ColorSpaces.HSL(_owner._hsl.H, _owner._hsl.S, 0.975 - 0.05 * GridRow);
                return new ColorSpaces.HSL(_owner._hsl.H, 0.05 + 0.1 * GridRow, _owner._hsl.L);
            }

            private string FormatInfoString(string label, double value1, double value2, double value3) =>
                (label + ":").PadRight(7) + FormatDouble(value1) + FormatDouble(value2) + FormatDouble(value3);
            private string FormatDouble(double value) => value.ToString("F1", _owner.CurrentCulture).PadLeft(7);
        }

        private const int NumberOfTones = 10;
        public ColorToneBox[] Tones { get; private set; }

        private void TonesGenerate()
        {
            if (Tones == null)
            {
                Tones = new ColorToneBox[3 * NumberOfTones];
                for (var k1 = 0; k1 < 3; k1++)
                for (var k2 = 0; k2 < NumberOfTones; k2++)
                    Tones[k2 + k1 * NumberOfTones] = new ColorToneBox(this, k1, k2);
            }

            foreach (var tone in Tones)
            {
                tone.Background.Color = tone.GetBackgroundHSL().GetRGB().GetColor();
                if (tone.GridColumn == 2)
                    tone.Foreground.Color = ColorSpaces.GetForegroundColor(tone.Background.Color);
            }

            OnPropertiesChanged(nameof(Tones));
        }
        #endregion

        #region ===============  TextBox values  ===================
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

        #region ===============  Color box event handlers  ===============
        private void ColorBoxPopup_OnOpened(object sender, EventArgs e)
        {
            var textBox = Tips.GetVisualChildren(((Popup)sender).Child).OfType<TextBox>().FirstOrDefault();
            textBox.Text = (textBox.DataContext as ColorToneBox).Info;
            textBox.Focus();
        }

        private void ColorBox_OnSetColor(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var toggleButton = Tips.GetVisualParents(element).OfType<Grid>().SelectMany(grid => grid.Children.OfType<ToggleButton>()).FirstOrDefault();
            toggleButton.IsChecked = false;

            _hsl = (element.DataContext as ColorToneBox).GetBackgroundHSL();
            UpdateValues(ColorSpace.HSL);
        }
        #endregion

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.Print($"ButtonBase_OnClick");
        }

        #region ===========  ColorProperty  ======================

        private static Dictionary<string, ColorProperty> Properties => new Dictionary<string, ColorProperty>()
        {
            {"RGB_R", new ColorProperty(0, 255, ColorSpace.RGB, p => p._rgb.R, (p, v) => p._rgb.R = v)},
            {"RGB_G", new ColorProperty(0, 255, ColorSpace.RGB, p => p._rgb.G, (p, v) => p._rgb.G = v)},
            {"RGB_B", new ColorProperty(0, 255, ColorSpace.RGB, p => p._rgb.B, (p, v) => p._rgb.B = v)},
            {"HSL_H", new ColorProperty(0, 360, ColorSpace.HSL, p => p._hsl.H, (p, v) => p._hsl.H = v)},
            {"HSL_S", new ColorProperty(0, 100, ColorSpace.HSL, p => p._hsl.S, (p, v) => p._hsl.S = v)},
            {"HSL_L", new ColorProperty(0, 100, ColorSpace.HSL, p => p._hsl.L, (p, v) => p._hsl.L = v)},
            {"HSV_H", new ColorProperty(0, 360, ColorSpace.HSV, p => p._hsv.H, (p, v) => p._hsv.H = v)},
            {"HSV_S", new ColorProperty(0, 100, ColorSpace.HSV, p => p._hsv.S, (p, v) => p._hsv.S = v)},
            {"HSV_V", new ColorProperty(0, 100, ColorSpace.HSV, p => p._hsv.V, (p, v) => p._hsv.V = v)},
            {"LAB_L", new ColorProperty(0, 100, ColorSpace.LAB, p => p._lab.L / 100, (p, v) => p._lab.L = v * 100)},
            {"LAB_A", new ColorProperty(-127.5, 127.5, ColorSpace.LAB, p => (p._lab.A + 127.5) / 255.0, (p, v) => p._lab.A = v * 255 - 127.5)},
            {"LAB_B", new ColorProperty(-127.5, 127.5, ColorSpace.LAB, p => (p._lab.B + 127.5) / 255.0, (p, v) => p._lab.B = v * 255 - 127.5)},
            {"YCbCr_Y", new ColorProperty(0, 255, ColorSpace.YCbCr, p => p._yCbCr.Y, (p, v) => p._yCbCr.Y = v)},
            {"YCbCr_Cb", new ColorProperty(-127.5, 127.5, ColorSpace.YCbCr, p => p._yCbCr.Cb + 0.5, (p, v) => p._yCbCr.Cb = v - 0.5)},
            {"YCbCr_Cr", new ColorProperty(-127.5, 127.5, ColorSpace.YCbCr, p => p._yCbCr.Cr + 0.5, (p, v) => p._yCbCr.Cr = v - 0.5)}
        };

        public class ColorProperty
        {
            public double Min;
            public double Max;
            public ColorSpace ColorSpace;
            public Func<ColorPickerAsync, double> SliderValue;
            public Action<ColorPickerAsync, double> MouseMoveAction;

            public ColorProperty(double min, double max, ColorSpace colorSpace, Func<ColorPickerAsync, double> sliderValue, Action<ColorPickerAsync, double> mouseMoveAction )
            {
                Min = min; Max = max; ColorSpace = colorSpace; SliderValue = sliderValue;
                MouseMoveAction = mouseMoveAction;
            }
        }
        #endregion

        #region ===========  Linear gradient brushes for Color property  ==========
        public Dictionary<string, LinearGradientBrush> Brushes { get; private set; }

        private void UpdateSliderBrushes()
        {
            if (Brushes == null)
            {
                Brushes = new Dictionary<string, LinearGradientBrush>();
                foreach (var kvp in Properties)
                {
                    var gradientCount = kvp.Value.ColorSpace == ColorSpace.RGB
                        ? 1
                        : Convert.ToInt32(kvp.Value.Max - kvp.Value.Min);
                    Brushes.Add(kvp.Key,
                        new LinearGradientBrush(new GradientStopCollection(Enumerable.Range(0, gradientCount + 1)
                            .Select(n => new GradientStop(Colors.Transparent, 1.0 * n / gradientCount)))));
                }
            }

            for (var k = 0; k < 2; k++)
            {
                Brushes["RGB_R"].GradientStops[k].Color = Color.FromRgb(Convert.ToByte(255 * k), CurrentColor.G, CurrentColor.B);
                Brushes["RGB_G"].GradientStops[k].Color = Color.FromRgb(CurrentColor.R, Convert.ToByte(255 * k), CurrentColor.B);
                Brushes["RGB_B"].GradientStops[k].Color = Color.FromRgb(CurrentColor.R, CurrentColor.G, Convert.ToByte(255 * k));
            }
            for (var k = 0; k <= 100; k++)
            {
                Brushes["HSL_S"].GradientStops[k].Color = new ColorSpaces.HSL(_hsl.H, k / 100.0, _hsl.L).GetRGB().GetColor();
                Brushes["HSL_L"].GradientStops[k].Color = new ColorSpaces.HSL(_hsl.H, _hsl.S, k / 100.0).GetRGB().GetColor();
                Brushes["HSV_S"].GradientStops[k].Color = new ColorSpaces.HSV(_hsv.H, k / 100.0, _hsv.V).GetRGB().GetColor();
                Brushes["HSV_V"].GradientStops[k].Color = new ColorSpaces.HSV(_hsv.H, _hsv.S, k / 100.0).GetRGB().GetColor();
                Brushes["LAB_L"].GradientStops[k].Color = new ColorSpaces.LAB(k, _lab.A, _lab.B).GetRGB().GetColor();
            }
            for (var k = 0; k <= 255; k++)
            {
                Brushes["LAB_A"].GradientStops[k].Color = new ColorSpaces.LAB(_lab.L, k - 127.5, _lab.B).GetRGB().GetColor();
                Brushes["LAB_B"].GradientStops[k].Color = new ColorSpaces.LAB(_lab.L, _lab.A, k - 127.5).GetRGB().GetColor();
                Brushes["YCbCr_Y"].GradientStops[k].Color = new ColorSpaces.YCbCr(k / 255.0, _yCbCr.Cb, _yCbCr.Cr).GetRGB().GetColor();
                Brushes["YCbCr_Cb"].GradientStops[k].Color = new ColorSpaces.YCbCr(_yCbCr.Y, (k - 127.5) / 255, _yCbCr.Cr).GetRGB().GetColor();
                Brushes["YCbCr_Cr"].GradientStops[k].Color = new ColorSpaces.YCbCr(_yCbCr.Y, _yCbCr.Cb, (k - 127.5) / 255).GetRGB().GetColor();
            }
            for (var k = 0; k <= 360; k++)
            {
                Brushes["HSL_H"].GradientStops[k].Color = new ColorSpaces.HSL(k / 360.0, _hsl.S, _hsl.L).GetRGB().GetColor();
                Brushes["HSV_H"].GradientStops[k].Color = new ColorSpaces.HSV(k / 360.0, _hsv.S, _hsv.V).GetRGB().GetColor();
            }
        }

        private void NewUpdateSliderBrushes()
        {
            const int gradientStopsNumber = 20;
            if (Brushes == null)
            {
                Brushes = new Dictionary<string, LinearGradientBrush>();
                foreach (var kvp in Properties)
                {
                    var gradientCount = kvp.Value.ColorSpace == ColorSpace.RGB ? 1 : gradientStopsNumber;
                    Brushes.Add(kvp.Key,
                        new LinearGradientBrush(new GradientStopCollection(Enumerable.Range(0, gradientCount + 1)
                            .Select(n => new GradientStop(Colors.Transparent, 1.0 * n / gradientCount)))));
                }
            }

            for (var k = 0; k < 2; k++)
            {
                var component = k == 0 ? (byte) 0x0 : (byte) 0xff;
                Brushes["RGB_R"].GradientStops[k].Color = Color.FromRgb(component, CurrentColor.G, CurrentColor.B);
                Brushes["RGB_G"].GradientStops[k].Color = Color.FromRgb(CurrentColor.R, component, CurrentColor.B);
                Brushes["RGB_B"].GradientStops[k].Color = Color.FromRgb(CurrentColor.R, CurrentColor.G, component);
            }

            const double xStep = 1.0 / (gradientStopsNumber + 1);
            var x = 0.0;
            for (var k = 0; k <= gradientStopsNumber; k++)
            {
                Brushes["HSL_H"].GradientStops[k].Color = new ColorSpaces.HSL(x, _hsl.S, _hsl.L).GetRGB().GetColor();
                Brushes["HSL_S"].GradientStops[k].Color = new ColorSpaces.HSL(_hsl.H, x, _hsl.L).GetRGB().GetColor();
                Brushes["HSL_L"].GradientStops[k].Color = new ColorSpaces.HSL(_hsl.H, _hsl.S, x).GetRGB().GetColor();
                Brushes["HSV_H"].GradientStops[k].Color = new ColorSpaces.HSV(x, _hsv.S, _hsv.V).GetRGB().GetColor();
                Brushes["HSV_S"].GradientStops[k].Color = new ColorSpaces.HSV(_hsv.H, x, _hsv.V).GetRGB().GetColor();
                Brushes["HSV_V"].GradientStops[k].Color = new ColorSpaces.HSV(_hsv.H, _hsv.S, x).GetRGB().GetColor();
                Brushes["LAB_L"].GradientStops[k].Color = new ColorSpaces.LAB(x * 100, _lab.A, _lab.B).GetRGB().GetColor();
                Brushes["LAB_A"].GradientStops[k].Color = new ColorSpaces.LAB(_lab.L, x * 255 - 127.5, _lab.B).GetRGB().GetColor();
                Brushes["LAB_B"].GradientStops[k].Color = new ColorSpaces.LAB(_lab.L, _lab.A, x * 255 - 127.5).GetRGB().GetColor();
                Brushes["YCbCr_Y"].GradientStops[k].Color = new ColorSpaces.YCbCr(x, _yCbCr.Cb, _yCbCr.Cr).GetRGB().GetColor();
                Brushes["YCbCr_Cb"].GradientStops[k].Color = new ColorSpaces.YCbCr(_yCbCr.Y, x - 0.5, _yCbCr.Cr).GetRGB().GetColor();
                Brushes["YCbCr_Cr"].GradientStops[k].Color = new ColorSpaces.YCbCr(_yCbCr.Y, _yCbCr.Cb, x - 0.5).GetRGB().GetColor();
                x += xStep;
            }
        }
        #endregion
    }
}
