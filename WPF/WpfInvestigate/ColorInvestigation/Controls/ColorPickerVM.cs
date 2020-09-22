using System;
using System.Diagnostics;
using System.Diagnostics;
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
        internal enum ColorSpace { RGB, HSL, HSV, LAB, YCbCr };
        internal CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;

        internal FrameworkElement Owner;

        // need to duplicate Component because "Type ColorComponent[]' is not collection" error occurs
        // for "Content={Binding Components[N]}" line of ColorPicker.xaml in VS designer
        // I tried to use Array, Collection<T>, List<T>, ReadOnlyCollection<T>, Dictionary<T,V>
        public ColorComponent RGB_R => (ColorComponent)Sliders[0];
        public ColorComponent RGB_G => (ColorComponent)Sliders[1];
        public ColorComponent RGB_B => (ColorComponent)Sliders[2]; 
        public ColorComponent HSL_H => (ColorComponent)Sliders[3];
        public ColorComponent HSL_S => (ColorComponent)Sliders[4]; 
        public ColorComponent HSL_L => (ColorComponent)Sliders[5];
        public ColorComponent HSV_H => (ColorComponent)Sliders[6];
        public ColorComponent HSV_S => (ColorComponent)Sliders[7];
        public ColorComponent HSV_V => (ColorComponent)Sliders[8];
        public ColorComponent LAB_L => (ColorComponent)Sliders[9];
        public ColorComponent LAB_A => (ColorComponent)Sliders[10];
        public ColorComponent LAB_B => (ColorComponent)Sliders[11];
        public ColorComponent YCbCr_Y => (ColorComponent)Sliders[12];
        public ColorComponent YCbCr_Cb => (ColorComponent)Sliders[13];
        public ColorComponent YCbCr_Cr => (ColorComponent)Sliders[14];
        public XYSlider AlphaSlider => Sliders[15];
        public XYSlider HueSlider => Sliders[16];
        public XYSlider SaturationAndValueSlider => Sliders[17];

        public ColorToneBox[] Tones { get; }

        private XYSlider[] Sliders { get; }
        public ColorPickerVM()
        {
            Sliders = new []
            {
                new ColorComponent("RGB_R", this, 0, 255, null,
                    (k) => Color.FromRgb(Convert.ToByte(255 * k), CurrentColor.G, CurrentColor.B)),
                new ColorComponent("RGB_G", this, 0, 255, null,
                    (k) => Color.FromRgb(CurrentColor.R, Convert.ToByte(255 * k), CurrentColor.B)),
                new ColorComponent("RGB_B", this, 0, 255, null,
                    (k) => Color.FromRgb(CurrentColor.R, CurrentColor.G, Convert.ToByte(255 * k))),
                new ColorComponent("HSL_H", this, 0, 360, "°",
                    (k) => new ColorSpaces.HSL(k / 100.0, HSL_S.SpaceValue, HSL_L.SpaceValue).GetRGB().GetColor()),
                new ColorComponent("HSL_S", this, 0, 100, "%",
                    (k) => new ColorSpaces.HSL(HSL_H.SpaceValue, k / 100.0, HSL_L.SpaceValue).GetRGB().GetColor()),
                new ColorComponent("HSL_L", this, 0, 100, "%",
                    (k) => new ColorSpaces.HSL(HSL_H.SpaceValue,HSL_S.SpaceValue, k / 100.0).GetRGB().GetColor()),
                new ColorComponent("HSV_H", this, 0, 360, "°",
                    (k) => new ColorSpaces.HSV(k / 100.0, HSV_S.SpaceValue, HSV_V.SpaceValue).GetRGB().GetColor()),
                new ColorComponent("HSV_S", this, 0, 100, "%",
                    (k) => new ColorSpaces.HSV(HSV_H.SpaceValue, k / 100.0, HSV_V.SpaceValue).GetRGB().GetColor()),
                new ColorComponent("HSV_V/B", this, 0, 100, "%",
                    (k) => new ColorSpaces.HSV(HSV_H.SpaceValue, HSV_S.SpaceValue, k / 100.0).GetRGB().GetColor()),
                new ColorComponent("LAB_L", this, 0, 100, null,
                    (k) => new ColorSpaces.LAB(k, LAB_A.SpaceValue, LAB_B.SpaceValue).GetRGB().GetColor()),
                new ColorComponent("LAB_A", this, -127.5, 127.5, null,
                    (k) => new ColorSpaces.LAB(LAB_L.SpaceValue, (k / 100.0 - 0.5) * 255, LAB_B.SpaceValue).GetRGB().GetColor()),
                new ColorComponent("LAB_B", this, -127.5, 127.5, null,
                    (k) => new ColorSpaces.LAB(LAB_L.SpaceValue, LAB_A.SpaceValue, (k / 100.0 - 0.5) * 255).GetRGB().GetColor()),
                new ColorComponent("YCbCr_Y", this, 0, 255, null,
                    (k) => new ColorSpaces.YCbCr(k / 100.0, YCbCr_Cb.SpaceValue, YCbCr_Cr.SpaceValue).GetRGB().GetColor()),
                new ColorComponent("YCbCr_Cb", this, -127.5, 127.5, null,
                    (k) => new ColorSpaces.YCbCr(YCbCr_Y.SpaceValue, k / 100.0 - 0.5, YCbCr_Cr.SpaceValue).GetRGB().GetColor()),
                new ColorComponent("YCbCr_Cr", this, -127.5, 127.5, null,
                    (k) => new ColorSpaces.YCbCr(YCbCr_Y.SpaceValue, YCbCr_Cb.SpaceValue, k / 100.0 - 0.5).GetRGB().GetColor()),
                new XYSlider("Alpha", (x, y) => UpdateUI()), 
                new XYSlider("Hue", (x, y) => HSV_H.SetSpaceValue(y, true)),
                new XYSlider("SaturationAndValue", (x, y) =>
                {
                    HSV_S.SetSpaceValue(x);
                    HSV_V.SetSpaceValue(1.0 - y, true);
                })
            };

            const int numberOfTones = 10;
            Tones = new ColorToneBox[3 * numberOfTones];
            for (var k1 = 0; k1 < 3; k1++)
            for (var k2 = 0; k2 < numberOfTones; k2++)
                Tones[k2 + k1 * numberOfTones] = new ColorToneBox(this, k1, k2);
        }

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
            get => Color.FromArgb(Convert.ToByte((1 - AlphaSlider.yValue) * 255), Convert.ToByte(RGB_R.Value),
                Convert.ToByte(RGB_G.Value), Convert.ToByte(RGB_B.Value));
            set
            {
                _isUpdating = true;
                AlphaSlider.SetProperties(0, 1.0 - value.A / 255.0);
                RGB_R.Value = value.R;
                RGB_G.Value = value.G;
                _isUpdating = false;
                RGB_B.Value = value.B;
            }
        }

        private SolidColorBrush[] _brushesCache = { new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush() };
        private SolidColorBrush GetCacheBrush(int index, Color color)
        {
            _brushesCache[index].Color = color;
            return _brushesCache[index];
        }
        public SolidColorBrush HueBrush => GetCacheBrush(0, new ColorSpaces.HSV(HSV_H.SpaceValue, 1, 1).GetRGB().GetColor());
        public SolidColorBrush Color_ForegroundBrush => GetCacheBrush(1, ColorSpaces.GetForegroundColor(Color));
        public SolidColorBrush CurrentColor_ForegroundBrush => GetCacheBrush(2, ColorSpaces.GetForegroundColor(CurrentColor));
        public SolidColorBrush ColorWithoutAlphaBrush => GetCacheBrush(3, new ColorSpaces.RGB(_oldColorData[0] / 255, _oldColorData[1] / 255, _oldColorData[2] / 255).GetColor());
        public SolidColorBrush CurrentColorWithoutAlphaBrush => GetCacheBrush(4, new ColorSpaces.RGB(RGB_R.SpaceValue, RGB_G.SpaceValue, RGB_B.SpaceValue).GetColor());
        #endregion

        #region ==============  Update Values/UI  ===============
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

            HueSlider.SetProperties(0, HSV_H.SpaceValue, false);
            SaturationAndValueSlider.SetProperties(HSV_S.SpaceValue, 1.0 - HSV_V.SpaceValue, false);

            UpdateUI();
            _isUpdating = false;
        }

        public override void UpdateUI()
        {
            foreach (var component in Sliders)
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

        #region ===================  SUBCLASSES  ========================
        #region ==============  Hue/SaturationAndValue Sliders  ============
        public class XYSlider : INotifyPropertyChangedAbstract
        {
            public string Id { get; }
            public double xValue { get; private set; }
            public double yValue { get; private set; }
            public virtual double xSliderValue => SizeOfSlider.Width * xValue - SizeOfThumb.Width / 2;
            public virtual double ySliderValue => SizeOfSlider.Height * yValue - SizeOfThumb.Height / 2;

            protected Action<double, double> SetValuesAction;

            protected Size SizeOfSlider;
            protected Size SizeOfThumb;

            public XYSlider(string id, Action<double, double> setValuesAction)
            {
                Id = id; SetValuesAction = setValuesAction;
            }
            public void SetProperties(double xValue, double yValue, bool updateUI = false)
            {
                this.xValue = xValue;
                this.yValue = yValue;
                SetValuesAction?.Invoke(xValue, yValue);
                if (updateUI)
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

            public string Label => Id.Split('_')[1];
            public string ValueLabel { get; }
            public LinearGradientBrush BackgroundBrush { get; }

            // private readonly string _id;
            private ColorPickerVM _owner;
            private readonly double _spaceMultiplier;
            private readonly double Min;
            private readonly double Max;
            private ColorSpace ColorSpace;
            private int _gradientCount => ColorSpace == ColorSpace.RGB ? 1 : 100;
            private Func<int, Color> _backgroundGradient;

            public ColorComponent(string id, ColorPickerVM owner, double min, double max, string valueLabel = null,
                Func<int, Color> backgroundGradient = null) : base(id, null)
            {
                SetValuesAction = (x, y) => Value = xValue * _spaceMultiplier;
                _owner = owner;
                Min = min;
                Max = max;
                ValueLabel = valueLabel;
                _backgroundGradient = backgroundGradient;
                _spaceMultiplier = Id.StartsWith("LAB") ? 1 : Max - Min;
                ColorSpace = (ColorSpace)Enum.Parse(typeof(ColorSpace), Id.Split('_')[0]);
                BackgroundBrush = new LinearGradientBrush(new GradientStopCollection(Enumerable.Range(0, _gradientCount + 1)
                    .Select(n => new GradientStop(Colors.Transparent, 1.0 * n / _gradientCount))));
            }

            public override void UpdateUI()
            {
                if (_backgroundGradient != null)
                    for (var k = 0; k < BackgroundBrush.GradientStops.Count; k++)
                        BackgroundBrush.GradientStops[k].Color = _backgroundGradient(k);
                OnPropertiesChanged(nameof(xSliderValue), nameof(Value), nameof(BackgroundBrush));
            }

            public override double xSliderValue => (SizeOfSlider.Width - SizeOfThumb.Width) * (Value - Min) / (Max - Min);

            public double SpaceValue => Value / _spaceMultiplier;
            public void SetSpaceValue(double value, bool updateUI = false)
            {
                if (updateUI)
                    Value = value * _spaceMultiplier;
                else
                    _value = value * _spaceMultiplier;
            }
        }
        #endregion

        #region ==============  ColorToneBox  =======================
        public class ColorToneBox : INotifyPropertyChangedAbstract
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
                    var hsl = GetBackgroundHSL();
                    var rgb = hsl.GetRGB();
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

            public override void UpdateUI()
            {
                var hsl = GetBackgroundHSL();
                Background.Color = hsl.GetRGB().GetColor();
                Foreground.Color = ColorSpaces.GetForegroundColor(Background.Color);
                OnPropertiesChanged(nameof(Background), nameof(Foreground));
            }

            public void SetCurrentColor() =>
                _owner.CurrentColor = GetBackgroundHSL().GetRGB().GetColor(1 - _owner.AlphaSlider.yValue);

            private ColorSpaces.HSL GetBackgroundHSL()
            {
                if (GridColumn == 0)
                    return new ColorSpaces.HSL(_owner.HSL_H.SpaceValue, _owner.HSL_S.SpaceValue, 0.025 + 0.05 * GridRow);
                if (GridColumn == 1)
                    return new ColorSpaces.HSL(_owner.HSL_H.SpaceValue, _owner.HSL_S.SpaceValue, 0.975 - 0.05 * GridRow);
                return new ColorSpaces.HSL(_owner.HSL_H.SpaceValue, 0.05 + 0.1 * GridRow, _owner.HSL_L.SpaceValue);
            }

            private string FormatInfoString(string label, double value1, double value2, double value3) =>
                (label + ":").PadRight(7) + FormatDouble(value1) + FormatDouble(value2) + FormatDouble(value3);
            private string FormatDouble(double value) => value.ToString("F1", _owner.CurrentCulture).PadLeft(7);
        }
        #endregion
        #endregion
    }
}
