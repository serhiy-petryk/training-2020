using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Common.ColorSpaces;

namespace WpfSpLib.Controls
{
    // ColorControl ViewModel for DataTemplate
    public class ColorControlViewModel : NotifyPropertyChangedAbstract
    {
        internal enum ColorSpace { RGB, HSL, HSV, LAB, YCbCr };
        internal CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;

        // need to duplicate Sliders because "Type ColorComponent[]' is not collection" error occurs
        // for "Content={Binding Sliders[N]}" line of ColorControl.xaml in VS designer
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
        public ColorControlViewModel()
        {
            Sliders = new[]
            {
                new ColorComponent("RGB_R", this, 0, 255, null,
                    (k) => Color.FromRgb(Convert.ToByte(255 * k), Color.G, Color.B)),
                new ColorComponent("RGB_G", this, 0, 255, null,
                    (k) => Color.FromRgb(Color.R, Convert.ToByte(255 * k), Color.B)),
                new ColorComponent("RGB_B", this, 0, 255, null,
                    (k) => Color.FromRgb(Color.R, Color.G, Convert.ToByte(255 * k))),
                new ColorComponent("HSL_H", this, 0, 360, "°",
                    (k) => new HSL(k / 100.0, HSL_S.SpaceValue, HSL_L.SpaceValue).RGB.Color),
                new ColorComponent("HSL_S", this, 0, 100, "%",
                    (k) => new HSL(HSL_H.SpaceValue, k / 100.0, HSL_L.SpaceValue).RGB.Color),
                new ColorComponent("HSL_L", this, 0, 100, "%",
                    (k) => new HSL(HSL_H.SpaceValue,HSL_S.SpaceValue, k / 100.0).RGB.Color),
                new ColorComponent("HSV_H", this, 0, 360, "°",
                    (k) => new HSV(k / 100.0, HSV_S.SpaceValue, HSV_V.SpaceValue).RGB.Color),
                new ColorComponent("HSV_S", this, 0, 100, "%",
                    (k) => new HSV(HSV_H.SpaceValue, k / 100.0, HSV_V.SpaceValue).RGB.Color),
                new ColorComponent("HSV_V/B", this, 0, 100, "%",
                    (k) => new HSV(HSV_H.SpaceValue, HSV_S.SpaceValue, k / 100.0).RGB.Color),
                new ColorComponent("LAB_L", this, 0, 100, null,
                    (k) => new LAB(k, LAB_A.SpaceValue, LAB_B.SpaceValue).RGB.Color),
                new ColorComponent("LAB_A", this, -127.5, 127.5, null,
                    (k) => new LAB(LAB_L.SpaceValue, (k / 100.0 - 0.5) * 255, LAB_B.SpaceValue).RGB.Color),
                new ColorComponent("LAB_B", this, -127.5, 127.5, null,
                    (k) => new LAB(LAB_L.SpaceValue, LAB_A.SpaceValue, (k / 100.0 - 0.5) * 255).RGB.Color),
                new ColorComponent("YCbCr_Y", this, 0, 255, null,
                    (k) => new YCbCr(k / 100.0, YCbCr_Cb.SpaceValue, YCbCr_Cr.SpaceValue).RGB.Color),
                new ColorComponent("YCbCr_Cb", this, -127.5, 127.5, null,
                    (k) => new YCbCr(YCbCr_Y.SpaceValue, k / 100.0 - 0.5, YCbCr_Cr.SpaceValue).RGB.Color),
                new ColorComponent("YCbCr_Cr", this, -127.5, 127.5, null,
                    (k) => new YCbCr(YCbCr_Y.SpaceValue, YCbCr_Cb.SpaceValue, k / 100.0 - 0.5).RGB.Color),
                new XYSlider("Alpha", (x, y) => UpdateUI()),
                new XYSlider("Hue", (x, y) => HSV_H.SetSpaceValue(y, true)),
                new XYSlider("SaturationAndValue", (x, y) =>
                {
                    HSV_S.SetSpaceValue(x);
                    HSV_V.SetSpaceValue(1.0 - y, true);
                })
            };

            const int numberOfTones = 11;
            Tones = new ColorToneBox[3 * numberOfTones];
            for (var k1 = 0; k1 < 3; k1++)
                for (var k2 = 0; k2 < numberOfTones; k2++)
                    Tones[k2 + k1 * numberOfTones] = new ColorToneBox(this, k1, k2);
        }

        #region  ==============  Public Properties  ================

        private bool _isAlphaSliderVisible;
        public bool IsAlphaSliderVisible
        {
            get => _isAlphaSliderVisible;
            set
            {
                _isAlphaSliderVisible = value;
                OnPropertiesChanged(nameof(IsAlphaSliderVisible));
                UpdateUI();
            }
        }

