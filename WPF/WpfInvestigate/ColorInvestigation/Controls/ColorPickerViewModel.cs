using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ColorInvestigation.Common;

namespace ColorInvestigation.Controls
{
    public class ColorPickerViewModel : INotifyPropertyChanged
    {
        public event EventHandler PropertiesUpdated;
        public enum ColorSpace { RGB, HSL, HSV, XYZ, LAB, YCbCr };

        private double _alpha;
        private double[] _values = new double[15];

        #region  ==============  Public Properties  ================
        public double Alpha // in range [0, 1]
        {
            get => _alpha;
            set
            {
                _alpha = value;
                UpdateUI();
            }
        }
        public double RGB_R // in range [0, 255]
        {
            get => _values[0];
            set => SetProperty(value);
        }
        public double RGB_G // in range [0, 255]
        {
            get => _values[1];
            set => SetProperty(value);
        }
        public double RGB_B // in range [0, 255]
        {
            get => _values[2];
            set => SetProperty(value);
        }
        public double HSL_H // in range [0, 360]
        {
            get => _values[3];
            set => SetProperty(value);
        }
        public double HSL_S // in range [0, 100]
        {
            get => _values[4];
            set => SetProperty(value);
        }
        public double HSL_L // in range [0, 100]
        {
            get => _values[5];
            set => SetProperty(value);
        }
        public double HSV_H // in range [0, 360]
        {
            get => _values[6];
            set => SetProperty(value);
        }
        public double HSV_S // in range [0, 100]
        {
            get => _values[7];
            set => SetProperty(value);
        }
        public double HSV_V // in range [0, 100]
        {
            get => _values[8];
            set => SetProperty(value);
        }
        public double LAB_L // in range [0, 100]
        {
            get => _values[9];
            set => SetProperty(value);
        }
        public double LAB_A // in range [-127.5, 127.5]
        {
            get => _values[10];
            set => SetProperty(value);
        }
        public double LAB_B // in range [-127.5, 127.5]
        {
            get => _values[11];
            set => SetProperty(value);
        }
        public double YCbCr_Y // in range [0, 100]
        {
            get => _values[12];
            set => SetProperty(value);
        }
        public double YCbCr_Cb // in range [-127.5, 127.5]
        {
            get => _values[13];
            set => SetProperty(value);
        }
        public double YCbCr_Cr // in range [-127.5, 127.5]
        {
            get => _values[14];
            set => SetProperty(value);
        }
        public Tuple<double, double> HSV_S_And_V // set hsv.S & HSV.V simultaneously (for HueAndSaturation slider)
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
                UpdateValues(meta);
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

        static ColorPickerViewModel()
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

        public class MetaItem
        {
            public readonly string Id;
            public int SeqNo;
            public readonly double Min;
            public readonly double Max;
            public readonly double SpaceMultiplier;
            public ColorSpace ColorSpace;
            public double GetValue(ColorPickerViewModel VM) => VM._values[SeqNo];
            // public Func<ColorPickerAsync, double> SliderValue;
            // public Action<ColorPickerAsync, double> MouseMoveAction;

            public MetaItem(string id, double min, double max)
            {
                Id = id; Min = min; Max = max;
                SpaceMultiplier = Id.StartsWith("LAB") ? 1 : Max - Min;
                // SliderValue = sliderValue; MouseMoveAction = mouseMoveAction;
            }
        }
        #endregion

        #region ==============  Update Value  ===============

        private double GetSpaceValue(int index) => _values[index] / Metalist[index].SpaceMultiplier;
        private void SetValues(int startIndex, params double[] newValues)
        {
            for (var k = 0; k < newValues.Length; k++)
                _values[k + startIndex] = newValues[k] * Metalist[k + startIndex].SpaceMultiplier;
        }

        private void UpdateValues(MetaItem meta)
        {
            // Get rgb object
            var rgb = new ColorSpaces.RGB(0, 0, 0);
            if (meta.ColorSpace == ColorSpace.RGB)
                rgb = new ColorSpaces.RGB(GetSpaceValue(0), GetSpaceValue(1), GetSpaceValue(2));
            else if (meta.ColorSpace == ColorSpace.HSL)
            {
                rgb = new ColorSpaces.HSL(GetSpaceValue(3), GetSpaceValue(4), GetSpaceValue(5)).GetRGB();
                // Update HSV
                _values[6] = _values[3]; // _hsv.H = _hsl.H;
                var hsv = new ColorSpaces.HSV(rgb); // _hsv = new ColorSpaces.HSV(_rgb);
                SetValues(7, hsv.S, hsv.V);
            }
            else if (meta.ColorSpace == ColorSpace.HSV)
            {
                rgb = new ColorSpaces.HSV(GetSpaceValue(6), GetSpaceValue(7), GetSpaceValue(8)).GetRGB();
                // Update HSL
                _values[3] = _values[6]; // _hsl.H = _hsv.H;
                var hsl = new ColorSpaces.HSL(rgb); // _hsl = new ColorSpaces.HSL(_rgb);
                SetValues(4, hsl.S, hsl.L);
            }
            else if (meta.ColorSpace == ColorSpace.LAB)
                rgb = new ColorSpaces.LAB(GetSpaceValue(9), GetSpaceValue(10), GetSpaceValue(11)).GetRGB();
            else if (meta.ColorSpace == ColorSpace.YCbCr)
                rgb = new ColorSpaces.YCbCr(GetSpaceValue(12), GetSpaceValue(13), GetSpaceValue(14)).GetRGB();

            // Update other objects
            if (meta.ColorSpace != ColorSpace.RGB)
                SetValues(0, rgb.R, rgb.G, rgb.B);
            if (meta.ColorSpace != ColorSpace.HSL && meta.ColorSpace != ColorSpace.HSV)
            {
                var hsl = new ColorSpaces.HSL(rgb);
                SetValues(3, hsl.H, hsl.S, hsl.L);
                var hsv = new ColorSpaces.HSV(rgb);
                SetValues(6, hsv.H, hsv.S, hsv.V);
            }
            if (meta.ColorSpace != ColorSpace.LAB)
            {
                var lab = new ColorSpaces.LAB(rgb);
                SetValues(9, lab.L, lab.A, lab.B);
            }
            if (meta.ColorSpace != ColorSpace.YCbCr)
            {
                var yCbCr = new ColorSpaces.YCbCr(rgb);
                SetValues(12, yCbCr.Y, yCbCr.Cb, yCbCr.Cr);
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            OnPropertiesChanged(Metadata.Keys.ToArray());
            PropertiesUpdated?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
