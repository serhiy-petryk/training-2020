// ToDo:
// 1. fixed mouse move on HSV area
// 2. Layout
// 3. Remove all dependency properties + use calculated properties
//      in setter set variable _isChanging=true
//      public properties are Color type (not Brush)
//      ? old/current Color
//      add rgb tuple to ColorUtilities
// 4. Separate component: ColorComponentSlider
//      try use calculated properties
// +5. StartUp color
// +6. Remove IsMouseDown property (only Capture)
// 7. Process value when mouse is clicked
// +8. Size changed => how Hue don't change

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ColorInvestigation.Common;
using ColorInvestigation.Lib;

namespace ColorInvestigation.Controls
{
    /// <summary>
    /// Interaction logic ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        private enum UpdateMode { RGB, HSL, HSV, XYZ, LAB, YCbCr};

        // Color space values must be independent => we need to have separate object for each color space 
        private double _alpha = 1.0;
        private ColorSpaces.RGB _rgb = new ColorSpaces.RGB(0, 0, 0);
        private ColorSpaces.HSL _hsl = new ColorSpaces.HSL(0,0,0);
        private ColorSpaces.HSV _hsv = new ColorSpaces.HSV(0,0,0);
        private ColorSpaces.XYZ _xyz= new ColorSpaces.XYZ(0,0,0);
        private ColorSpaces.LAB _lab = new ColorSpaces.LAB(0, 0, 0);
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
        private Color CurrentColor => _rgb.GetColor(_alpha);

        // Constructor
        public ColorPicker()
        {
            InitializeComponent();
            UpdateValue(UpdateMode.RGB);
            _savedColor = _oldRgb.GetColor(_oldAlpha);
        }

        public Color Color
        {
            get => _oldRgb.GetColor(_oldAlpha);
            set
            {
                _rgb = new ColorSpaces.RGB(value);
                _alpha = value.A;
                SaveColor();
                UpdateUI();
            }
        }

        public void SaveColor()
        {
            _savedColor = _oldRgb.GetColor(_oldAlpha);
            _oldRgb = _rgb;
            _oldAlpha = _alpha;
            OnPropertiesChanged(nameof(Color));
        }

        public void RestoreColor()
        {
            _oldRgb = new ColorSpaces.RGB(_savedColor);
            _oldAlpha = _savedColor.A;
            xxUpdateRgb(_oldRgb, _oldAlpha);
            OnPropertiesChanged(nameof(Color));
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

        bool _xxisUpdating = false;
        private void xxUpdateRgb(ColorSpaces.RGB newRgb, double? newAlpha)
        {
            if (!_xxisUpdating)
            {
                _xxisUpdating = true;
                _rgb = newRgb;
                _alpha = newAlpha ?? _alpha;
                UpdateUI();
                _xxisUpdating = false;
            }
        }

        private void UpdateUI()
        {
            OnPropertiesChanged(nameof(HueBrush));

            UpdateRgbBrushes();
            UpdateHslBrushes();
            UpdateHsvBrushes();

            UpdateSlider(AlphaSlider, 1.0 - _alpha, 1.0);
            UpdateSlider(HueSlider, _hsv.H, 1.0);

            RefreshSlider(RofRgbSlider, _rgb.R, 1.0);
            RefreshSlider(GofRgbSlider, _rgb.G, 1.0);
            RefreshSlider(BofRgbSlider, _rgb.B, 1.0);

            RefreshSlider(HofHslSlider, _hsl.H, 1.0);
            RefreshSlider(SofHslSlider, _hsl.S, 1.0);
            RefreshSlider(LofHslSlider, _hsl.L, 1.0);

            RefreshSlider(HofHsvSlider, _hsv.H, 1.0);
            RefreshSlider(SofHsvSlider, _hsv.S, 1.0);
            RefreshSlider(VofHsvSlider, _hsv.V, 1.0);

            /*var hsl = ColorUtilities.ColorToHsl(CurrentColor);
            RefreshSlider(HControl, hsl.Item1 * 360, 360);
            RefreshSlider(HslSaturationControl, hsl.Item2 * 100, 100);*/

        }

        #region ==============  Event handlers  ====================

        /// <summary>
        /// Processing the control size changing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*var height = ((Canvas)ColorBoxThumb.Parent).ActualHeight;
            var width = ((Canvas)ColorBoxThumb.Parent).ActualWidth;
            Canvas.SetLeft(ColorBoxThumb, (Saturation / 100) * width - (ColorBoxThumb.ActualWidth / 2));
            Canvas.SetTop(ColorBoxThumb, -((Value / 100) * height) + height - (ColorBoxThumb.ActualHeight / 2));

            height = ((Canvas)HueThumb.Parent).ActualHeight;
            Canvas.SetTop(HueThumb, (Hue / 100) * height - (HueThumb.ActualHeight / 2));

            height = ((Canvas)AlphaThumb.Parent).ActualHeight;
            Canvas.SetTop(AlphaThumb, -((Alpha / 100) * height) + height - (AlphaThumb.ActualHeight / 2));

            RefreshUI();*/

            UpdateUI();
        }

