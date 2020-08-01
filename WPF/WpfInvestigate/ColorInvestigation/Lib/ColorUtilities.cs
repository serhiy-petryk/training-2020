/* Interface
Color   StringToColor(string hexStringOfColor)
double  ColorToGrayLevel(Color color)
bool    IsDarkColor(Color color)
Color   InvertColor(Color color)

HSL:    Tuple<double, double, double> ColorToHsl(Color color)
        Color HslToColor(double h, double s, double l)
HSV:    Tuple<double, double, double> ColorToHsv(Color color)
        Color HsvToColor(double h, double s, double v)
XYZ:    Tuple<double, double, double> ColorToXyz(Color color)
        Color XyzToColor(double x, double y, double z)
LAB:    Tuple<double, double, double> ColorToLab(Color color)
        Color LabToColor(double l, double a, double b)
YCbCr:  Tuple<double, double, double> ColorToYCbCr(Color color, YCbCrStandard yCbCrStandard)
        Color YCbCrToColor(double y, double cB, double cR, YCbCrStandard yCbCrStandard)
 */

using System;
using System.Windows.Media;

namespace ColorInvestigation.Lib
{
    public class ColorUtilities
    {
        public const double DarkSplit = 0.582 * 255.0; // ~148.4

        private const double DefaultPrecision = 0.0001;
        private const double OneThird = 1.0 / 3.0;
        private const double TwoThirds = 2.0 / 3.0;
        private const double OneSixth = 1.0 / 6.0;
        private const YCbCrStandard DefaultYCbCr = YCbCrStandard.BT709;

        public static Color StringToColor(string hexStringOfColor) => (Color)ColorConverter.ConvertFromString(hexStringOfColor);

        /// <summary>
        /// Get gray level based on the BT.709 standard of YCbCr color space
        /// </summary>
        /// <param name="color"></param>
        /// <returns>Gray level representation: double [0, 255]</returns>
        public static double ColorToGrayLevel(Color color)
        {
            var kB = yCbCrMultipliers[(int)DefaultYCbCr, 0];
            var kR = yCbCrMultipliers[(int)DefaultYCbCr, 1];
            return kR * color.R + (1.0 - kB - kR) * color.G + kB * color.B;
        }
        /// <summary>
        /// Define is color dark based on the BT.709 standard of YCbCr color space
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsDarkColor(Color color) => ColorToGrayLevel(color) < DarkSplit;

        /// <summary>
        /// Get invert color based on hue of HSV color space
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color InvertColor(Color color)
        {
            var hsv = ColorToHsv(color);
            return HsvToColor((hsv.Item1 + 180.0) % 360.0, hsv.Item2, hsv.Item3);
        }

        #region =========  HSL  =========
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
        #endregion

        #region =========  HSV  =========
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
        #endregion

        #region =========  XYZ  =========
        private static Tuple<double, double, double> XyzWhiteReference { get; } = new Tuple<double, double, double>(95.047, 100.000, 108.883);
        private const double Epsilon = 216.0 / 24389.0; // Intent is 0.008856 = 216/24389
        private const double Kappa = 24389.0 / 27.0; // Intent is 903.3 = 24389/27

        /**
        * Converts an RGB color to XYZ.
        * @param   Color
        * @return  Tuple<double, double, double> The XYZ representation: x[0-95.5], y[0-100], and z[0-108.9]
        */
        public static Tuple<double, double, double> ColorToXyz(Color color)
        {
            var r = PivotRgb(color.R / 255.0);
            var g = PivotRgb(color.G / 255.0);
            var b = PivotRgb(color.B / 255.0);

            // Observer. = 2°, Illuminant = D65
            var x = r * 0.4124 + g * 0.3576 + b * 0.1805;
            var y = r * 0.2126 + g * 0.7152 + b * 0.0722;
            var z = r * 0.0193 + g * 0.1192 + b * 0.9505;
            return new Tuple<double, double, double>(x, y, z);
        }

        public static Color XyzToColor(double x, double y, double z)
        {
            // (Observer = 2°, Illuminant = D65)
            x = x / 100.0;
            y = y / 100.0;
            z = z / 100.0;

            var r = x * 3.2406 + y * -1.5372 + z * -0.4986;
            var g = x * -0.9689 + y * 1.8758 + z * 0.0415;
            var b = x * 0.0557 + y * -0.2040 + z * 1.0570;

            r = r > 0.0031308 ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r;
            g = g > 0.0031308 ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g;
            b = b > 0.0031308 ? 1.055 * Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b;

            return Color.FromRgb(ToRgb(r), ToRgb(g), ToRgb(b));
        }

