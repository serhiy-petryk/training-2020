using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ColorInvestigation.Common;

namespace ColorInvestigation.Controls
{
    // ColorPicker ViewModel for DataTemplate
    public class ColorPickerVM : INotifyPropertyChangedAbstract
    {
        internal FrameworkElement Owner;
        public XYSlider AlphaSlider { get; }
        public XYSlider HueSlider { get; }
        public XYSlider SaturationAndValueSlider { get; }
        private ColorComponent[] Components { get; }
        // need to duplicate Component because "Type ColorComponent[]' is not collection" error occurs
        // for "Content={Binding Components[N]}" line of ColorPicker.xaml in VS designer
        // I tried to use Array, Collection<T>, List<T>, ReadOnlyCollection<T>, Dictionary<T,V>
        public ColorComponent RGB_R => Components[0];
        public ColorComponent RGB_G => Components[1];
        public ColorComponent RGB_B => Components[2]; 
        public ColorComponent HSL_H => Components[3];
        public ColorComponent HSL_S => Components[4]; 
        public ColorComponent HSL_L => Components[5];
        public ColorComponent HSV_H => Components[6];
        public ColorComponent HSV_S => Components[7];
        public ColorComponent HSV_V => Components[8];
        public ColorComponent LAB_L => Components[9];
        public ColorComponent LAB_A => Components[10];
        public ColorComponent LAB_B => Components[11];
        public ColorComponent YCbCr_Y => Components[12];
        public ColorComponent YCbCr_Cb => Components[13];
        public ColorComponent YCbCr_Cr => Components[14];

        public ColorToneBox[] Tones { get; }

        #region ==============  Hue/SaturationAndValue Sliders  ============
        public class XYSlider : INotifyPropertyChangedAbstract
        {
            public double xValue { get; private set; }
            public double yValue { get; private set; }
            public virtual double xSliderValue => SizeOfSlider.Width * xValue - SizeOfThumb.Width / 2;
            public virtual double ySliderValue => SizeOfSlider.Height * yValue - SizeOfThumb.Height / 2;
            public Action<double, double> SetValuesAction;

            protected Size SizeOfSlider;
            protected Size SizeOfThumb;

            public XYSlider(Action<double, double> setValuesAction)
            {
                SetValuesAction = setValuesAction;
            }
            public void SetProperties(double xValue, double yValue)
            {
                this.xValue = xValue;
                this.yValue = yValue;
                UpdateUI();
            }

            public override void UpdateUI() => OnPropertiesChanged(nameof(xSliderValue), nameof(ySliderValue));

            public void SetSizeOfControl(Panel panel)
            {
                SizeOfSlider = new Size(panel.ActualWidth, panel.ActualHeight);
                var thumb = panel.Children[0] as FrameworkElement;
                SizeOfThumb = new Size(thumb.ActualWidth, thumb.ActualHeight);
            }
        }
        #endregion

