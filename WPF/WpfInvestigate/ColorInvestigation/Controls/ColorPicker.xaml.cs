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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ColorInvestigation.Common.ColorSpaces;

namespace ColorInvestigation.Controls
{
    /// <summary>
    /// Interaction logic ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        // Constructor
        public ColorPicker()
        {
            InitializeComponent();
            SaveColor();
        }

        private const int ComponentNumber = 15;
        internal CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;

        public enum ColorSpace { RGB, HSL, HSV, LAB, YCbCr };

        #region  ==============  Public Properties  ================
        // Original color
        public Color Color
        {
            get => new RGB(_oldColorData[0]/255, _oldColorData[1]/255, _oldColorData[2]/255).GetColor(_oldColorData[ComponentNumber]);
            set
            {
                CurrentColor = value;
                SaveColor();
            }
        }

        private Color CurrentColor
        {
            get => new RGB(GetCC(0), GetCC(1), GetCC(2)).GetColor(Alpha);
            set
            {
                _isUpdating = true;
                _alpha = value.A / 255.0;
                RGB_R = value.R;
                RGB_G = value.G;
                _isUpdating = false;
                RGB_B = value.B;
            }
        }

        private SolidColorBrush[] _brushesCache = { new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush() };
        private SolidColorBrush GetCacheBrush(int index, Color color)
        {
            _brushesCache[index].Color = color;
            return _brushesCache[index];
        }

        public SolidColorBrush HueBrush => GetCacheBrush(0, new HSV(GetCC(6), 1, 1).RGB.Color);

        public SolidColorBrush Color_ForegroundBrush => GetCacheBrush(1, ColorUtils.GetForegroundColor(Color));
        public SolidColorBrush CurrentColor_ForegroundBrush => GetCacheBrush(2, ColorUtils.GetForegroundColor(CurrentColor));

        public SolidColorBrush ColorWithoutAlphaBrush => GetCacheBrush(3,
            new RGB(_oldColorData[0] / 255, _oldColorData[1] / 255, _oldColorData[2] / 255).Color);

        public SolidColorBrush CurrentColorWithoutAlphaBrush => GetCacheBrush(4, new RGB(GetCC(0), GetCC(1), GetCC(2)).Color);

        #endregion

        #region  ===========  Color component public Properties  ============
        private double _alpha = 1.0;
        private double[] _values = new double[ComponentNumber];

        // Get color component in space unit
        private double GetCC(int index) => _values[index] / Metalist[index].SpaceMultiplier;
        // Set color components from space unit
        private void SetCC(int startIndex, params double[] newValues)
        {
            for (var k = 0; k < newValues.Length; k++)
                _values[k + startIndex] = newValues[k] * Metalist[k + startIndex].SpaceMultiplier;
        }

        public double Alpha // in range [0, 1]
        {
            get => _alpha;
            set
            {
                _alpha = value;
                if (!_isUpdating)
                    UpdateUI();
            }
        }
        public double RGB_R // in range [0, 255]
        {
            get => _values[0]; set => SetProperty(value);
        }
        public double RGB_G // in range [0, 255]
        {
            get => _values[1]; set => SetProperty(value);
        }
        public double RGB_B // in range [0, 255]
        {
            get => _values[2]; set => SetProperty(value);
        }
        public double HSL_H // in range [0, 360]
        {
            get => _values[3]; set => SetProperty(value);
        }
        public double HSL_S // in range [0, 100]
        {
            get => _values[4]; set => SetProperty(value);
        }
        public double HSL_L // in range [0, 100]
        {
            get => _values[5]; set => SetProperty(value);
        }
        public double HSV_H // in range [0, 360]
        {
            get => _values[6]; set => SetProperty(value);
        }
        public double HSV_S // in range [0, 100]
        {
            get => _values[7]; set => SetProperty(value);
        }
        public double HSV_V // in range [0, 100]
        {
            get => _values[8]; set => SetProperty(value);
        }
        public double LAB_L // in range [0, 100]
        {
            get => _values[9]; set => SetProperty(value);
        }
        public double LAB_A // in range [-127.5, 127.5]
        {
            get => _values[10]; set => SetProperty(value);
        }
        public double LAB_B // in range [-127.5, 127.5]
        {
            get => _values[11]; set => SetProperty(value);
        }
        public double YCbCr_Y // in range [0, 100]
        {
            get => _values[12]; set => SetProperty(value);
        }
        public double YCbCr_Cb // in range [-127.5, 127.5]
        {
            get => _values[13]; set => SetProperty(value);
        }
        public double YCbCr_Cr // in range [-127.5, 127.5]
        {
            get => _values[14]; set => SetProperty(value);
        }
        public Tuple<double, double> HSV_S_And_V // set HSV.S & HSV.V simultaneously (for HueAndSaturation slider)
        {
            set
            {
                _isUpdating = true;
                HSV_S = value.Item1;
                _isUpdating = false;
                HSV_V = value.Item2;
            }
        }