        public Color Color
        {
            get => Color.FromArgb(Convert.ToByte((1 - AlphaSlider.yValue) * 255), Convert.ToByte(RGB_R.Value),
                Convert.ToByte(RGB_G.Value), Convert.ToByte(RGB_B.Value));
            set
            {
                // _isUpdating = true;
                AlphaSlider.SetProperties(0, 1.0 - value.A / 255.0);
                // _isUpdating = false;
                RGB_R.Value = value.R;
                RGB_G.Value = value.G;
                RGB_B.Value = value.B;
                OnPropertiesChanged(nameof(Color));
            }
        }

        private SolidColorBrush[] _brushesCache = { new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush(), new SolidColorBrush() };
        private SolidColorBrush GetCacheBrush(int index, Color color)
        {
            _brushesCache[index].Color = color;
            return _brushesCache[index];
        }
        public SolidColorBrush HueBrush => GetCacheBrush(0, new HSV(HSV_H.SpaceValue, 1, 1).RGB.Color);
        public SolidColorBrush Color_ForegroundBrush => GetCacheBrush(1, ColorUtils.GetForegroundColor(Color));
        public SolidColorBrush ColorWithoutAlphaBrush => GetCacheBrush(3, Color.FromRgb(Color.R, Color.G, Color.B));
        public double ColorGrayLevel => ColorUtils.GetGrayLevel(new RGB(Color)) * 100;
        #endregion

        #region ==============  Update Values/UI  ===============
        private bool _isUpdating;
        private void UpdateValues(ColorSpace baseColorSpace)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            // Get rgb components
            var rgb = new RGB(RGB_R.SpaceValue, RGB_G.SpaceValue, RGB_B.SpaceValue);
            if (baseColorSpace == ColorSpace.HSL)
            {
                rgb = new HSL(HSL_H.SpaceValue, HSL_S.SpaceValue, HSL_L.SpaceValue).RGB;
                // Update HSV
                var hsv = new HSV(rgb); // _hsv = new ColorSpaces.HSV(_rgb);
                HSV_H.Value = HSL_H.Value;
                HSV_S.SetSpaceValue(hsv.S);
                HSV_V.SetSpaceValue(hsv.V);
            }
            else if (baseColorSpace == ColorSpace.HSV)
            {
                rgb = new HSV(HSV_H.SpaceValue, HSV_S.SpaceValue, HSV_V.SpaceValue).RGB;
                // Update HSL
                var hsl = new HSL(rgb); // _hsl = new ColorSpaces.HSL(_rgb);
                HSL_H.Value = HSV_H.Value;
                HSL_S.SetSpaceValue(hsl.S);
                HSL_L.SetSpaceValue(hsl.L);
            }
            else if (baseColorSpace == ColorSpace.LAB)
                rgb = new LAB(LAB_L.SpaceValue, LAB_A.SpaceValue, LAB_B.SpaceValue).RGB;
            else if (baseColorSpace == ColorSpace.YCbCr)
                rgb = new YCbCr(YCbCr_Y.SpaceValue, YCbCr_Cb.SpaceValue, YCbCr_Cr.SpaceValue).RGB;

            // Update other components
            if (baseColorSpace != ColorSpace.RGB)
            {
                RGB_R.SetSpaceValue(rgb.R);
                RGB_G.SetSpaceValue(rgb.G);
                RGB_B.SetSpaceValue(rgb.B);
            }
            if (baseColorSpace != ColorSpace.HSL && baseColorSpace != ColorSpace.HSV)
            {
                var hsl = new HSL(rgb);
                HSL_H.SetSpaceValue(hsl.H);
                HSL_S.SetSpaceValue(hsl.S);
                HSL_L.SetSpaceValue(hsl.L);
                var hsv = new HSV(rgb);
                HSV_H.SetSpaceValue(hsv.H);
                HSV_S.SetSpaceValue(hsv.S);
                HSV_V.SetSpaceValue(hsv.V);
            }
            if (baseColorSpace != ColorSpace.LAB)
            {
                var lab = new LAB(rgb);
                LAB_L.SetSpaceValue(lab.L);
                LAB_A.SetSpaceValue(lab.A);
                LAB_B.SetSpaceValue(lab.B);
            }
            if (baseColorSpace != ColorSpace.YCbCr)
            {
                var yCbCr = new YCbCr(rgb);
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

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                OnPropertiesChanged(nameof(Color), nameof(HueBrush), nameof(Color_ForegroundBrush),
                    nameof(ColorWithoutAlphaBrush), nameof(ColorGrayLevel));
                foreach (var tone in Tones)
                    tone.UpdateUI();
            }, DispatcherPriority.ContextIdle);