        public ColorPickerVM()
        {
            AlphaSlider = new XYSlider((x, y) =>
            {
                AlphaSlider.SetProperties(0, y);
                UpdateUI();
            });
            
            HueSlider = new XYSlider((x, y) => SetCC(6, y)); // hue of HSV

            SaturationAndValueSlider = new XYSlider((x, y) =>
            {
                _isUpdating = true;
                SetCC(7, x); // saturation of HSV
                _isUpdating = false;
                SetCC(8, 1.0 - y); // value of HSV
            });

            Components = new []
            {
                new ColorComponent(this, "RGB_R", 0, 255, null,
                    (k) => Color.FromRgb(Convert.ToByte(255 * k), CurrentColor.G, CurrentColor.B)),
                new ColorComponent(this, "RGB_G", 0, 255, null,
                    (k) => Color.FromRgb(CurrentColor.R, Convert.ToByte(255 * k), CurrentColor.B)),
                new ColorComponent(this, "RGB_B", 0, 255, null,
                    (k) => Color.FromRgb(CurrentColor.R, CurrentColor.G, Convert.ToByte(255 * k))),
                new ColorComponent(this, "HSL_H", 0, 360, "°",
                    (k) => new ColorSpaces.HSL(k / 100.0, GetCC(4), GetCC(5)).GetRGB().GetColor()),
                new ColorComponent(this, "HSL_S", 0, 100, "%",
                    (k) => new ColorSpaces.HSL(GetCC(3), k / 100.0, GetCC(5)).GetRGB().GetColor()),
                new ColorComponent(this, "HSL_L", 0, 100, "%",
                    (k) => new ColorSpaces.HSL(GetCC(3), GetCC(4), k / 100.0).GetRGB().GetColor()),
                new ColorComponent(this, "HSV_H", 0, 360, "°",
                    (k) => new ColorSpaces.HSV(k / 100.0, GetCC(7), GetCC(8)).GetRGB().GetColor()),
                new ColorComponent(this, "HSV_S", 0, 100, "%",
                    (k) => new ColorSpaces.HSV(GetCC(6), k / 100.0, GetCC(8)).GetRGB().GetColor()),
                new ColorComponent(this, "HSV_V", 0, 100, "%",
                    (k) => new ColorSpaces.HSV(GetCC(6), GetCC(7), k / 100.0).GetRGB().GetColor()),
                new ColorComponent(this, "LAB_L", 0, 100, null,
                    (k) => new ColorSpaces.LAB(k, GetCC(10), GetCC(11)).GetRGB().GetColor()),
                new ColorComponent(this, "LAB_A", -127.5, 127.5, null,
                    (k) => new ColorSpaces.LAB(GetCC(9), (k / 100.0 - 0.5) * 255, GetCC(11)).GetRGB().GetColor()),
                new ColorComponent(this, "LAB_B", -127.5, 127.5, null,
                    (k) => new ColorSpaces.LAB(GetCC(9), GetCC(10), (k / 100.0 - 0.5) * 255).GetRGB().GetColor()),
                new ColorComponent(this, "YCbCr_Y", 0, 255, null,
                    (k) => new ColorSpaces.YCbCr(k / 100.0, GetCC(13), GetCC(14)).GetRGB().GetColor()),
                new ColorComponent(this, "YCbCr_Cb", -127.5, 127.5, null,
                    (k) => new ColorSpaces.YCbCr(GetCC(12), k / 100.0 - 0.5, GetCC(14)).GetRGB().GetColor()),
                new ColorComponent(this, "YCbCr_Cr", -127.5, 127.5, null,
                    (k) => new ColorSpaces.YCbCr(GetCC(12), GetCC(13), k / 100.0 - 0.5).GetRGB().GetColor()),
            };

            const int NumberOfTones = 10;
            Tones = new ColorToneBox[3 * NumberOfTones];
            for (var k1 = 0; k1 < 3; k1++)
            for (var k2 = 0; k2 < NumberOfTones; k2++)
                Tones[k2 + k1 * NumberOfTones] = new ColorToneBox(this, k1, k2);
        }

        #region ==============  Color Component  ===============
        public class ColorComponent : XYSlider
        {
            private double _value;
            public double Value
            {
                get => _value;
                set
                {
                    _value = Math.Max(Min, Math.Min(Max, value));
                    _owner.UpdateValues(ColorSpace);
                }
            }

            public readonly string Id;
            public string Label => Id.Split('_')[1];
            public string ValueLabel { get; }
            public LinearGradientBrush BackgroundBrush { get; }
            public readonly double Min;
            public readonly double Max;
            public readonly double SpaceMultiplier;
            internal ColorSpace ColorSpace;
            private ColorPickerVM _owner;
            private int _gradientCount => ColorSpace == ColorSpace.RGB ? 1 : 100;
            private Func<int, Color> _backgroundGradient;

