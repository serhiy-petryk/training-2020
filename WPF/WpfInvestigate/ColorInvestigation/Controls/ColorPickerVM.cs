using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Media;
using ColorInvestigation.Common;

namespace ColorInvestigation.Controls
{
    // ColorPicker ViewModel for DataTemplate
    public class ColorPickerVM : INotifyPropertyChanged
    {
        public ColorPickerVM()
        {
            Components = new List<ColorComponent>
            {
                new ColorComponent(this, "RGB_A", 0, 255, null, null),
                new ColorComponent(this, "RGB_R", 0, 255, null,
                    (k) => Color.FromRgb(Convert.ToByte(255 * k), CurrentColor.G, CurrentColor.B)),
                new ColorComponent(this, "RGB_G", 0, 255, null,
                    (k) => Color.FromRgb(CurrentColor.R, Convert.ToByte(255 * k), CurrentColor.B)),
                new ColorComponent(this, "RGB_B", 0, 255, null,
                    (k) => Color.FromRgb(CurrentColor.R, CurrentColor.G, Convert.ToByte(255 * k))),
                new ColorComponent(this, "HSL_H", 0, 360, "°",
                    (k) => new ColorSpaces.HSL(k / 360.0, GetCC(5), GetCC(6)).GetRGB().GetColor()),
                new ColorComponent(this, "HSL_S", 0, 100, "%",
                    (k) => new ColorSpaces.HSL(GetCC(4), k / 100.0, GetCC(6)).GetRGB().GetColor()),
                new ColorComponent(this, "HSL_L", 0, 100, "%",
                    (k) => new ColorSpaces.HSL(GetCC(4), GetCC(5), k / 100.0).GetRGB().GetColor()),
                new ColorComponent(this, "HSV_H", 0, 360, "°",
                (k) => new ColorSpaces.HSL(k / 360.0, GetCC(8), GetCC(9)).GetRGB().GetColor()),
                new ColorComponent(this, "HSV_S", 0, 100, "%",
                    (k) => new ColorSpaces.HSL(GetCC(7), k / 100.0, GetCC(9)).GetRGB().GetColor()),
                new ColorComponent(this, "HSV_V", 0, 100, "%",
                    (k) => new ColorSpaces.HSL(GetCC(7), GetCC(8), k / 100.0).GetRGB().GetColor())
            };
        }