        private bool _isUpdating;
        public void SetProperty(double value, [CallerMemberName]string propertyName = null)
        {
            var meta = Metadata[propertyName];
            value = Math.Max(meta.Min, Math.Min(meta.Max, value));
            _values[meta.SeqNo] = value;
            if (!_isUpdating)
            {
                _isUpdating = true;
                UpdateValues(meta.ColorSpace);
                _isUpdating = false;
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

        #region ===========  Color component metadata  ============
        public static Dictionary<string, MetaItem> Metadata;

        static ColorPicker()
        {
            for (var k = 0; k < Metalist.Count; k++)
            {
                Metalist[k].SeqNo = k;
                Metalist[k].ColorSpace = (ColorSpace)Enum.Parse(typeof(ColorSpace), Metalist[k].Id.Split('_')[0]);
            }
            Metadata = Metalist.ToDictionary(a => a.Id, a => a);
        }

        private static List<MetaItem> Metalist = new List<MetaItem>
        {
            new MetaItem(nameof(RGB_R), 0, 255),
            new MetaItem(nameof(RGB_G), 0, 255),
            new MetaItem(nameof(RGB_B), 0, 255),
            new MetaItem(nameof(HSL_H), 0, 360),
            new MetaItem(nameof(HSL_S), 0, 100),
            new MetaItem(nameof(HSL_L), 0, 100),
            new MetaItem(nameof(HSV_H), 0, 360),
            new MetaItem(nameof(HSV_S), 0, 100),
            new MetaItem(nameof(HSV_V), 0, 100),
            new MetaItem(nameof(LAB_L), 0, 100),
            new MetaItem(nameof(LAB_A), -127.5, 127.5),
            new MetaItem(nameof(LAB_B), -127.5, 127.5),
            new MetaItem(nameof(YCbCr_Y), 0, 255),
            new MetaItem(nameof(YCbCr_Cb), -127.5, 127.5),
            new MetaItem(nameof(YCbCr_Cr), -127.5, 127.5)
        };

        public class MetaItem
        {
            public readonly string Id;
            public int SeqNo;
            public readonly double Min;
            public readonly double Max;
            public readonly double SpaceMultiplier;
            public ColorSpace ColorSpace;
            public double GetValue(ColorPicker picker) => picker._values[SeqNo];

            public MetaItem(string id, double min, double max)
            {
                Id = id; Min = min; Max = max;
                SpaceMultiplier = Id.StartsWith("LAB") ? 1 : Max - Min;
            }
        }
        #endregion

        #region ==============  Update Values/UI  ===============
        private void UpdateValues(ColorSpace baseColorSpace)
        {
            // Get rgb object
            var rgb = new RGB(0, 0, 0);
            if (baseColorSpace == ColorSpace.RGB)
                rgb = new RGB(GetCC(0), GetCC(1), GetCC(2));
            else if (baseColorSpace == ColorSpace.HSL)
            {
                rgb = new HSL(GetCC(3), GetCC(4), GetCC(5)).RGB;
                // Update HSV
                _values[6] = _values[3]; // _hsv.H = _hsl.H;
                var hsv = new HSV(rgb); // _hsv = new ColorSpaces.HSV(_rgb);
                SetCC(7, hsv.S, hsv.V);
            }
            else if (baseColorSpace == ColorSpace.HSV)
            {
                rgb = new HSV(GetCC(6), GetCC(7), GetCC(8)).RGB;
                // Update HSL
                _values[3] = _values[6]; // _hsl.H = _hsv.H;
                var hsl = new HSL(rgb); // _hsl = new ColorSpaces.HSL(_rgb);
                SetCC(4, hsl.S, hsl.L);
            }
            else if (baseColorSpace == ColorSpace.LAB)
                rgb = new LAB(GetCC(9), GetCC(10), GetCC(11)).RGB;
            else if (baseColorSpace == ColorSpace.YCbCr)
                rgb = new YCbCr(GetCC(12), GetCC(13), GetCC(14)).RGB;

            // Update other objects
            if (baseColorSpace != ColorSpace.RGB)
                SetCC(0, rgb.R, rgb.G, rgb.B);
            if (baseColorSpace != ColorSpace.HSL && baseColorSpace != ColorSpace.HSV)
            {
                var hsl = new HSL(rgb);
                SetCC(3, hsl.H, hsl.S, hsl.L);
                var hsv = new HSV(rgb);
                SetCC(6, hsv.H, hsv.S, hsv.V);
            }
            if (baseColorSpace != ColorSpace.LAB)
            {
                var lab = new LAB(rgb);
                SetCC(9, lab.L, lab.A, lab.B);
            }
            if (baseColorSpace != ColorSpace.YCbCr)
            {
                var yCbCr = new YCbCr(rgb);
                SetCC(12, yCbCr.Y, yCbCr.Cb, yCbCr.Cr);
            }

            UpdateUI();
        }

        public void UpdateUI()
        {
            OnPropertiesChanged(Metadata.Keys.ToArray());
            OnPropertiesChanged(nameof(CurrentColor), nameof(HueBrush), nameof(CurrentColor_ForegroundBrush),
                nameof(CurrentColorWithoutAlphaBrush));

            UpdateSlider(SaturationAndValueSlider, GetSliderValueByModel("HSV_S"), 1.0 - GetSliderValueByModel("HSV_V"));
            UpdateSlider(HueSlider, null, GetSliderValueByModel("HSV_H"));
            UpdateSlider(AlphaSlider, null, 1.0 - Alpha);

            foreach (var kvp in Metadata)
                UpdateSlider(FindName("Slider_" + kvp.Key) as FrameworkElement, GetSliderValueByModel(kvp.Key), null);

            UpdateTones();
            OnPropertiesChanged(nameof(Tones));

            UpdateSliderBrushes();
            OnPropertiesChanged(nameof(Brushes));

        }

        #endregion

        #region ===========  Save & Restore color  =================

        private double[] _savedColorData = new double[ComponentNumber + 1];
        private double[] _oldColorData = new double[ComponentNumber + 1];

        public void SaveColor()
        {
            for (var k = 0; k < _values.Length; k++)
            {
                _savedColorData[k] = _oldColorData[k];
                _oldColorData[k] = _values[k];
            }
            _savedColorData[_savedColorData.Length - 1] = _oldColorData[_savedColorData.Length - 1];
            _oldColorData[_savedColorData.Length - 1] = Alpha;
            OnPropertiesChanged(nameof(Color), nameof(Color_ForegroundBrush), nameof(ColorWithoutAlphaBrush));
        }
        public void RestoreColor()
        {
            for (var k = 0; k < _values.Length; k++)
            {
                _oldColorData[k] = _savedColorData[k];
                _values[k] = _savedColorData[k];
            }
            _oldColorData[_oldColorData.Length - 1] = _savedColorData[_oldColorData.Length - 1];
            Alpha = _savedColorData[_savedColorData.Length - 1];
            OnPropertiesChanged(nameof(Color), nameof(Color_ForegroundBrush), nameof(ColorWithoutAlphaBrush));
        }
        #endregion

        #region ===========  Linear gradient brushes for Color components  ==========
        public Dictionary<string, LinearGradientBrush> Brushes { get; private set; }

        private void UpdateSliderBrushes()
        {
            if (Brushes == null)
            {
                Brushes = new Dictionary<string, LinearGradientBrush>();
                foreach (var kvp in Metadata)
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
                Brushes["HSL_S"].GradientStops[k].Color = new HSL(GetCC(3), k / 100.0, GetCC(5)).RGB.Color;
                Brushes["HSL_L"].GradientStops[k].Color = new HSL(GetCC(3), GetCC(4), k / 100.0).RGB.Color;
                Brushes["HSV_S"].GradientStops[k].Color = new HSV(GetCC(6), k / 100.0, GetCC(8)).RGB.Color;
                Brushes["HSV_V"].GradientStops[k].Color = new HSV(GetCC(6), GetCC(7), k / 100.0).RGB.Color;
                Brushes["LAB_L"].GradientStops[k].Color = new LAB(k, GetCC(10), GetCC(11)).RGB.Color;
            }
            for (var k = 0; k <= 255; k++)
            {
                Brushes["LAB_A"].GradientStops[k].Color = new LAB(GetCC(9), k - 127.5, GetCC(11)).RGB.Color;
                Brushes["LAB_B"].GradientStops[k].Color = new LAB(GetCC(9), GetCC(10), k - 127.5).RGB.Color;
                Brushes["YCbCr_Y"].GradientStops[k].Color = new YCbCr(k / 255.0, GetCC(13), GetCC(14)).RGB.Color;
                Brushes["YCbCr_Cb"].GradientStops[k].Color = new YCbCr(GetCC(12), (k - 127.5) / 255, GetCC(14)).RGB.Color;
                Brushes["YCbCr_Cr"].GradientStops[k].Color = new YCbCr(GetCC(12), GetCC(13), (k - 127.5) / 255).RGB.Color;
            }
            for (var k = 0; k <= 360; k++)
            {
                Brushes["HSL_H"].GradientStops[k].Color = new HSL(k / 360.0, GetCC(4), GetCC(5)).RGB.Color;
                Brushes["HSV_H"].GradientStops[k].Color = new HSV(k / 360.0, GetCC(7), GetCC(8)).RGB.Color;
            }
        }

        #endregion

        #region ==============  Tones  =======================
        public class ColorToneBox
        {
            private readonly ColorPicker _owner;
            public int GridColumn { get; }
            public int GridRow { get; }
            public SolidColorBrush Background { get; } = new SolidColorBrush();
            public SolidColorBrush Foreground { get; } = new SolidColorBrush();
            public string Info
            {
                get
                {
                    var rgb = GetBackgroundHSL().RGB;
                    var hsl = GetBackgroundHSL();
                    var hsv = new HSV(rgb) { H = hsl.H };
                    var lab = new LAB(rgb);
                    var yCbCr = new YCbCr(rgb);

                    var sb = new StringBuilder();
                    sb.AppendLine("HEX:".PadRight(5) + rgb.Color);
                    sb.AppendLine("Gray level: ???".PadRight(15));
                    sb.AppendLine(FormatInfoString("RGB", rgb.R * 255, rgb.G * 255, rgb.B * 255));
                    sb.AppendLine(FormatInfoString("HSL", hsl.H * 360, hsl.S * 100, hsl.L * 100));
                    sb.AppendLine(FormatInfoString("HSV", hsv.H * 360, hsv.S * 100, hsv.V * 100));
                    sb.AppendLine(FormatInfoString("LAB", lab.L, lab.A, lab.B));
                    sb.Append(FormatInfoString("YCbCr", yCbCr.Y * 255, yCbCr.Cb * 255, yCbCr.Cr * 255));
                    return sb.ToString();
                }
            }

            public ColorToneBox(ColorPicker owner, int gridColumn, int gridRow)
            {
                _owner = owner;
                GridColumn = gridColumn;
                GridRow = gridRow;
                Foreground.Color = gridColumn == 0 ? Colors.White : Colors.Black;
            }

            internal HSL GetBackgroundHSL()
            {
                if (GridColumn == 0)
                    return new HSL(_owner.GetCC(3), _owner.GetCC(4), 0.025 + 0.05 * GridRow);
                if (GridColumn == 1)
                    return new HSL(_owner.GetCC(3), _owner.GetCC(4), 0.975 - 0.05 * GridRow);
                return new HSL(_owner.GetCC(3), 0.05 + 0.1 * GridRow, _owner.GetCC(5));
            }

            private string FormatInfoString(string label, double value1, double value2, double value3) =>
                (label + ":").PadRight(7) + FormatDouble(value1) + FormatDouble(value2) + FormatDouble(value3);
            private string FormatDouble(double value) => value.ToString("F1", _owner.CurrentCulture).PadLeft(7);
        }

        private const int NumberOfTones = 10;
        public ColorToneBox[] Tones { get; private set; }

        private void UpdateTones()
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
                tone.Background.Color = tone.GetBackgroundHSL().RGB.Color;
                if (tone.GridColumn == 2)
                    tone.Foreground.Color = ColorUtils.GetForegroundColor(tone.Background.Color);
            }

            OnPropertiesChanged(nameof(Tones));
        }
        #endregion