            public ColorComponent(ColorPickerVM owner, string id, double min, double max, string valueLabel = null,
                Func<int, Color> backgroundGradient = null) : base(null)
            {
                SetValuesAction = (x, y) => SetProperties(x);

                Id = id; Min = min; Max = max;
                ValueLabel = valueLabel; _owner = owner;
                _backgroundGradient = backgroundGradient;
                SpaceMultiplier = Id.StartsWith("LAB") ? 1 : Max - Min;
                ColorSpace = (ColorSpace)Enum.Parse(typeof(ColorSpace), Id.Split('_')[0]);
                BackgroundBrush = new LinearGradientBrush(new GradientStopCollection(Enumerable.Range(0, _gradientCount + 1)
                    .Select(n => new GradientStop(Colors.Transparent, 1.0 * n / _gradientCount))));
            }

            public override void UpdateUI()
            {
                if (_backgroundGradient !=null)
                    for (var k = 0; k < BackgroundBrush.GradientStops.Count; k++)
                        BackgroundBrush.GradientStops[k].Color = _backgroundGradient(k);
                OnPropertiesChanged(nameof(xSliderValue), nameof(Value), nameof(BackgroundBrush));
            }

            public override double xSliderValue => (SizeOfSlider.Width - SizeOfThumb.Width) * (Value - Min) / (Max - Min);
            public void SetProperties(double sliderValue) => Value = (Max - Min) * sliderValue + Min;

            public double SpaceValue => Value / SpaceMultiplier;
            public void SetSpaceValue(double value, bool updateUI = false)
            {
                if (updateUI)
                    Value = value * SpaceMultiplier;
                else
                    _value = value * SpaceMultiplier;
            }
        }
        #endregion

        internal enum ColorSpace { RGB, HSL, HSV, LAB, YCbCr };
        internal CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;

        #region  ==============  Public Properties  ================
        public Color Color // Original color
        {
            get => Color.FromArgb(Convert.ToByte(_oldColorData[_oldColorData.Length - 1] * 255),
                Convert.ToByte(_oldColorData[0]), Convert.ToByte(_oldColorData[1]), Convert.ToByte(_oldColorData[2]));
            set
            {
                CurrentColor = value;
                SaveColor();
            }
        }

        public Color CurrentColor
        {
            get => Color.FromArgb(Convert.ToByte((1 - AlphaSlider.yValue) * 255), Convert.ToByte(GetCC(0) * 255),
                Convert.ToByte(GetCC(1) * 255), Convert.ToByte(GetCC(2) * 255));
            set
            {
                _isUpdating = true;
                AlphaSlider.SetProperties(0, 1.0 - value.A / 255.0);
                SetCC(0, value.R / 255.0, value.G / 255.0);
                _isUpdating = false;
                SetCC(2, value.B / 255.0);
            }
        }

        private SolidColorBrush[] _brushesCache = { new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush() };
        private SolidColorBrush GetCacheBrush(int index, Color color)
        {
            _brushesCache[index].Color = color;
            return _brushesCache[index];
        }
        public SolidColorBrush HueBrush => GetCacheBrush(0, new ColorSpaces.HSV(GetCC(6), 1, 1).GetRGB().GetColor());
        public SolidColorBrush Color_ForegroundBrush => GetCacheBrush(1, ColorSpaces.IsDarkColor(Color) ? Colors.White : Colors.Black);
        public SolidColorBrush CurrentColor_ForegroundBrush => GetCacheBrush(2, ColorSpaces.IsDarkColor(CurrentColor) ? Colors.White : Colors.Black);
        public SolidColorBrush ColorWithoutAlphaBrush => GetCacheBrush(3, new ColorSpaces.RGB(_oldColorData[0] / 255, _oldColorData[1] / 255, _oldColorData[2] / 255).GetColor());
        public SolidColorBrush CurrentColorWithoutAlphaBrush => GetCacheBrush(4, new ColorSpaces.RGB(GetCC(0), GetCC(1), GetCC(2)).GetColor());

        #endregion

        #region ==============  Update Values/UI  ===============
        // Get color component in space unit
        private double GetCC(int index) => Components[index].Value / Components[index].SpaceMultiplier;
        // Set color components from space unit
        internal void SetCC(int startIndex, params double[] newValues)
        {
            for (var k = 0; k < newValues.Length; k++)
                Components[k + startIndex].Value = newValues[k] * Components[k + startIndex].SpaceMultiplier;
        }