        public List<ColorComponent> Components { get; }// = new List<ColorComponent>{ new ColorComponent(), new ColorComponent()};
        #region ==============  Color Component  ===============
        public class ColorComponent : INotifyPropertyChanged
        {
            private double _value;
            public double Value
            {
                get => _value;
                set
                {
                    _value = value;
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
            public double SliderControlSize;
            public double SliderControlOffset;
            private Func<int, Color> _backgroundGradient;

            public ColorComponent(ColorPickerVM owner, string id, double min, double max, string valueLabel, Func<int, Color> backgroundGradient)
            {
                Id = id; Min = min; Max = max;
                ValueLabel = valueLabel; _owner = owner;
                _backgroundGradient = backgroundGradient;
                SpaceMultiplier = Id.StartsWith("LAB") ? 1 : Max - Min;
                ColorSpace = (ColorSpace)Enum.Parse(typeof(ColorSpace), Id.Split('_')[0]);
                var gradientCount = ColorSpace == ColorSpace.RGB ? 1 : Convert.ToInt32(Max - Min);
                BackgroundBrush = new LinearGradientBrush(new GradientStopCollection(Enumerable
                    .Range(0, gradientCount + 1)
                    .Select(n => new GradientStop(Colors.Transparent, 1.0 * n / gradientCount))));
            }
            public void SetSliderValue(double sliderValue) => Value = (Max - Min) * sliderValue + Min;
            public double SliderValue => SliderControlSize * (Value - Min) / (Max - Min) - SliderControlOffset;

            public void UpdateUI()
            {
                if (_backgroundGradient !=null)
                    for (var k = 0; k < BackgroundBrush.GradientStops.Count; k++)
                        BackgroundBrush.GradientStops[k].Color = _backgroundGradient(k);
                OnPropertiesChanged(nameof(SliderValue), nameof(Value), nameof(BackgroundBrush));
            }

            #region ===========  INotifyPropertyChanged  ===============
            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertiesChanged(params string[] propertyNames)
            {
                foreach (var propertyName in propertyNames)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion

        }
        #endregion

        internal enum ColorSpace { RGB, HSL, HSV, LAB, YCbCr };

        private const int ComponentNumber = 15;
        internal CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;
        internal Action AfterUpdatedCallback;

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
            // get => Color.FromArgb(Convert.ToByte(Alpha * 255), Convert.ToByte(RGB_R), Convert.ToByte(RGB_G),
               // Convert.ToByte(RGB_B));
            get => Color.FromArgb(Convert.ToByte(Components[0].Value), Convert.ToByte(Components[1].Value), Convert.ToByte(Components[2].Value),
                Convert.ToByte(Components[3].Value));
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
        public SolidColorBrush HueBrush => GetCacheBrush(0, new ColorSpaces.HSV(GetCC_Old(6), 1, 1).GetRGB().GetColor());
        public SolidColorBrush Color_ForegroundBrush => GetCacheBrush(1, ColorSpaces.IsDarkColor(Color) ? Colors.White : Colors.Black);
        public SolidColorBrush CurrentColor_ForegroundBrush => GetCacheBrush(2, ColorSpaces.IsDarkColor(CurrentColor) ? Colors.White : Colors.Black);
        public SolidColorBrush ColorWithoutAlphaBrush => GetCacheBrush(3, new ColorSpaces.RGB(_oldColorData[0] / 255, _oldColorData[1] / 255, _oldColorData[2] / 255).GetColor());
        public SolidColorBrush CurrentColorWithoutAlphaBrush => GetCacheBrush(4, new ColorSpaces.RGB(GetCC_Old(0), GetCC_Old(1), GetCC_Old(2)).GetColor());

        #endregion

        #region  ===========  Color component public Properties  ============
        private double _alpha;
        private double[] _values = new double[ComponentNumber];

        // Get color component in space unit
        private double GetCC(int index) => Components[index].Value / Components[index].SpaceMultiplier;
        private double GetCC_Old(int index) => _values[index] / Metalist[index].SpaceMultiplier;
        // Set color components from space unit
        internal void SetCC_Old(int startIndex, params double[] newValues)
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
        internal Tuple<double, double> HSV_S_And_V // set HSV.S & HSV.V simultaneously (for HueAndSaturation slider)
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
        internal void SetProperty(double value, [CallerMemberName]string propertyName = null)
        {
            var meta = Metadata[propertyName];
            value = Math.Max(meta.Min, Math.Min(meta.Max, value));
            _values[meta.SeqNo] = value;
            UpdateValues(meta.ColorSpace);
        }
        #endregion

        #region ===========  Color component metadata  ============
        internal static Dictionary<string, MetaItem> Metadata;

        static ColorPickerVM()
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
            // new MetaItem(nameof(Alpha), 0, 255),
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

        internal class MetaItem
        {
            public readonly string Id;
            public int SeqNo;
            public readonly double Min;
            public readonly double Max;
            public readonly double SpaceMultiplier;
            internal ColorSpace ColorSpace;
            public double GetValue(ColorPickerVM VM) => VM._values[SeqNo];

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
            if (_isUpdating) return;
            _isUpdating = true;

            // Get rgb object
            var rgb = new ColorSpaces.RGB(0, 0, 0);
            if (baseColorSpace == ColorSpace.RGB)
                rgb = new ColorSpaces.RGB(GetCC_Old(0), GetCC_Old(1), GetCC_Old(2));
            else if (baseColorSpace == ColorSpace.HSL)
            {
                rgb = new ColorSpaces.HSL(GetCC_Old(3), GetCC_Old(4), GetCC_Old(5)).GetRGB();
                // Update HSV
                _values[6] = _values[3]; // _hsv.H = _hsl.H;
                var hsv = new ColorSpaces.HSV(rgb); // _hsv = new ColorSpaces.HSV(_rgb);
                SetCC_Old(7, hsv.S, hsv.V);
            }
            else if (baseColorSpace == ColorSpace.HSV)
            {
                rgb = new ColorSpaces.HSV(GetCC_Old(6), GetCC_Old(7), GetCC_Old(8)).GetRGB();
                // Update HSL
                _values[3] = _values[6]; // _hsl.H = _hsv.H;
                var hsl = new ColorSpaces.HSL(rgb); // _hsl = new ColorSpaces.HSL(_rgb);
                SetCC_Old(4, hsl.S, hsl.L);
            }
            else if (baseColorSpace == ColorSpace.LAB)
                rgb = new ColorSpaces.LAB(GetCC_Old(9), GetCC_Old(10), GetCC_Old(11)).GetRGB();
            else if (baseColorSpace == ColorSpace.YCbCr)
                rgb = new ColorSpaces.YCbCr(GetCC_Old(12), GetCC_Old(13), GetCC_Old(14)).GetRGB();

            // Update other objects
            if (baseColorSpace != ColorSpace.RGB)
                SetCC_Old(0, rgb.R, rgb.G, rgb.B);
            if (baseColorSpace != ColorSpace.HSL && baseColorSpace != ColorSpace.HSV)
            {
                var hsl = new ColorSpaces.HSL(rgb);
                SetCC_Old(3, hsl.H, hsl.S, hsl.L);
                var hsv = new ColorSpaces.HSV(rgb);
                SetCC_Old(6, hsv.H, hsv.S, hsv.V);
            }
            if (baseColorSpace != ColorSpace.LAB)
            {
                var lab = new ColorSpaces.LAB(rgb);
                SetCC_Old(9, lab.L, lab.A, lab.B);
            }
            if (baseColorSpace != ColorSpace.YCbCr)
            {
                var yCbCr = new ColorSpaces.YCbCr(rgb);
                SetCC_Old(12, yCbCr.Y, yCbCr.Cb, yCbCr.Cr);
            }

            UpdateUI();
            _isUpdating = false;
        }

        internal void UpdateUI()
        {
            UpdateTones();
            OnPropertiesChanged(Metadata.Keys.ToArray());
            OnPropertiesChanged(nameof(CurrentColor), nameof(HueBrush), nameof(CurrentColor_ForegroundBrush),
                nameof(CurrentColorWithoutAlphaBrush));

            AfterUpdatedCallback?.Invoke();
            // NewUpdateSliderBrushes();

            foreach (var component in Components)
                component.UpdateUI();
        }

        #endregion

        #region ===========  Save & Restore color  =================

        private double[] _savedColorData = new double[ComponentNumber + 1];
        private double[] _oldColorData = new double[ComponentNumber + 1];

        internal void SaveColor()
        {
            _oldColorData.CopyTo(_savedColorData, 0);
            Array.Copy(_values, _oldColorData, ComponentNumber);
            _oldColorData[_savedColorData.Length - 1] = Alpha;
            OnPropertiesChanged(nameof(Color), nameof(Color_ForegroundBrush), nameof(ColorWithoutAlphaBrush));
        }
        internal void RestoreColor()
        {
            _savedColorData.CopyTo(_oldColorData, 0);
            Array.Copy(_savedColorData, _values, ComponentNumber);
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
                Brushes["HSL_S"].GradientStops[k].Color = new ColorSpaces.HSL(GetCC_Old(3), k / 100.0, GetCC_Old(5)).GetRGB().GetColor();
                Brushes["HSL_L"].GradientStops[k].Color = new ColorSpaces.HSL(GetCC_Old(3), GetCC_Old(4), k / 100.0).GetRGB().GetColor();
                Brushes["HSV_S"].GradientStops[k].Color = new ColorSpaces.HSV(GetCC_Old(6), k / 100.0, GetCC_Old(8)).GetRGB().GetColor();
                Brushes["HSV_V"].GradientStops[k].Color = new ColorSpaces.HSV(GetCC_Old(6), GetCC_Old(7), k / 100.0).GetRGB().GetColor();
                Brushes["LAB_L"].GradientStops[k].Color = new ColorSpaces.LAB(k, GetCC_Old(10), GetCC_Old(11)).GetRGB().GetColor();
            }
            for (var k = 0; k <= 255; k++)
            {
                Brushes["LAB_A"].GradientStops[k].Color = new ColorSpaces.LAB(GetCC_Old(9), k - 127.5, GetCC_Old(11)).GetRGB().GetColor();
                Brushes["LAB_B"].GradientStops[k].Color = new ColorSpaces.LAB(GetCC_Old(9), GetCC_Old(10), k - 127.5).GetRGB().GetColor();
                Brushes["YCbCr_Y"].GradientStops[k].Color = new ColorSpaces.YCbCr(k / 255.0, GetCC_Old(13), GetCC_Old(14)).GetRGB().GetColor();
                Brushes["YCbCr_Cb"].GradientStops[k].Color = new ColorSpaces.YCbCr(GetCC_Old(12), (k - 127.5) / 255, GetCC_Old(14)).GetRGB().GetColor();
                Brushes["YCbCr_Cr"].GradientStops[k].Color = new ColorSpaces.YCbCr(GetCC_Old(12), GetCC_Old(13), (k - 127.5) / 255).GetRGB().GetColor();
            }
            for (var k = 0; k <= 360; k++)
            {
                Brushes["HSL_H"].GradientStops[k].Color = new ColorSpaces.HSL(k / 360.0, GetCC_Old(4), GetCC_Old(5)).GetRGB().GetColor();
                Brushes["HSV_H"].GradientStops[k].Color = new ColorSpaces.HSV(k / 360.0, GetCC_Old(7), GetCC_Old(8)).GetRGB().GetColor();
            }
            OnPropertiesChanged(nameof(Brushes));
        }

        private void NewUpdateSliderBrushes()
        {
            const int gradientStopsNumber = 100;
            if (Brushes == null)
            {
                Brushes = new Dictionary<string, LinearGradientBrush>();
                foreach (var kvp in Metadata)
                {
                    var gradientCount = kvp.Value.ColorSpace == ColorSpace.RGB ? 1 : gradientStopsNumber;
                    Brushes.Add(kvp.Key,
                        new LinearGradientBrush(new GradientStopCollection(Enumerable.Range(0, gradientCount + 1)
                            .Select(n => new GradientStop(Colors.Transparent, 1.0 * n / gradientCount)))));
                }
            }

            for (var k = 0; k < 2; k++)
            {
                var component = k == 0 ? (byte)0x0 : (byte)0xff;
                Brushes["RGB_R"].GradientStops[k].Color = Color.FromRgb(component, CurrentColor.G, CurrentColor.B);
                Brushes["RGB_G"].GradientStops[k].Color = Color.FromRgb(CurrentColor.R, component, CurrentColor.B);
                Brushes["RGB_B"].GradientStops[k].Color = Color.FromRgb(CurrentColor.R, CurrentColor.G, component);
            }

            const double xStep = 1.0 / (gradientStopsNumber + 1);
            var x = 0.0;
            for (var k = 0; k <= gradientStopsNumber; k++)
            {
                Brushes["HSL_H"].GradientStops[k].Color = new ColorSpaces.HSL(x, GetCC_Old(4), GetCC_Old(5)).GetRGB().GetColor();
                Brushes["HSL_S"].GradientStops[k].Color = new ColorSpaces.HSL(GetCC_Old(3), x, GetCC_Old(5)).GetRGB().GetColor();
                Brushes["HSL_L"].GradientStops[k].Color = new ColorSpaces.HSL(GetCC_Old(3), GetCC_Old(4), x).GetRGB().GetColor();
                Brushes["HSV_H"].GradientStops[k].Color = new ColorSpaces.HSV(x, GetCC_Old(7), GetCC_Old(8)).GetRGB().GetColor();
                Brushes["HSV_S"].GradientStops[k].Color = new ColorSpaces.HSV(GetCC_Old(6), x, GetCC_Old(8)).GetRGB().GetColor();
                Brushes["HSV_V"].GradientStops[k].Color = new ColorSpaces.HSV(GetCC_Old(6), GetCC_Old(7), x).GetRGB().GetColor();
                Brushes["LAB_L"].GradientStops[k].Color = new ColorSpaces.LAB(x * 100, GetCC_Old(10), GetCC_Old(11)).GetRGB().GetColor();
                Brushes["LAB_A"].GradientStops[k].Color = new ColorSpaces.LAB(GetCC_Old(9), x * 256 - 127.5, GetCC_Old(11)).GetRGB().GetColor();
                Brushes["LAB_B"].GradientStops[k].Color = new ColorSpaces.LAB(GetCC_Old(9), GetCC_Old(10), x * 256 - 127.5).GetRGB().GetColor();
                Brushes["YCbCr_Y"].GradientStops[k].Color = new ColorSpaces.YCbCr(x, GetCC_Old(13), GetCC_Old(14)).GetRGB().GetColor();
                Brushes["YCbCr_Cb"].GradientStops[k].Color = new ColorSpaces.YCbCr(GetCC_Old(12), x - 0.5, GetCC_Old(14)).GetRGB().GetColor();
                Brushes["YCbCr_Cr"].GradientStops[k].Color = new ColorSpaces.YCbCr(GetCC_Old(12), GetCC_Old(13), x - 0.5).GetRGB().GetColor();
                x += xStep;
            }
            OnPropertiesChanged(nameof(Brushes));
        }
        #endregion

        #region ==============  Tones  =======================
        public class ColorToneBox
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
                    return new ColorSpaces.HSL(_owner.GetCC_Old(3), _owner.GetCC_Old(4), 0.025 + 0.05 * GridRow);
                if (GridColumn == 1)
                    return new ColorSpaces.HSL(_owner.GetCC_Old(3), _owner.GetCC_Old(4), 0.975 - 0.05 * GridRow);
                return new ColorSpaces.HSL(_owner.GetCC_Old(3), 0.05 + 0.1 * GridRow, _owner.GetCC_Old(5));
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
                tone.Background.Color = tone.GetBackgroundHSL().GetRGB().GetColor();
                tone.Foreground.Color = ColorSpaces.IsDarkColor(tone.Background.Color) ? Colors.White : Colors.Black;
            }

            OnPropertiesChanged(nameof(Tones));
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
    }
}
