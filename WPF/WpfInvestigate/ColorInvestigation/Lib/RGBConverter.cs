// Taken from https://github.com/waesteve/Color-RGB_HSL_HSV_XY/blob/master/RGBConverter.cs

using System;
using System.Windows.Media;

namespace ColorInvestigation.Lib
{
    public class RGBConverter
    {
        private const double DefaultPrecision = 0.0001;
        private const double OneThird = 1.0 / 3.0;
        private const double TwoThirds = 2.0 / 3.0;
        private const double OneSixth = 1.0 / 6.0;

        public static Color StringToColor(string hexStringOfColor) => (Color)ColorConverter.ConvertFromString(hexStringOfColor);

        /*public static Color InvertColor(Color color)
        {
            var hsv = ColorToHsv(color);
            double newHue = (hsv.Item1 + 180.0) % 360.0;
            var newColor = HsvToColor(newHue, hsv.Item2, hsv.Item3);
            return newColor;
        }*/

        public static void rgbToHsl(byte r, byte g, byte b, double[] hsl)
        {
            var rd = r / 255.0;
            var gd = g / 255.0;
            var bd = b / 255.0;
            var max = threeway_max(rd, gd, bd);
            var min = threeway_min(rd, gd, bd);
            double h = 0.0, s = 0.0, l = (max + min) / 2;

            if (IsEqual(max, min))
                h = s = 0.0; // achromatic
            else
            {
                var d = max - min;
                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
                if (IsEqual(max, rd))
                    h = (gd - bd) / d + (gd < bd ? 6.0 : 0.0);
                else if (IsEqual(max, gd))
                    h = (bd - rd) / d + 2.0;
                else if (IsEqual(max, bd) )
                    h = (rd - gd) / d + 4.0;
                h /= 6.0;
            }
            hsl[0] = h;
            hsl[1] = s;
            hsl[2] = l;
        }

        /**
         * Converts an HSL color value to RGB. Conversion formula
         * adapted from http://en.wikipedia.org/wiki/HSL_color_space.
         * Assumes h, s, and l are contained in the set [0, 1] and
         * returns r, g, and b in the set [0, 255].
         *
         * @param   Number  h       The hue
         * @param   Number  s       The saturation
         * @param   Number  l       The lightness
         * @return  Array           The RGB representation
         */
        public static void hslToRgb(double h, double s, double l, byte[] rgb)
        {
            double r, g, b;

            if (IsEqual(s, 0.0))
                r = g = b = l; // achromatic
            else
            {
                var q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
                var p = 2.0 * l - q;
                r = hue2rgb(p, q, h + OneThird);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - OneThird);
            }

            rgb[0] = Convert.ToByte(r * 255.0);
            rgb[1] = Convert.ToByte(g * 255.0);
            rgb[2] = Convert.ToByte(b * 255.0);
        }

        /**
         * Converts an RGB color value to HSV. Conversion formula
         * adapted from http://en.wikipedia.org/wiki/HSV_color_space.
         * Assumes r, g, and b are contained in the set [0, 255] and
         * returns h, s, and v in the set [0, 1].
         *
         * @param   Number  r       The red color value
         * @param   Number  g       The green color value
         * @param   Number  b       The blue color value
         * @return  Array           The HSV representation
         */
        public static void rgbToHsv(byte r, byte g, byte b, double[] hsv)
        {
            double rd = (double)r / 255.0;
            double gd = (double)g / 255.0;
            double bd = (double)b / 255.0;
            double max = threeway_max(rd, gd, bd), min = threeway_min(rd, gd, bd);
            double h = 0.0, s = 0, v = max;

            double d = max - min;
            s = IsEqual(max, 0.0) ? 0.0 : d / max;

            if (IsEqual(max, min))
                h = 0.0; // achromatic
            else
            {
                if (IsEqual(max, rd))
                    h = (gd - bd) / d + (gd < bd ? 6.0 : 0.0);
                else if (IsEqual(max, gd))
                    h = (bd - rd) / d + 2.0;
                else if (IsEqual(max, bd)) 
                    h = (rd - gd) / d + 4.0;
                h /= 6.0;
            }

            hsv[0] = h;
            hsv[1] = s;
            hsv[2] = v;
        }

        /**
         * Converts an HSV color value to RGB. Conversion formula
         * adapted from http://en.wikipedia.org/wiki/HSV_color_space.
         * Assumes h, s, and v are contained in the set [0, 1] and
         * returns r, g, and b in the set [0, 255].
         *
         * @param   Number  h       The hue
         * @param   Number  s       The saturation
         * @param   Number  v       The value
         * @return  Array           The RGB representation
         */
        public static void hsvToRgb(double h, double s, double v, byte[] rgb)
        {
            double r = 0.0, g = 0.0, b = 0.0;

            var i = (int)(h * 6.0);
            var f = h * 6.0 - i;
            var p = v * (1.0 - s);
            var q = v * (1.0 - f * s);
            var t = v * (1.0 - (1.0 - f) * s);

            switch (i % 6.0)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }

            rgb[0] = Convert.ToByte(r * 255.0);
            rgb[1] = Convert.ToByte(g * 255.0);
            rgb[2] = Convert.ToByte(b * 255.0);
        }

        private static double threeway_max(double a, double b, double c) => Math.Max(a, Math.Max(b, c));
        public static double threeway_min(double a, double b, double c) => Math.Min(a, Math.Min(b, c));
        private static bool IsEqual(double d1, double d2) => Math.Abs(d1 - d2) < DefaultPrecision;

        private static double hue2rgb(double p, double q, double t)
        {
            if (t < 0) t += 1.0;
            if (t > 1) t -= 1.0;
            if (t < OneSixth) return p + (q - p) * 6.0 * t;
            if (t < 0.5) return q;
            if (t < TwoThirds) return p + (q - p) * (TwoThirds - t) * 6.0;
            return p;
        }

    }
}