        #region =============  CONTROL  ==================
        private void Control_OnSizeChanged(object sender, SizeChangedEventArgs e) => UpdateUI();

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
            var metaData = Metadata[propertyName];
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
                    HSV_H = GetModelValueBySlider("HSV_H", value);
                else if (canvas.Name == nameof(AlphaSlider))
                    Alpha = 1.0 - value;
                else
                {
                    var valueId = sliderName.Replace("Slider_", "");
                    SetProperty(GetModelValueBySlider(valueId, value), valueId);
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

                HSV_S_And_V = new Tuple<double, double>(GetModelValueBySlider("HSV_S", x / canvas.ActualWidth),
                    GetModelValueBySlider("HSV_V", 1 - y / canvas.ActualHeight));
            }
        }

        #endregion

        #region  ==============  NEW from ASYNC  =================
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

            var hsl = (element.DataContext as ColorToneBox).GetBackgroundHSL();
            SetCC(3, hsl.H, hsl.S, hsl.L);

            UpdateValues(ColorSpace.HSL);
        }
        #endregion

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.Print($"ButtonBase_OnClick");
        }


        #endregion

        #region ================  Update slider values  ======================
        private double GetModelValueBySlider(string componentName, double sliderValue)
        {
            var meta = ColorPicker.Metadata[componentName];
            return (meta.Max - meta.Min) * sliderValue + meta.Min;
        }
        private double GetSliderValueByModel(string componentName)
        {
            var meta = ColorPicker.Metadata[componentName];
            return (meta.GetValue(this) - meta.Min) / (meta.Max - meta.Min);
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