        /// <summary>
        /// Processing when mouse down on Hue/Alpha sliders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).CaptureMouse();
            Keyboard.ClearFocus();
        }

        /// <summary>
        /// Processing when mouse up on Hue/Alpha sliders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).ReleaseMouseCapture();
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = sender as Panel;
                var sliderName = string.IsNullOrEmpty(canvas.Name)
                    ? ((FrameworkElement)VisualTreeHelper.GetParent(canvas)).Name
                    : canvas.Name;

                var isVertical = canvas.ActualHeight > canvas.ActualWidth;
                var offset = isVertical ? e.GetPosition(canvas).Y : e.GetPosition(canvas).X;
                var thumb = canvas.Children[0] as FrameworkElement;

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
                else if (sliderName == nameof(RofRgbSlider))
                {
                    _rgb.R = multiplier;
                    UpdateValue(UpdateMode.RGB);
                }
                else if (sliderName == nameof(GofRgbSlider))
                {
                    _rgb.G = multiplier;
                    UpdateValue(UpdateMode.RGB);
                }
                else if (sliderName == nameof(BofRgbSlider))
                {
                    _rgb.B = multiplier;
                    UpdateValue(UpdateMode.RGB);
                }
                else if (sliderName == nameof(HofHslSlider))
                {
                    _hsl.H = multiplier;
                    UpdateValue(UpdateMode.HSL);
                }
                else if (sliderName == nameof(SofHslSlider))
                {
                    _hsl.S = multiplier;
                    UpdateValue(UpdateMode.HSL);
                }
                else if (sliderName == nameof(LofHslSlider))
                {
                    _hsl.L = multiplier;
                    UpdateValue(UpdateMode.HSL);
                }
                else if (sliderName == nameof(HofHsvSlider))
                {
                    _hsv.H = multiplier;
                    UpdateValue(UpdateMode.HSV);
                }
                else if (sliderName == nameof(SofHsvSlider))
                {
                    _hsv.S = multiplier;
                    UpdateValue(UpdateMode.HSV);
                }
                else if (sliderName == nameof(VofHsvSlider))
                {
                    _hsv.V = multiplier;
                    UpdateValue(UpdateMode.HSV);
                }
            }
        }
        #endregion

        #region ===========  Properties  ==============
        #endregion
        #region ===========  Old Property  ==============
        /// <summary>
        /// Input Brush
        /// </summary>
        public static readonly DependencyProperty BeforeBrushProperty = DependencyProperty.Register(nameof(BeforeBrush),
            typeof(SolidColorBrush), typeof(ColorPicker), new PropertyMetadata(Brushes.Black, (obj, e)=>
            {
                var cp = obj as ColorPicker;
                cp.AfterBrush = cp.BeforeBrush;
                cp.Red = cp.BeforeBrush.Color.R;
                cp.Green = cp.BeforeBrush.Color.G;
                cp.Blue = cp.BeforeBrush.Color.B;
                cp.Alpha = (cp.BeforeBrush.Color.A / 255.0) * 100;

                cp.CalcHSV();
            }));

        /// <summary>
        /// Output brush
        /// </summary>
        public static readonly DependencyProperty AfterBrushProperty = DependencyProperty.Register(nameof(AfterBrush),
            typeof(SolidColorBrush), typeof(ColorPicker), new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// Hue
        /// </summary>
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register(nameof(Hue),
            typeof(double), typeof(ColorPicker), new PropertyMetadata(0d, (obj, e)=> 
            {
                var cp = obj as ColorPicker;
                var height = ((Canvas)cp.HueThumb.Parent).ActualHeight;
                Canvas.SetTop(cp.HueThumb, (cp.Hue / 100) * height - (cp.HueThumb.ActualHeight / 2));
                cp.CalcRGB();
            }));

        /// <summary>
        /// Saturation
        /// </summary>
        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(nameof(Saturation),
            typeof(double), typeof(ColorPicker), new PropertyMetadata(0d, (obj, e)=>
            {
                var cp = obj as ColorPicker;
                var width = ((Canvas)cp.ColorBoxThumb.Parent).ActualWidth;
                Canvas.SetLeft(cp.ColorBoxThumb, (cp.Saturation / 100) * width - (cp.ColorBoxThumb.ActualWidth / 2));
                cp.CalcRGB();
            }));

        /// <summary>
        /// Value
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value),
            typeof(double), typeof(ColorPicker), new PropertyMetadata(0d, (obj, e)=>
            {
                var cp = obj as ColorPicker;
                var height = ((Canvas)cp.ColorBoxThumb.Parent).ActualHeight;
                Canvas.SetTop(cp.ColorBoxThumb, -((cp.Value / 100) * height) + height - (cp.ColorBoxThumb.ActualHeight / 2));
                cp.CalcRGB();
            }));

        /// <summary>
        /// AlphaProperty
        /// </summary>
        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register(nameof(Alpha),
            typeof(double), typeof(ColorPicker), new PropertyMetadata(100d, (obj, e)=>
            {
                var cp = obj as ColorPicker;
                var height = ((Canvas)cp.AlphaThumb.Parent).ActualHeight;
                Canvas.SetTop(cp.AlphaThumb, -((cp.Alpha / 100) * height) + height - (cp.AlphaThumb.ActualHeight / 2));
                cp.CalcRGB();
            }));

        /// <summary>
        /// RedProperty
        /// </summary>
        public static readonly DependencyProperty RedProperty = DependencyProperty.Register(nameof(Red),
            typeof(byte), typeof(ColorPicker), new PropertyMetadata((byte)0, (obj, e)=>
            {
                var cp = obj as ColorPicker;
                cp.CalcHSV();

                var height = ((Canvas)cp.ColorBoxThumb.Parent).ActualHeight;
                var width = ((Canvas)cp.ColorBoxThumb.Parent).ActualWidth;
                Canvas.SetLeft(cp.ColorBoxThumb, (cp.Saturation / 100) * width - (cp.ColorBoxThumb.ActualWidth / 2));
                Canvas.SetTop(cp.ColorBoxThumb, -((cp.Value / 100) * height) + height - (cp.ColorBoxThumb.ActualHeight / 2));

                height = ((Canvas)cp.HueThumb.Parent).ActualHeight;
                Canvas.SetTop(cp.HueThumb, (cp.Hue / 100) * height - (cp.HueThumb.ActualHeight / 2));

                height = ((Canvas)cp.AlphaThumb.Parent).ActualHeight;
                Canvas.SetTop(cp.AlphaThumb, -((cp.Alpha / 100) * height) + height - (cp.AlphaThumb.ActualHeight / 2));
            }));

        private byte _red;
        public Byte Red
        {
            get => _red;
            private set
            {
                _red = value;
                CalcHSV();

                var height = ((Canvas)ColorBoxThumb.Parent).ActualHeight;
                var width = ((Canvas)ColorBoxThumb.Parent).ActualWidth;
                Canvas.SetLeft(ColorBoxThumb, (Saturation / 100) * width - (ColorBoxThumb.ActualWidth / 2));
                Canvas.SetTop(ColorBoxThumb, -((Value / 100) * height) + height - (ColorBoxThumb.ActualHeight / 2));

                height = ((Canvas)HueThumb.Parent).ActualHeight;
                Canvas.SetTop(HueThumb, (Hue / 100) * height - (HueThumb.ActualHeight / 2));

                height = ((Canvas)AlphaThumb.Parent).ActualHeight;
                Canvas.SetTop(AlphaThumb, -((Alpha / 100) * height) + height - (AlphaThumb.ActualHeight / 2));
            }
        }

        /// <summary>
        /// GreenProperty
        /// </summary>
        public static readonly DependencyProperty GreenProperty = DependencyProperty.Register(nameof(Green),
            typeof(byte), typeof(ColorPicker), new PropertyMetadata((byte)0, (obj, e) =>
            {
                var cp = obj as ColorPicker;
                cp.CalcHSV();

                var height = ((Canvas)cp.ColorBoxThumb.Parent).ActualHeight;
                var width = ((Canvas)cp.ColorBoxThumb.Parent).ActualWidth;
                Canvas.SetLeft(cp.ColorBoxThumb, (cp.Saturation / 100) * width - (cp.ColorBoxThumb.ActualWidth / 2));
                Canvas.SetTop(cp.ColorBoxThumb, -((cp.Value / 100) * height) + height - (cp.ColorBoxThumb.ActualHeight / 2));

                height = ((Canvas)cp.HueThumb.Parent).ActualHeight;
                Canvas.SetTop(cp.HueThumb, (cp.Hue / 100) * height - (cp.HueThumb.ActualHeight / 2));

                height = ((Canvas)cp.AlphaThumb.Parent).ActualHeight;
                Canvas.SetTop(cp.AlphaThumb, -((cp.Alpha / 100) * height) + height - (cp.AlphaThumb.ActualHeight / 2));
            }));

        /// <summary>
        /// BlueProperty
        /// </summary>
        public static readonly DependencyProperty BlueProperty = DependencyProperty.Register(nameof(Blue),
            typeof(byte), typeof(ColorPicker), new PropertyMetadata((byte)0, (obj, e) =>
            {
                var cp = obj as ColorPicker;
                cp.CalcHSV();

                var height = ((Canvas)cp.ColorBoxThumb.Parent).ActualHeight;
                var width = ((Canvas)cp.ColorBoxThumb.Parent).ActualWidth;
                Canvas.SetLeft(cp.ColorBoxThumb, (cp.Saturation / 100) * width - (cp.ColorBoxThumb.ActualWidth / 2));
                Canvas.SetTop(cp.ColorBoxThumb, -((cp.Value / 100) * height) + height - (cp.ColorBoxThumb.ActualHeight / 2));

                height = ((Canvas)cp.HueThumb.Parent).ActualHeight;
                Canvas.SetTop(cp.HueThumb, (cp.Hue / 100) * height - (cp.HueThumb.ActualHeight / 2));

                height = ((Canvas)cp.AlphaThumb.Parent).ActualHeight;
                Canvas.SetTop(cp.AlphaThumb, -((cp.Alpha / 100) * height) + height - (cp.AlphaThumb.ActualHeight / 2));
            }));

        public SolidColorBrush BeforeBrush
        {
            get => (SolidColorBrush)GetValue(BeforeBrushProperty);
            set => SetValue(BeforeBrushProperty, value);
        }
        public SolidColorBrush AfterBrush
        {
            get => (SolidColorBrush)GetValue(AfterBrushProperty);
            set => SetValue(AfterBrushProperty, value);
        }
        public double Hue
        {
            get => (double)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }
        public double Saturation
        {
            get => (double)GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public double Alpha
        {
            get => (double)GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }
        /*public byte Red
        {
            get => (byte)GetValue(RedProperty);
            set => SetValue(RedProperty, value);
        }*/
        public byte Green
        {
            get => (byte)GetValue(GreenProperty);
            set => SetValue(GreenProperty, value);
        }
        public byte Blue
        {
            get => (byte)GetValue(BlueProperty);
            set => SetValue(BlueProperty, value);
        }
        #endregion  ==========================

        #region  =========  Event handlers  ==========
        private bool IsCalcHSV = false;
        private bool IsCalcRGB = false;

        /// <summary>
        /// Processing mouse moving for Hue slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HueBarThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = sender as Canvas;
                var grid = canvas.Children[0] as Grid;
                var y = e.GetPosition(canvas).Y;

                y = Math.Max(y, 0);
                y = Math.Min(y, canvas.ActualHeight);

                /*var a1 = RefreshSlider(canvas, y, 100);
                var a2 = y - (HueThumb.ActualHeight / 2);
                var a3 = (y / canvas.ActualHeight) * 100;
                Debug.Print($"A1: {a1}; {a2}; {a3}");*/
                Canvas.SetTop(grid, y - (HueThumb.ActualHeight / 2));

                Hue = (y / canvas.ActualHeight) * 100;

                var canvas1 = sender as Panel;
                var x = e.GetPosition(canvas1).Y;

                var thumb = canvas1.Children[0] as FrameworkElement;
                // var value = (x - thumb.ActualWidth / 2) / (canvas1.ActualWidth - thumb.ActualWidth) * 100;
                var value = (x - thumb.ActualHeight / 2) / (canvas1.ActualHeight - thumb.ActualHeight) * 100;
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                // var byteValue = Convert.ToByte(value);

                Debug.Print($"A1: {Hue}; {value}");
            }
        }

        /// <summary>
        /// Processing mouse moving for Alpha slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlphaBarThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = sender as Canvas;
                var grid = canvas.Children[0] as Grid;
                var y = e.GetPosition(canvas).Y;

                y = Math.Max(y, 0);
                y = Math.Min(y, canvas.ActualHeight);

                Canvas.SetTop(grid, y - (AlphaThumb.ActualHeight / 2));

                Alpha = -((y / canvas.ActualHeight) * 100) + 100;
            }
        }

        /// <summary>
        /// Processing mouse moving for Saturation and brightness controls 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorBoxThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = sender as Canvas;
                var grid = canvas.Children[0] as Grid;
                var x = e.GetPosition(canvas).X;
                var y = e.GetPosition(canvas).Y;

                x = Math.Max(x, 0);
                x = Math.Min(x, canvas.ActualWidth);
                y = Math.Max(y, 0);
                y = Math.Min(y, canvas.ActualHeight);

                Canvas.SetLeft(grid, x - (ColorBoxThumb.ActualWidth / 2));
                Canvas.SetTop(grid, y - (ColorBoxThumb.ActualHeight / 2));

                Saturation = (x / canvas.ActualWidth) * 100;
                Value = -((y / canvas.ActualHeight) * 100) + 100;
            }
        }

        /// <summary>
        /// Processing text changing in HSV text boxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HSVA_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse((sender as TextBox).Text, out double num))
            {
                if (num > 100) (sender as TextBox).Text = "100";
                else if (num < 0) (sender as TextBox).Text = "0";
            }
            else (sender as TextBox).Text = "0";
        }

        /// <summary>
        /// Processing text changing in RGB text boxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RGB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse((sender as TextBox).Text, out int num))
            {
                if (num > 255) (sender as TextBox).Text = "255";
                else if (num < 0) (sender as TextBox).Text = "0";
            }
            else (sender as TextBox).Text = "0";
        }

        #endregion

        /// <summary>
        /// Refresh HSV
        /// </summary>
        private void CalcHSV()
        {
            IsCalcHSV = true;

            if (!IsCalcRGB)
            {
                var max = Math.Max(Red, Math.Max(Green, Blue));
                var min = Math.Min(Red, Math.Min(Green, Blue));

                var hue = 0.0;
                if (Red == Green && Green == Blue) hue = 0;
                else if (Red >= Green && Red >= Blue) hue = 16.667 * ((Green - Blue) / (double)(max - min));
                else if (Green >= Red && Green >= Blue) hue = (16.667 * ((Blue - Red) / (double)(max - min)) + 33.333);
                else if (Blue >= Red && Blue >= Green) hue = (16.667 * ((Red - Green) / (double)(max - min)) + 66.667);

                if (hue < 0) hue += 100;
                Hue = hue;

                try
                {
                    Saturation = ((max - min) / (double)max) * 100;
                    if (double.IsNaN(Saturation)) Saturation = 0;
                }
                catch (DivideByZeroException)
                {
                    Saturation = 0;
                }

                Value = (max / 255.0) * 100;

                AfterBrush = new SolidColorBrush(Color.FromArgb((byte)((Alpha / 100.0) * 255), Red, Green, Blue));
                xxRefreshUI();
            }

            IsCalcHSV = false;
        }

        /// <summary>
        /// Refresh RGB
        /// </summary>
        private void CalcRGB()
        {
            IsCalcRGB = true;

            if (!IsCalcHSV)
            {
                var max = (byte)((Value / 100) * 255);
                var min = (byte)(max - ((Saturation / 100) * max));

                if (Hue <= 16.667)
                {
                    Red = max;
                    Green = (byte)((Hue / 16.667) * (max - min) + min);
                    Blue = min;
                }
                else if (Hue <= 33.333)
                {
                    Red = (byte)(((33.333 - Hue) / 16.667) * (max - min) + min);
                    Green = max;
                    Blue = min;
                }
                else if (Hue <= 50)
                {
                    Red = min;
                    Green = max;
                    Blue = (byte)(((Hue - 33.333) / 16.667) * (max - min) + min);
                }
                else if (Hue <= 66.667)
                {
                    Red = min;
                    Green = (byte)(((66.667 - Hue) / 16.667) * (max - min) + min);
                    Blue = max;
                }
                else if (Hue <= 83.333)
                {
                    Red = (byte)(((Hue - 66.667) / 16.667) * (max - min) + min);
                    Green = min;
                    Blue = max;
                }
                else
                {
                    Red = max;
                    Green = min;
                    Blue = (byte)(((100 - Hue) / 16.667) * (max - min) + min);
                }

                AfterBrush = new SolidColorBrush(Color.FromArgb((byte)((Alpha / 100) * 255), Red, Green, Blue));
                xxRefreshUI();
            }

            IsCalcRGB = false;
        }

        private void ComponentSliderCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = sender as Panel;
                var x = e.GetPosition(canvas).X;

                var thumb = canvas.Children[0] as FrameworkElement;
                var value = (x - thumb.ActualWidth / 2) / (canvas.ActualWidth - thumb.ActualWidth) * 255;
                if (value < 0) value = 0;
                else if (value > 255) value = 255;
                var byteValue = Convert.ToByte(value);

                var control = VisualTreeHelper.GetParent(canvas) as FrameworkElement;
                if (control.Name == "RofRgbSlider")
                    Red = byteValue;
                else if (control.Name == "GofRgbSlider")
                    Green = byteValue;
                else if (control.Name == "BofRgbSlider")
                    Blue = byteValue;
            }
        }

        #region ============  Calculated Properties  =================
        public Color RDarkColor => Color.FromArgb(0xff, 0, CurrentColor.G, CurrentColor.B);
        public Color RLightColor => Color.FromArgb(0xff, 0xff, CurrentColor.G, CurrentColor.B);
        public Color GDarkColor => Color.FromArgb(0xff, CurrentColor.R, 0, CurrentColor.B);
        public Color GLightColor => Color.FromArgb(0xff, CurrentColor.R, 0xff, CurrentColor.B);
        public Color BDarkColor => Color.FromArgb(0xff, CurrentColor.R, CurrentColor.G, 0);
        public Color BLightColor => Color.FromArgb(0xff, CurrentColor.R, CurrentColor.G, 0xff);
        public Color ADarkColor => Color.FromArgb(0, CurrentColor.R, CurrentColor.G, CurrentColor.B);
        public Color ALightColor => Color.FromArgb(0xff, CurrentColor.R, CurrentColor.G, CurrentColor.B);
        public LinearGradientBrush HueOfHslBackground => ColorUtilities.GetHueGradientBrush(CurrentColor);
        public LinearGradientBrush HslSaturationBackground => ColorUtilities.GetHslSaturationGradientBrush(CurrentColor);

        public double HslSaturation
        {
            get => ColorUtilities.ColorToHsl(CurrentColor).Item2 * 100;
            set
            {
                var a1 = value;
            }
        }

        #endregion

        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;

        private void xxRefreshUI()
        {
            return;
            xxUpdateRgb(new ColorSpaces.RGB(CurrentColor.R, CurrentColor.G, CurrentColor.B ), CurrentColor.A);

            OnPropertiesChanged(nameof(RDarkColor), nameof(RLightColor), nameof(GDarkColor), nameof(GLightColor),
                nameof(BDarkColor), nameof(BLightColor), nameof(ADarkColor), nameof(ALightColor), nameof(HueOfHslBackground), nameof(HslSaturationBackground));

            RefreshSlider(RofRgbSlider, Red, 255);
            RefreshSlider(GofRgbSlider, Green, 255);
            RefreshSlider(BofRgbSlider, Blue, 255);

            var hsl = ColorUtilities.ColorToHsl(CurrentColor);
            // RefreshSlider(HControl, hsl.Item1 * 360, 360);
            // RefreshSlider(HslSaturationControl, hsl.Item2 * 100, 100);

        }

        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void RefreshSlider(Control control, double value, double maxValue)
        {
            if (VisualTreeHelper.GetChildrenCount(control) > 0)
                UpdateSlider(VisualTreeHelper.GetChild(control, 0) as Panel, value, maxValue);
        }
        private void UpdateSlider(Panel panel, double value, double maxValue)
        {
            if (VisualTreeHelper.GetChildrenCount(panel) > 0)
            {
                var thumb = panel.Children[0] as FrameworkElement;
                if (panel.ActualWidth > panel.ActualHeight)
                {   // horizontal slider
                    var x = (panel.ActualWidth - thumb.ActualWidth) * value / maxValue;
                    Canvas.SetLeft(thumb, x);
                }
                else
                {   // vertical slider
                    var x = panel.ActualHeight * value / maxValue - thumb.ActualHeight/2;
                    Canvas.SetTop(thumb, x);
                }
            }
        }

        #region ===========  Linear gradient brushes  ====================
        public LinearGradientBrush RofRgbBrush { get; } = CreateLinearGradientBrush(1);
        public LinearGradientBrush GofRgbBrush { get; } = CreateLinearGradientBrush(1);
        public LinearGradientBrush BofRgbBrush { get; } = CreateLinearGradientBrush(1);
        public LinearGradientBrush HofHslBrush { get; } = CreateLinearGradientBrush(360);
        public LinearGradientBrush SofHslBrush { get; } = CreateLinearGradientBrush(100);
        public LinearGradientBrush LofHslBrush { get; } = CreateLinearGradientBrush(100);
        public LinearGradientBrush HofHsvBrush { get; } = CreateLinearGradientBrush(360);
        public LinearGradientBrush SofHsvBrush { get; } = CreateLinearGradientBrush(100);
        public LinearGradientBrush VofHsvBrush { get; } = CreateLinearGradientBrush(100);

        private static LinearGradientBrush CreateLinearGradientBrush(int gradientCount) => new LinearGradientBrush(
            new GradientStopCollection(Enumerable.Range(0, gradientCount + 1)
                .Select(n => new GradientStop(Colors.Transparent, 1.0 * n / gradientCount))));

        private void UpdateRgbBrushes()
        {
            RofRgbBrush.GradientStops[0].Color = Color.FromRgb(0, CurrentColor.G, CurrentColor.B);
            RofRgbBrush.GradientStops[1].Color = Color.FromRgb(0xFF, CurrentColor.G, CurrentColor.B);

            GofRgbBrush.GradientStops[0].Color = Color.FromRgb(CurrentColor.R, 0,CurrentColor.B);
            GofRgbBrush.GradientStops[1].Color = Color.FromRgb(CurrentColor.R,0xFF, CurrentColor.B);

            BofRgbBrush.GradientStops[0].Color = Color.FromRgb(CurrentColor.R, CurrentColor.G, 0);
            BofRgbBrush.GradientStops[1].Color = Color.FromRgb(CurrentColor.R, CurrentColor.G, 0xFF);

            OnPropertiesChanged(nameof(RofRgbBrush), nameof(GofRgbBrush), nameof(BofRgbBrush));
        }

        private void UpdateHslBrushes()
        {
            for (var i = 0; i <= 360; i++)
                HofHslBrush.GradientStops[i].Color = new ColorSpaces.HSL(i / 360.0, _hsl.S, _hsl.L).GetRGB().GetColor();
            for (var i = 0; i <= 100; i++)
            {
                SofHslBrush.GradientStops[i].Color = new ColorSpaces.HSL(_hsl.H, i / 100.0, _hsl.L).GetRGB().GetColor();
                LofHslBrush.GradientStops[i].Color = new ColorSpaces.HSL(_hsl.H, _hsl.S, i / 100.0).GetRGB().GetColor();
            }
            OnPropertiesChanged(nameof(HofHslBrush), nameof(SofHslBrush), nameof(LofHslBrush));
        }

        private void UpdateHsvBrushes()
        {
            for (var i = 0; i <= 360; i++)
                HofHsvBrush.GradientStops[i].Color = new ColorSpaces.HSV(i / 360.0, _hsv.S, _hsv.V).GetRGB().GetColor();
            for (var i = 0; i <= 100; i++)
            {
                SofHsvBrush.GradientStops[i].Color = new ColorSpaces.HSV(_hsv.H, i / 100.0, _hsv.V).GetRGB().GetColor();
                VofHsvBrush.GradientStops[i].Color = new ColorSpaces.HSV(_hsv.H, _hsv.S, i / 100.0).GetRGB().GetColor();
            }
            OnPropertiesChanged(nameof(HofHsvBrush), nameof(SofHsvBrush), nameof(VofHsvBrush));
        }
        #endregion
    }
}
