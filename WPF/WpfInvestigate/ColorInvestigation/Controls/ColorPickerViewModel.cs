using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ColorInvestigation.Controls
{
    public class ColorPickerViewModel : INotifyPropertyChanged
    {
        public enum ColorSpace { RGB, HSL, HSV, XYZ, LAB, YCbCr };

        private double[] _values = new double[15];

        #region  ==============  Public Properties  ================
        public double RGB_R
        {
            get => _values[0];
            set => SetProperty(value);
        }
        public double RGB_G
        {
            get => _values[1];
            set => SetProperty(value);
        }
        public double RGB_B
        {
            get => _values[2];
            set => SetProperty(value);
        }
        public double HSL_H
        {
            get => _values[3];
            set => SetProperty(value);
        }
        public double HSL_S
        {
            get => _values[4];
            set => SetProperty(value);
        }
        public double HSL_L
        {
            get => _values[5];
            set => SetProperty(value);
        }
        public double HSV_H
        {
            get => _values[6];
            set => SetProperty(value);
        }
        public double HSV_S
        {
            get => _values[7];
            set => SetProperty(value);
        }
        public double HSV_V
        {
            get => _values[8];
            set => SetProperty(value);
        }
        public double LAB_L
        {
            get => _values[9];
            set => SetProperty(value);
        }
        public double LAB_A
        {
            get => _values[10];
            set => SetProperty(value);
        }
        public double LAB_B
        {
            get => _values[11];
            set => SetProperty(value);
        }
        public double YCbCr_Y
        {
            get => _values[12];
            set => SetProperty(value);
        }
        public double YCbCr_Cb
        {
            get => _values[13];
            set => SetProperty(value);
        }
        public double YCbCr_Cr
        {
            get => _values[14];
            set => SetProperty(value);
        }
        private void SetProperty(double value, [CallerMemberName]string propertyName = null)
        {
            var meta = Metadata[propertyName];
            value = Math.Max(meta.Min, Math.Min(meta.Max, value));
            _values[meta.SeqNo] = value;
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
            public string Id;
            public int SeqNo { get; set; }
            public double Min;
            public double Max;
            public ColorSpace ColorSpace;
            // public Func<ColorPickerAsync, double> SliderValue;
            // public Action<ColorPickerAsync, double> MouseMoveAction;

            public MetaItem(string id, double min, double max)
            {
                Id = id; Min = min; Max = max;
                //ColorSpace = colorSpace;
                // SliderValue = sliderValue; MouseMoveAction = mouseMoveAction;
            }
        }
        #endregion

    }
}