        private bool _isUpdating;
        private void UpdateValues(ColorSpace baseColorSpace)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            // Get rgb components
            var rgb = new ColorSpaces.RGB(RGB_R.SpaceValue, RGB_G.SpaceValue, RGB_B.SpaceValue);
            if (baseColorSpace == ColorSpace.HSL)
            {
                rgb = new ColorSpaces.HSL(HSL_H.SpaceValue, HSL_S.SpaceValue, HSL_L.SpaceValue).GetRGB();
                // Update HSV
                var hsv = new ColorSpaces.HSV(rgb); // _hsv = new ColorSpaces.HSV(_rgb);
                HSV_H.Value = HSL_H.Value;
                HSV_S.SetSpaceValue(hsv.S);
                HSV_V.SetSpaceValue(hsv.V);
            }
            else if (baseColorSpace == ColorSpace.HSV)
            {
                rgb = new ColorSpaces.HSV(HSV_H.SpaceValue, HSV_S.SpaceValue, HSV_V.SpaceValue).GetRGB();
                // Update HSL
                var hsl = new ColorSpaces.HSL(rgb); // _hsl = new ColorSpaces.HSL(_rgb);
                HSL_H.Value = HSV_H.Value;
                HSL_S.SetSpaceValue(hsl.S);
                HSL_L.SetSpaceValue(hsl.L);
            }
            else if (baseColorSpace == ColorSpace.LAB)
                rgb = new ColorSpaces.LAB(LAB_L.SpaceValue, LAB_A.SpaceValue, LAB_B.SpaceValue).GetRGB();
            else if (baseColorSpace == ColorSpace.YCbCr)
                rgb = new ColorSpaces.YCbCr(YCbCr_Y.SpaceValue, YCbCr_Cb.SpaceValue, YCbCr_Cr.SpaceValue).GetRGB();

            // Update other components
            if (baseColorSpace != ColorSpace.RGB)
            {
                RGB_R.SetSpaceValue(rgb.R);
                RGB_G.SetSpaceValue(rgb.G);
                RGB_B.SetSpaceValue(rgb.B);
            }
            if (baseColorSpace != ColorSpace.HSL && baseColorSpace != ColorSpace.HSV)
            {
                var hsl = new ColorSpaces.HSL(rgb);
                HSL_H.SetSpaceValue(hsl.H);
                HSL_S.SetSpaceValue(hsl.S);
                HSL_L.SetSpaceValue(hsl.L);
                var hsv = new ColorSpaces.HSV(rgb);
                HSV_H.SetSpaceValue(hsv.H);
                HSV_S.SetSpaceValue(hsv.S);
                HSV_V.SetSpaceValue(hsv.V);
            }
            if (baseColorSpace != ColorSpace.LAB)
            {
                var lab = new ColorSpaces.LAB(rgb);
                LAB_L.SetSpaceValue(lab.L);
                LAB_A.SetSpaceValue(lab.A);
                LAB_B.SetSpaceValue(lab.B);
            }
            if (baseColorSpace != ColorSpace.YCbCr)
            {
                var yCbCr = new ColorSpaces.YCbCr(rgb);
                YCbCr_Y.SetSpaceValue(yCbCr.Y);
                YCbCr_Cb.SetSpaceValue(yCbCr.Cb);
                YCbCr_Cr.SetSpaceValue(yCbCr.Cr);
            }

            UpdateUI();
            _isUpdating = false;
        }

