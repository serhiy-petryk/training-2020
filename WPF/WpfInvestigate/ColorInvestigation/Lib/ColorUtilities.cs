// Based on from https://github.com/waesteve/Color-RGB_HSL_HSV_XY/blob/master/RGBConverter.cs
using System;
using System.Windows.Media;

namespace ColorInvestigation.Lib
{
    public class ColorUtilities
    {
        private const double DefaultPrecision = 0.0001;
        private const double OneThird = 1.0 / 3.0;
        private const double TwoThirds = 2.0 / 3.0;
        private const double OneSixth = 1.0 / 6.0;

        public static Color StringToColor(string hexStringOfColor) => (Color)ColorConverter.ConvertFromString(hexStringOfColor);

        public static double ColorToGrayScale(Color color) => (0.17 * color.R + 0.78 * color.G + 0.01 * color.B) / 256.0;
        public static Color GetForegroundColor(Color color) => 0.17 * color.R + 0.78 * color.G + 0.01 * color.B < 155 ? Colors.White : Colors.Black;

        public static Color InvertColor(Color color)
        {
            var hsv = ColorToHsv(color);
            return HsvToColor((hsv.Item1 + 180.0) % 360.0, hsv.Item2, hsv.Item3);
        }

        /**
         * Converts an RGB color to HSL.
         * Conversion formula adapted from http://en.wikipedia.org/wiki/HSL_color_space.
         * Returns h, s, and l in the set [0, 1].
         *
         * @param   Color
         * @return  Tuple<double, double, double> The HSL representation
         */
        public static Tuple<double, double, double> ColorToHsl(Color color)
        {
            var r = color.R / 255.0;
            var g = color.G / 255.0;
            var b = color.B / 255.0;
            var max = threeway_max(r, g, b);
            var min = threeway_min(r, g, b);
            double h = 0.0, s = 0.0, l = (max + min) / 2;

            if (is_equal(max, min))
                h = s = 0.0; // achromatic
            else
            {
                var d = max - min;
                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
                if (is_equal(max, r))
                    h = (g - b) / d + (g < b ? 6.0 : 0.0);
                else if (is_equal(max, g))
                    h = (b - r) / d + 2.0;
                else if (is_equal(max, b))
                    h = (r - g) / d + 4.0;
                h /= 6.0;
            }

            return new Tuple<double, double, double>(h, s, l);
        }

        /**
         * Converts an HSL color value to Color.
         * Conversion formula adapted from http://en.wikipedia.org/wiki/HSL_color_space.
         * Assumes h, s, and l are contained in the set [0, 1].
         *
         * @param   Number  h       The hue
         * @param   Number  s       The saturation
         * @param   Number  l       The lightness
         * @return  Color
         */
        public static Color HslToColor(double h, double s, double l)
        {
            double r, g, b;

            if (is_equal(s, 0.0))
                r = g = b = l; // achromatic
            else
            {
                var q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
                var p = 2.0 * l - q;
                r = hue2rgb(p, q, h + OneThird);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - OneThird);
            }

            return Color.FromRgb(Convert.ToByte(r * 255.0), Convert.ToByte(g * 255.0), Convert.ToByte(b * 255.0));
        }

        /**
         * Converts an RGB color to HSV.
         * Conversion formula adapted from http://en.wikipedia.org/wiki/HSV_color_space.
         * Returns h, s, and v in the set [0, 1].
         *
         * @param   Color
         * @return  Tuple<double, double, double> The HSV representation
         */
        public static Tuple<double, double, double> ColorToHsv(Color color)
        {
            var r = color.R / 255.0;
            var g = color.G / 255.0;
            var b = color.B / 255.0;
            double max = threeway_max(r, g, b), min = threeway_min(r, g, b);
            double h = 0.0, s = 0.0, v = max;

            var d = max - min;
            s = is_equal(max, 0.0) ? 0.0 : d / max;

            if (is_equal(max, min))
                h = 0.0; // achromatic
            else
            {
                if (is_equal(max, r))
                    h = (g - b) / d + (g < b ? 6.0 : 0.0);
                else if (is_equal(max, g))
                    h = (b - r) / d + 2.0;
                else if (is_equal(max, b))
                    h = (r - g) / d + 4.0;
                h /= 6.0;
            }

            return new Tuple<double, double, double>(h, s, v);
        }

        /**
         * Converts an HSV color value to Color.
         * Conversion formula adapted from http://en.wikipedia.org/wiki/HSV_color_space.
         * Assumes h, s, and v are contained in the set [0, 1].
         *
         * @param   Number  h       The hue
         * @param   Number  s       The saturation
         * @param   Number  v       The value
         * @return  Color
         */
        public static Color HsvToColor(double h, double s, double v)
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

            return Color.FromRgb(Convert.ToByte(r * 255.0), Convert.ToByte(g * 255.0), Convert.ToByte(b * 255.0));
        }

        private static double threeway_max(double a, double b, double c) => Math.Max(a, Math.Max(b, c));
        private static double threeway_min(double a, double b, double c) => Math.Min(a, Math.Min(b, c));
        private static bool is_equal(double d1, double d2) => Math.Abs(d1 - d2) < DefaultPrecision;

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
