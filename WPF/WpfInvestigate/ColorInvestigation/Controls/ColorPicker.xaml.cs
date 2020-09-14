// ToDo:
// +1. fixed mouse move on HSV area
// 2. Layout
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
// 10. Tones
// +11. Binding to dictionary
// +12. Edited text box for CurrentColor
// +13. Degree label for Hue
// -14. Control for slider
// 15. ViewModel
// 16. Click on ColorBox
// 17. Alpha for old Color in ColorBox
// 18. Popup for ColorBox info

using ColorInvestigation.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

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
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                UpdateValue(UpdateMode.RGB);
                _savedColor = _oldRgb.GetColor(_oldAlpha);
            }));
        }

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
        public Color CurrentColor
        {
            get => _rgb.GetColor(_alpha);
            set
            {
                _alpha = value.A / 255.0;
                _rgb = new ColorSpaces.RGB(value);
                UpdateValue(UpdateMode.RGB);
            }
        }

        private SolidColorBrush[] _brushesCache = {new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush() };
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
                _brushesCache[1].Color = ColorSpaces.IsDarkColor(new ColorSpaces.RGB(CurrentColor)) ? Colors.White : Colors.Black;
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
            OnPropertiesChanged(nameof(Color), nameof(Color_ForegroundBrush));
        }

        public void RestoreColor()
        {
            _oldRgb = new ColorSpaces.RGB(_savedColor);
            _oldAlpha = _savedColor.A / 255.0;
            _rgb = new ColorSpaces.RGB(_savedColor);
            _alpha = _savedColor.A / 255.0;
            UpdateValue(UpdateMode.RGB);
            OnPropertiesChanged(nameof(Color), nameof(Color_ForegroundBrush));
        }

        private void UpdateValue(UpdateMode mode)
        {
            // Update rgb object
            if (mode == UpdateMode.HSL)
            {
                _rgb = _hsl.GetRGB();
                _hsv = new ColorSpaces.HSV(_rgb);
                _hsv.H = _hsl.H;
            }
            else if (mode == UpdateMode.HSV)
            {
                _rgb = _hsv.GetRGB();
                _hsl = new ColorSpaces.HSL(_rgb);
                _hsl.H = _hsv.H;
            }
            else if (mode == UpdateMode.XYZ)
            {
                _rgb = _xyz.GetRGB();
                _lab = new ColorSpaces.LAB(_xyz);
            }
            else if (mode == UpdateMode.LAB)
            {
                _rgb = _lab.GetRGB();
                _xyz = _lab.GetXYZ();
            }
            else if (mode == UpdateMode.YCbCr)
                _rgb = _yCbCr.GetRGB();

            // Update other objects
            if (mode != UpdateMode.HSL && mode != UpdateMode.HSV)
            {
                _hsl = new ColorSpaces.HSL(_rgb);
                _hsv = new ColorSpaces.HSV(_rgb);
            }
            if (mode != UpdateMode.XYZ && mode != UpdateMode.LAB)
            {
                _xyz = new ColorSpaces.XYZ(_rgb);
                _lab = new ColorSpaces.LAB(_rgb);
            }
            if (mode != UpdateMode.YCbCr)
                _yCbCr = new ColorSpaces.YCbCr(_rgb);

            UpdateUI();
        }

        private void UpdateUI()
        {
            TonesGenerate();
            OnPropertiesChanged(nameof(CurrentColor),
                nameof(CurrentColorWithoutAlphaBrush), nameof(CurrentColor_ForegroundBrush), nameof(HueBrush), 
                nameof(Value_RGB_R), nameof(Value_RGB_G), nameof(Value_RGB_B),
                nameof(Value_HSL_H), nameof(Value_HSL_S), nameof(Value_HSL_L),
                nameof(Value_HSV_H), nameof(Value_HSV_S), nameof(Value_HSV_V),
                nameof(Value_LAB_L), nameof(Value_LAB_A), nameof(Value_LAB_B),
                nameof(Value_YCbCr_Y), nameof(Value_YCbCr_Cb), nameof(Value_YCbCr_Cr));

            UpdateRgbBrushes();
            UpdateHslBrushes();
            UpdateHsvBrushes();
            UpdateLabBrushes();
            UpdateYCbCrBrushes();

            UpdateSaturationAndValueSlider();
            UpdateSlider(AlphaSlider, 1.0 - _alpha, 1.0);
            UpdateSlider(HueSlider, _hsv.H, 1.0);

            RefreshSlider(Slider_RGB_R, _rgb.R, 1.0);
            RefreshSlider(Slider_RGB_G, _rgb.G, 1.0);
            RefreshSlider(Slider_RGB_B, _rgb.B, 1.0);

            RefreshSlider(Slider_HSL_H, _hsl.H, 1.0);
            RefreshSlider(Slider_HSL_S, _hsl.S, 1.0);
            RefreshSlider(Slider_HSL_L, _hsl.L, 1.0);

            RefreshSlider(Slider_HSV_H, _hsv.H, 1.0);
            RefreshSlider(Slider_HSV_S, _hsv.S, 1.0);
            RefreshSlider(Slider_HSV_V, _hsv.V, 1.0);

            RefreshSlider(Slider_LAB_L, _lab.L, 100.0);
            RefreshSlider(Slider_LAB_A, _lab.A + 127.5, 255.0);
            RefreshSlider(Slider_LAB_B, _lab.B + 127.5, 255.0);

            RefreshSlider(Slider_YCbCr_Y, _yCbCr.Y * 255.0, 255.0);
            RefreshSlider(Slider_YCbCr_Cb, _yCbCr.Cb * 255.0 + 127.5, 255.0);
            RefreshSlider(Slider_YCbCr_Cr, _yCbCr.Cr * 255.0 + 127.5, 255.0);
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

                var multiplier = isVertical ? offset / canvas.ActualHeight : (offset - thumb.ActualWidth / 2) / (canvas.ActualWidth - thumb.ActualWidth);
                multiplier = Math.Max(0, Math.Min(1, multiplier));

                if (sliderName == nameof(HueSlider))
                {
                    _hsv.H = multiplier;
                    UpdateValue(UpdateMode.HSV);
                }
                else if (canvas.Name == nameof(AlphaSlider))
                {
                    _alpha = 1.0 - multiplier;
                    UpdateUI();
                }
                else if (sliderName == nameof(Slider_RGB_R))
                {
                    _rgb.R = multiplier;
                    UpdateValue(UpdateMode.RGB);
                }
                else if (sliderName == nameof(Slider_RGB_G))
                {
                    _rgb.G = multiplier;
                    UpdateValue(UpdateMode.RGB);
                }
                else if (sliderName == nameof(Slider_RGB_B))
                {
                    _rgb.B = multiplier;
                    UpdateValue(UpdateMode.RGB);
                }
                else if (sliderName == nameof(Slider_HSL_H))
                {
                    _hsl.H = multiplier;
                    UpdateValue(UpdateMode.HSL);
                }
                else if (sliderName == nameof(Slider_HSL_S))
                {
                    _hsl.S = multiplier;
                    UpdateValue(UpdateMode.HSL);
                }
                else if (sliderName == nameof(Slider_HSL_L))
                {
                    _hsl.L = multiplier;
                    UpdateValue(UpdateMode.HSL);
                }
                else if (sliderName == nameof(Slider_HSV_H))
                {
                    _hsv.H = multiplier;
                    UpdateValue(UpdateMode.HSV);
                }
                else if (sliderName == nameof(Slider_HSV_S))
                {
                    _hsv.S = multiplier;
                    UpdateValue(UpdateMode.HSV);
                }
                else if (sliderName == nameof(Slider_HSV_V))
                {
                    _hsv.V = multiplier;
                    UpdateValue(UpdateMode.HSV);
                }
                else if (sliderName == nameof(Slider_LAB_L))
                {
                    _lab.L = multiplier * 100;
                    UpdateValue(UpdateMode.LAB);
                }
                else if (sliderName == nameof(Slider_LAB_A))
                {
                    _lab.A = multiplier * 255 - 127.5;
                    UpdateValue(UpdateMode.LAB);
                }
                else if (sliderName == nameof(Slider_LAB_B))
                {
                    _lab.B = multiplier * 255 - 127.5;
                    UpdateValue(UpdateMode.LAB);
                }
                else if (sliderName == nameof(Slider_YCbCr_Y))
                {
                    _yCbCr.Y = multiplier;
                    UpdateValue(UpdateMode.YCbCr);
                }
                else if (sliderName == nameof(Slider_YCbCr_Cb))
                {
                    _yCbCr.Cb = multiplier - 0.5;
                    UpdateValue(UpdateMode.YCbCr);
                }
                else if (sliderName == nameof(Slider_YCbCr_Cr))
                {
                    _yCbCr.Cr = multiplier - 0.5;
                    UpdateValue(UpdateMode.YCbCr);
                }
            }
        }

        /// <summary>
        /// Processing mouse moving for Saturation and brightness controls 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                UpdateValue(UpdateMode.HSV);
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

        private void RefreshSlider(Control control, double value, double maxValue)
        {
            var panel = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(control, 0), 0);
            UpdateSlider(panel as Panel, value, maxValue);
        }
        private void UpdateSlider(Panel panel, double value, double maxValue)
        {
            var thumb = panel.Children[0] as FrameworkElement;
            if (panel.ActualWidth > panel.ActualHeight)
            {   // horizontal slider
                var x = (panel.ActualWidth - thumb.ActualWidth) * value / maxValue;
                Canvas.SetLeft(thumb, x);
            }
            else
            {   // vertical slider
                var y = panel.ActualHeight * value / maxValue - thumb.ActualHeight / 2;
                Canvas.SetTop(thumb, y);
            }
        }

        private void UpdateSaturationAndValueSlider()
        {
            var panel = (Panel)SaturationAndValueSlider;
            var thumb = panel.Children[0] as FrameworkElement;
            var x = panel.ActualWidth * _hsv.S - thumb.ActualWidth / 2;
            Canvas.SetLeft(thumb, x);
            var y = panel.ActualHeight * (1.0 - _hsv.V) - thumb.ActualHeight / 2;
            Canvas.SetTop(thumb, y);
        }

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
            Brush_RGB_R.GradientStops[0].Color = Color.FromRgb(0, CurrentColor.G, CurrentColor.B);
            Brush_RGB_R.GradientStops[1].Color = Color.FromRgb(0xFF, CurrentColor.G, CurrentColor.B);

            Brush_RGB_G.GradientStops[0].Color = Color.FromRgb(CurrentColor.R, 0, CurrentColor.B);
            Brush_RGB_G.GradientStops[1].Color = Color.FromRgb(CurrentColor.R, 0xFF, CurrentColor.B);

            Brush_RGB_B.GradientStops[0].Color = Color.FromRgb(CurrentColor.R, CurrentColor.G, 0);
            Brush_RGB_B.GradientStops[1].Color = Color.FromRgb(CurrentColor.R, CurrentColor.G, 0xFF);

            OnPropertiesChanged(nameof(Brush_RGB_R), nameof(Brush_RGB_G), nameof(Brush_RGB_B));
        }

        private void UpdateHslBrushes()
        {
            for (var i = 0; i <= 360; i++)
                Brush_HSL_H.GradientStops[i].Color = new ColorSpaces.HSL(i / 360.0, _hsl.S, _hsl.L).GetRGB().GetColor();
            for (var i = 0; i <= 100; i++)
            {
                Brush_HSL_S.GradientStops[i].Color = new ColorSpaces.HSL(_hsl.H, i / 100.0, _hsl.L).GetRGB().GetColor();
                Brush_HSL_L.GradientStops[i].Color = new ColorSpaces.HSL(_hsl.H, _hsl.S, i / 100.0).GetRGB().GetColor();
            }
            OnPropertiesChanged(nameof(Brush_HSL_H), nameof(Brush_HSL_S), nameof(Brush_HSL_L));
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

        private void ValueEditor_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var box = (TextBox)sender;
            var bindingExpression = box.GetBindingExpression(TextBox.TextProperty);
            var propertyName = bindingExpression.ParentBinding.Path.Path;
            var metaData = ValueMetadata[propertyName];

            if (double.TryParse(box.Text, NumberStyles.Any, CurrentCulture, out var value))
            {
                if (value < metaData.Item1) box.Text = metaData.Item1.ToString(CurrentCulture);
                else if (value > metaData.Item2) box.Text = metaData.Item2.ToString(CurrentCulture);
            }
            else box.Text = "0";
            UpdateValue(metaData.Item3);
        }

        #endregion

        #region ================  SelectAll on Focus events  ===============
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

        #region ==============  Tones  =======================
        public class ColorToneBox
        {
            public int GridColumn { get; }
            public int GridRow { get; }
            public SolidColorBrush Background { get; } = new SolidColorBrush();
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
                        Tones[k2 + k1 * NumberOfTones] = new ColorToneBox( k1, k2);
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

        private void ColorBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _hsl = (((FrameworkElement)sender).DataContext as ColorToneBox).GetBackgroundHSL(_hsl);
            UpdateValue(UpdateMode.HSL);
        }
    }
}
