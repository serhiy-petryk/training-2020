// based on https://github.com/muak/ColorMinePortable

using System;
using System.Globalization;
using System.Windows.Media;
using ColorInvestigation.Common;

namespace ColorInvestigation.Lib
{
    public class ColorHsl
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
        /// Lightness (0-1)
        /// </summary>
        public double L { get; }
        public Color RgbColor { get; }

        public ColorHsl(Color color)
        {
            RgbColor = color;

            double max = Math.Max(color.R, Math.Max(color.G, color.B));
            double min = Math.Min(color.R, Math.Min(color.G, color.B));

            //saturation
            var cnt = (max + min) / 2d;
            if (cnt <= 127d)
                S = ((max - min) / (max + min));
            else
                S = ((max - min) / (510d - max - min));

            //lightness
            L = ((max + min) / 2d) / 255d;

            //hue
            if (Math.Abs(max - min) <= float.Epsilon)
            {
                H = 0d;
                S = 0d;
            }
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
        }
        public ColorHsl(string hexColor): this((Color)ColorConverter.ConvertFromString(hexColor))
        {}

        public ColorHsl(double hue, double saturation, double lightness)
        {
            H = hue;
            S = saturation;
            L = lightness;

            var rangedH = H / 360.0;
            var r = 0.0;
            var g = 0.0;
            var b = 0.0;
            var s = S;
            var l = L;

            if (!Tips.AreEqual(l, 0))
            {
                if (Tips.AreEqual(s, 0))
                    r = g = b = l;
                else
                {
                    var temp2 = (l < 0.5) ? l * (1.0 + s) : l + s - (l * s);
                    var temp1 = 2.0 * l - temp2;

                    r = Tips.GetColorComponent(temp1, temp2, rangedH + 1.0 / 3.0);
                    g = Tips.GetColorComponent(temp1, temp2, rangedH);
                    b = Tips.GetColorComponent(temp1, temp2, rangedH - 1.0 / 3.0);
                }
            }
            RgbColor = Color.FromRgb(Convert.ToByte(255.0 * r), Convert.ToByte(255.0 * g), Convert.ToByte(255.0 * b));
        }

        private static CultureInfo ci = CultureInfo.InvariantCulture;
        public override string ToString()
        {
            return
                $"H: {Math.Round(H, 3).ToString(ci)}, S: {Math.Round(S, 3).ToString(ci)}, L: {Math.Round(L, 3).ToString(ci)}; #{RgbColor.R:X2}{RgbColor.G:X2}{RgbColor.B:X2}";
        }
    }
}