            foreach (var tone in Tones)
                tone.UpdateUI();
        }
        #endregion

        #region ===================  SUBCLASSES  ========================
        #region ==============  Hue/SaturationAndValue Sliders  ============
        public class XYSlider : NotifyPropertyChangedAbstract
        {
            protected string Id { get; }
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
                    _value = Math.Max(_min, Math.Min(_max, value));
                    _owner.UpdateValues(_colorSpace);
                }
            }

            public string Label => Id.Split('_')[1];
            public string ValueLabel { get; }
            public LinearGradientBrush BackgroundBrush { get; }

            private ColorControlViewModel _owner;
            private readonly double _spaceMultiplier;
            private readonly double _min;
            private readonly double _max;
            private ColorSpace _colorSpace;
            private int _gradientCount => _colorSpace == ColorSpace.RGB ? 1 : 100;
            private Func<int, Color> _backgroundGradient;

            public ColorComponent(string id, ColorControlViewModel owner, double min, double max, string valueLabel = null,
                Func<int, Color> backgroundGradient = null) : base(id, null)
            {
                SetValuesAction = (x, y) => Value = xValue * (_max - _min) + _min;
                _owner = owner;
                _min = min;
                _max = max;
                ValueLabel = valueLabel;
                _backgroundGradient = backgroundGradient;
                _spaceMultiplier = Id.StartsWith("LAB") ? 1 : _max - _min;
                _colorSpace = (ColorSpace)Enum.Parse(typeof(ColorSpace), Id.Split('_')[0]);
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

            public override double xSliderValue => (SizeOfSlider.Width - SizeOfThumb.Width) * (Value - _min) / (_max - _min);

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
        public class ColorToneBox : NotifyPropertyChangedAbstract
        {
            private readonly ColorControlViewModel _owner;
            public int GridColumn { get; }
            public int GridRow { get; }
            public SolidColorBrush Background { get; } = new SolidColorBrush();
            public SolidColorBrush Foreground { get; } = new SolidColorBrush();

            public string BoxLabel => _owner.IsAlphaSliderVisible ? Background.Color.ToString() : Background.Color.ToString().Remove(1,2);
            public string Info
            {
                get
                {
                    var hsl = GetBackgroundHSL();
                    var rgb = hsl.RGB;
                    var hsv = new HSV(rgb) { H = hsl.H };
                    var lab = new LAB(rgb);
                    var yCbCr = new YCbCr(rgb);
                    var color = rgb.Color;
                    var names = string.Join(", ", ColorUtils.GetKnownColors(false).Where(kvp => kvp.Value == color).Select(kvp => kvp.Key).ToArray());

                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(names))
                        sb.AppendLine((names.Contains(",") ? "Names: " : "Name: ") + names);
                    sb.AppendLine("Gray level:" + FormatDouble(ColorUtils.GetGrayLevel(rgb) * 100) + "%");
                    sb.AppendLine("HEX:".PadRight(5) + rgb.Color);
                    sb.AppendLine(FormatInfoString("RGB", rgb.R * 255, rgb.G * 255, rgb.B * 255));
                    sb.AppendLine(FormatInfoString("HSL", hsl.H * 360, hsl.S * 100, hsl.L * 100));
                    sb.AppendLine(FormatInfoString("HSV", hsv.H * 360, hsv.S * 100, hsv.V * 100));
                    sb.AppendLine(FormatInfoString("LAB", lab.L, lab.A, lab.B));
                    sb.Append(FormatInfoString("YCbCr", yCbCr.Y * 255, yCbCr.Cb * 255, yCbCr.Cr * 255));
                    return sb.ToString();
                }
            }

            public ColorToneBox(ColorControlViewModel owner, int gridColumn, int gridRow)
            {
                _owner = owner;
                GridColumn = gridColumn;
                GridRow = gridRow;
            }

            public override void UpdateUI()
            {
                var hsl = GetBackgroundHSL();
                Background.Color = hsl.RGB.Color;
                Foreground.Color = ColorUtils.GetForegroundColor(Background.Color);
                OnPropertiesChanged(nameof(Background), nameof(Foreground), nameof(Info), nameof(BoxLabel));
            }

            public void SetCurrentColor() =>
                _owner.Color = GetBackgroundHSL().RGB.GetColor(1 - _owner.AlphaSlider.yValue);

            private HSL GetBackgroundHSL()
            {
                if (GridColumn == 0)
                    return new HSL(_owner.HSL_H.SpaceValue, _owner.HSL_S.SpaceValue, 0.05 * GridRow);
                if (GridColumn == 1)
                    return new HSL(_owner.HSL_H.SpaceValue, _owner.HSL_S.SpaceValue, 1 - 0.05 * GridRow);
                return new HSL(_owner.HSL_H.SpaceValue, 0.1 * GridRow, _owner.HSL_L.SpaceValue);
            }

            private string FormatInfoString(string label, double value1, double value2, double value3) =>
                (label + ":").PadRight(7) + FormatDouble(value1) + FormatDouble(value2) + FormatDouble(value3);
            private string FormatDouble(double value) => value.ToString("F1", _owner.CurrentCulture).PadLeft(7);
        }
        #endregion
        #endregion
    }
}
