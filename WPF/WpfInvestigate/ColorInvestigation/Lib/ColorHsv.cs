// based on https://github.com/muak/ColorMinePortable

using System;
using System.Globalization;
using System.Windows.Media;

namespace ColorInvestigation.Lib
{
    public class ColorHsv
    {
        /// <summary>
        /// Hue (0-360)
        /// </summary>
        public double H { get; }
        /// <summary>
        /// Saturation (0-1)
        /// </summary>
        public double S { get; }
        /// <summary>
        /// Value (0-1)
        /// </summary>
        public double V { get; }
        public Color RgbColor { get; }

        public ColorHsv(string hexColor) : this((Color)ColorConverter.ConvertFromString(hexColor))
        { }
        public ColorHsv(Color color)
        {
            RgbColor = color;

            double max = Math.Max(color.R, Math.Max(color.G, color.B));
            double min = Math.Min(color.R, Math.Min(color.G, color.B));

            if (Math.Abs(max - min) <= float.Epsilon)
                H = 0d;
            else
            {
                var diff = max - min;

                if (Math.Abs(max - color.R) <= float.Epsilon)
                    H = 60d * (color.G - color.B) / diff;
                else if (Math.Abs(max - color.G) <= float.Epsilon)
                    H = 60d * (color.B - color.R) / diff + 120d;
                else
                    H = 60d * (color.R - color.G) / diff + 240d;

                if (H < 0d) H += 360d;
            }

            S = (max <= 0) ? 0 : 1d - (1d * min / max);
            V = max / 255d;
        }

        public ColorHsv(double hue, double saturation, double value)
        {
            H = hue;
            S = saturation;
            V = value;

            var range = Convert.ToInt32(Math.Floor(H / 60.0)) % 6;
            var f = H / 60.0 - Math.Floor(H / 60.0);

            var _v = V * 255.0;
            var v = Convert.ToByte(_v);
            var p = Convert.ToByte(_v * (1 - S));
            var q = Convert.ToByte(_v * (1 - f * S));
            var t = Convert.ToByte(_v * (1 - (1 - f) * S));

            switch (range)
            {
                case 0:
                    RgbColor = Color.FromRgb(v, t, p);
                    break;
                case 1:
                    RgbColor = Color.FromRgb(q, v, p);
                    break;
                case 2:
                    RgbColor = Color.FromRgb(p, v, t);
                    break;
                case 3:
                    RgbColor = Color.FromRgb(p, q, v);
                    break;
                case 4:
                    RgbColor = Color.FromRgb(t, p, v);
                    break;
                default:
                    RgbColor = Color.FromRgb(v, p, q);
                    break;
            }
        }



        private static CultureInfo ci = CultureInfo.InvariantCulture;
        public override string ToString()
        {
            return
                $"H: {Math.Round(H, 3).ToString(ci)}, S: {Math.Round(S, 3).ToString(ci)}, V: {Math.Round(V, 3).ToString(ci)}; #{RgbColor.R:X2}{RgbColor.G:X2}{RgbColor.B:X2}";
        }

    }
}