        private static byte ToRgb(double n)
        {
            var result = Convert.ToInt32(255.0 * n);
            if (result < 0) return 0;
            if (result > 255) return 255;
            return (byte)result;
        }
        private static double PivotRgb(double n) => (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100.0;
        #endregion

        #region =========  LAB  =========
        /// <summary>
        /// Convert color to CIELAB color space components
        /// </summary>
        /// <param name="color"></param>
        /// <returns>The CIELAB representation: Item1 = L*: lightness [0-100], Item2 = a*: from green (−) to red (+) component [-127.5, 127.5], Item3 = b*: from blue (−) to yellow (+) component [-127.5, 127.5]</returns>
        public static Tuple<double, double, double> ColorToLab(Color color)
        {
            var xyz = ColorToXyz(color);
            var white = XyzWhiteReference;

            var x = PivotXyz(xyz.Item1 / white.Item1);
            var y = PivotXyz(xyz.Item2 / white.Item2);
            var z = PivotXyz(xyz.Item3 / white.Item3);

            var l = Math.Max(0, 116 * y - 16);
            var a = 500 * (x - y);
            var b = 200 * (y - z);
            return new Tuple<double, double, double>(l, a, b);
        }
        /// <summary>
        /// Get color from CIELAB color space components
        /// </summary>
        /// <param name="l">L* (lightness) component [0-100]</param>
        /// <param name="a">a* (from green (−) to red (+)) component [-127.5, 127.5]</param>
        /// <param name="b">b* (from blue (−) to yellow (+)) component [-127.5, 127.5]</param>
        /// <returns></returns>
        public static Color LabToColor(double l, double a, double b)
        {
            var y = (l + 16.0) / 116.0;
            var x = a / 500.0 + y;
            var z = y - b / 200.0;

            var white = XyzWhiteReference;
            var x3 = x * x * x;
            var z3 = z * z * z;

            var xyzX = white.Item1 * (x3 > Epsilon ? x3 : (x - 16.0 / 116.0) / 7.787);
            var xyzY = white.Item2 * (l > (Kappa * Epsilon)
                           ? Math.Pow(((l + 16.0) / 116.0), 3)
                           : l / Kappa);
            var xyzZ = white.Item3 * (z3 > Epsilon ? z3 : (z - 16.0 / 116.0) / 7.787);
            return XyzToColor(xyzX, xyzY, xyzZ);
        }

        private static double CubicRoot(double n) => Math.Pow(n, OneThird);
        private static double PivotXyz(double n) => n > Epsilon ? CubicRoot(n) : (Kappa * n + 16) / 116;
        #endregion


        #region =========  YCbCr  =========
        public enum YCbCrStandard
        {
            BT601 = 0,
            BT709 = 1,
            BT2020 = 2,
            My = 3
        }

        private static double[,] yCbCrMultipliers = { { 0.114, 0.299 }, { 0.0722, 0.2126 }, { 0.0593, 0.2627 }, { 0.0102, 0.1736 } };

        /// <summary>
        /// Convert color to YCbCr/YPbCb/YPrCr color space components
        /// </summary>
        /// <param name="color"></param>
        /// <param name="yCbCrStandard">BT601, BT709 or BT2020 standard</param>
        /// <returns>The YCbCr/YPbCb/YPrCr representation: Item1 = Y: luma component [0-255], Item2 = Cb: blue-difference chroma component [-127.5, 127.5], Item3 = Cr: red-difference chroma component [-127.5, 127.5]</returns>
        public static Tuple<double, double, double> ColorToYCbCr(Color color, YCbCrStandard yCbCrStandard)
        {
            var kB = yCbCrMultipliers[(int)yCbCrStandard, 0];
            var kR = yCbCrMultipliers[(int)yCbCrStandard, 1];
            var y = kR * color.R + (1 - kR - kB) * color.G + kB * color.B;
            var cB = 0.5 / (1.0 - kB) * (1.0 * color.B - y);
            var cR = 0.5 / (1.0 - kR) * (1.0 * color.R - y);
            return new Tuple<double, double, double>(y, cB, cR);
        }

        /// <summary>
        /// Get color from YCbCr/YPbCb/YPrCr color space components
        /// </summary>
        /// <param name="y">Y: luma component [0-255]</param>
        /// <param name="cB">Cb: blue-difference chroma component [-127.5, 127.5]</param>
        /// <param name="cR">Cr: red-difference chroma component [-127.5, 127.5]</param>
        /// <param name="yCbCrStandard">BT601, BT709 or BT2020 standard</param>
        /// <returns></returns>
        public static Color YCbCrToColor(double y, double cB, double cR, YCbCrStandard yCbCrStandard)
        {
            var kB = yCbCrMultipliers[(int)yCbCrStandard, 0];
            var kR = yCbCrMultipliers[(int)yCbCrStandard, 1];
            var r = Convert.ToByte(Math.Min(255, Math.Max(0.0, y + (1 - kR) / 0.5 * cR)));
            var g = Convert.ToByte(Math.Min(255, Math.Max(0.0, y - 2 * kB * (1 - kB) / (1 - kB - kR) * cB - 2 * kR * (1 - kR) / (1 - kB - kR) * cR)));
            var b = Convert.ToByte(Math.Min(255, Math.Max(0.0, y + (1 - kB) / 0.5 * cB)));
            return Color.FromRgb(r, g, b);
        }
        #endregion
    }
}