        public override void UpdateUI()
        {
            AlphaSlider.UpdateUI();
            HueSlider.SetProperties(0, GetCC(6));
            SaturationAndValueSlider.SetProperties(GetCC(7), 1.0 - GetCC(8));
            foreach (var component in Components)
                component.UpdateUI();

            Owner.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                OnPropertiesChanged(nameof(CurrentColor), nameof(HueBrush), nameof(CurrentColor_ForegroundBrush),
                    nameof(CurrentColorWithoutAlphaBrush));
                foreach (var tone in Tones)
                    tone.UpdateUI();
            }));

        }
        #endregion

        #region ===========  Save & Restore color  =================

        private const int ComponentNumber = 15;
        private double[] _savedColorData = new double[ComponentNumber + 1];
        private double[] _oldColorData = new double[ComponentNumber + 1];

        internal void SaveColor()
        {
            /*_oldColorData.CopyTo(_savedColorData, 0);
            Array.Copy(_values, _oldColorData, ComponentNumber);
            _oldColorData[_savedColorData.Length - 1] = Alpha;
            OnPropertiesChanged(nameof(Color), nameof(Color_ForegroundBrush), nameof(ColorWithoutAlphaBrush));*/
        }
        internal void RestoreColor()
        {
            /*_savedColorData.CopyTo(_oldColorData, 0);
            Array.Copy(_savedColorData, _values, ComponentNumber);
            Alpha = _savedColorData[_savedColorData.Length - 1];
            OnPropertiesChanged(nameof(Color), nameof(Color_ForegroundBrush), nameof(ColorWithoutAlphaBrush));*/
        }
        #endregion

        #region ==============  Tones  =======================
        public class ColorToneBox: INotifyPropertyChangedAbstract
        {
            private readonly ColorPickerVM _owner;
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
                    var hsv = new ColorSpaces.HSV(rgb) { H = hsl.H };
                    var lab = new ColorSpaces.LAB(rgb);
                    var yCbCr = new ColorSpaces.YCbCr(rgb);

                    var sb = new StringBuilder();
                    sb.AppendLine("Gray level:" + FormatDouble(ColorSpaces.GetGrayLevel(rgb) * 100) + "%");
                    sb.AppendLine("HEX:".PadRight(5) + rgb.Color);
                    sb.AppendLine(FormatInfoString("RGB", rgb.R * 255, rgb.G * 255, rgb.B * 255));
                    sb.AppendLine(FormatInfoString("HSL", hsl.H * 360, hsl.S * 100, hsl.L * 100));
                    sb.AppendLine(FormatInfoString("HSV", hsv.H * 360, hsv.S * 100, hsv.V * 100));
                    sb.AppendLine(FormatInfoString("LAB", lab.L, lab.A, lab.B));
                    sb.Append(FormatInfoString("YCbCr", yCbCr.Y * 255, yCbCr.Cb * 255, yCbCr.Cr * 255));
                    return sb.ToString();
                }
            }

            public ColorToneBox(ColorPickerVM owner, int gridColumn, int gridRow)
            {
                _owner = owner;
                GridColumn = gridColumn;
                GridRow = gridRow;
            }

            internal ColorSpaces.HSL GetBackgroundHSL()
            {
                if (GridColumn == 0)
                    return new ColorSpaces.HSL(_owner.GetCC(3), _owner.GetCC(4), 0.025 + 0.05 * GridRow);
                if (GridColumn == 1)
                    return new ColorSpaces.HSL(_owner.GetCC(3), _owner.GetCC(5), 0.975 - 0.05 * GridRow);
                return new ColorSpaces.HSL(_owner.GetCC(3), 0.05 + 0.1 * GridRow, _owner.GetCC(5));
            }

            private string FormatInfoString(string label, double value1, double value2, double value3) =>
                (label + ":").PadRight(7) + FormatDouble(value1) + FormatDouble(value2) + FormatDouble(value3);
            private string FormatDouble(double value) => value.ToString("F1", _owner.CurrentCulture).PadLeft(7);
            public override void UpdateUI()
            {
                Background.Color = GetBackgroundHSL().GetRGB().GetColor();
                Foreground.Color = ColorSpaces.IsDarkColor(Background.Color) ? Colors.White : Colors.Black;
                OnPropertiesChanged(nameof(Background), nameof(Foreground));
            }
        }
        #endregion
    }
}
