// Some algorithms taken from https://github.com/waesteve/Color-RGB_HSL_HSV_XY/blob/master/RGBConverter.cs (no license)
// and https://github.com/muak/ColorMinePortable (MIT license)

/* Interface
Color   StringToColor(string hexStringOfColor)
double  ColorToGrayLevel(Color color)
bool    IsDarkColor(Color color)
Color   InvertColor(Color color)

Color spaces: 
RGB:    the RGB color representation: r[0-1.0], g[0-1.0], b[0-1.0]
HSL:    h, s, and l in the set [0, 1.0]
HSV:    h, s, and v in the set [0, 1.0]
XYZ:    the XYZ representation: x[0-95.5], y[0-100], and z[0-108.9]
LAB:    the CIELAB representation: L*: lightness [0-100], a*: from green (−) to red (+) component [-127.5, 127.5], b*: from blue (−) to yellow (+) component [-127.5, 127.5]
yCbCR (BT601, BT709, BT2020 standards): the YCbCr/YPbCb/YPrCr representation:
    y: luma component [0-1.0], cB: blue-difference chroma component [-0.5, 0.5], cR: red-difference chroma component [-0.5, 0.5]
============
 */

using System;
using System.Windows.Media;

namespace ColorInvestigation.Common
{
    public class ColorSpaces
    {
        public static Color StringToColor(string hexStringOfColor) => (Color)ColorConverter.ConvertFromString(hexStringOfColor);

        /// <summary>
        /// Get color gray level based on the BT.709 standard of YCbCr color space
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns>Gray level representation: double [0, 1]
        public static double GetGrayLevel(RGB rgb) => YCbCr.GetGrayLevel(rgb);

        /// <summary>
        /// Define is color dark based on the BT.709 standard of YCbCr color space
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsDarkColor(Color color) => GetGrayLevel(new RGB(color)) < DarkSplit;
        public static Color GetForegroundColor(Color backgroundColor) => IsDarkColor(backgroundColor) ? Colors.White : Colors.Black;

        /// <summary>
        /// Get invert color based on hue of HSV color space
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color InvertColor(Color color)
        {
            var hsv = new HSV(new RGB(color));
            hsv.H = hsv.H > 0.5 ? hsv.H - 0.5 : hsv.H + 0.5;
            return hsv.GetRGB().Color;
        }

        private const double DarkSplit = 0.582;
        private const double DefaultPrecision = 0.0001;
        private const double OneThird = 1.0 / 3.0;
        private const double TwoThirds = 2.0 / 3.0;
        private const double OneSixth = 1.0 / 6.0;

        private static double threeway_max(double a, double b, double c) => Math.Max(a, Math.Max(b, c));
        private static double threeway_min(double a, double b, double c) => Math.Min(a, Math.Min(b, c));
        private static bool is_equal(double d1, double d2) => Math.Abs(d1 - d2) < DefaultPrecision;

        private static double check_double(double value, double min = 0.0, double max = 1.0) =>
            Math.Min(max, Math.Max(min, value));

        #region  ===========  RGB  ============
        public class RGB
        {
            public double R, G, B;
            public Color Color => Color.FromRgb(Convert.ToByte(R * 255), Convert.ToByte(G * 255), Convert.ToByte(B * 255));

            public RGB(double r, double g, double b)
            {
                R = r; G = g; B = b;
            }

            public RGB(Color color)
            {
                R = color.R / 255.0; G = color.G / 255.0; B = color.B / 255.0;
            }

            public Color GetColor(double alpha) => Color.FromArgb(Convert.ToByte(alpha * 255),
                Convert.ToByte(R * 255), Convert.ToByte(G * 255), Convert.ToByte(B * 255));

            public override string ToString() => $"R: {R}, G: {G}, B: {B}";
        }
        #endregion

        #region  ===========  HSL  ============

        public class HSL
        {
            public double H, S, L;

            public HSL(double h, double s, double l)
            {
                H = h; S = s; L = l;
            }
            public HSL(RGB rgb)
            {
                var max = threeway_max(rgb.R, rgb.G, rgb.B);
                var min = threeway_min(rgb.R, rgb.G, rgb.B);
                L = (max + min) / 2;

                if (is_equal(max, min))
                    H = S = 0.0; // achromatic
                else
                {
                    var d = max - min;
                    S = L > 0.5 ? d / (2.0 - max - min) : d / (max + min);
                    if (is_equal(max, rgb.R))
                        H = (rgb.G - rgb.B) / d + (rgb.G < rgb.B ? 6.0 : 0.0);
                    else if (is_equal(max, rgb.G))
                        H = (rgb.B - rgb.R) / d + 2.0;
                    else if (is_equal(max, rgb.B))
                        H = (rgb.R - rgb.G) / d + 4.0;
                    H /= 6.0;
                }
            }
            public RGB GetRGB()
            {
                if (is_equal(S, 0.0))
                    return new RGB(L, L, L); // achromatic

                var q = L < 0.5 ? L * (1.0 + S) : L + S - L * S;
                var p = 2.0 * L - q;
                var r = hue2rgb(p, q, H + OneThird);
                var g = hue2rgb(p, q, H);
                var b = hue2rgb(p, q, H - OneThird);
                return new RGB(r, g, b);
            }

            private static double hue2rgb(double p, double q, double t)
            {
                if (t < 0) t += 1.0;
                if (t > 1) t -= 1.0;
                if (t < OneSixth) return p + (q - p) * 6.0 * t;
                if (t < 0.5) return q;
                if (t < TwoThirds) return p + (q - p) * (TwoThirds - t) * 6.0;
                return p;
            }
            public override string ToString() => $"H: {H}, S: {S}, L: {L}";
        }
        #endregion

        #region  ===========  HSV  ============
        public class HSV
        {
            public double H, S, V;

            public HSV(double h, double s, double v)
            {
                H = h; S = s; V = v;
            }
            public HSV(RGB rgb)
            {
                var max = threeway_max(rgb.R, rgb.G, rgb.B);
                var min = threeway_min(rgb.R, rgb.G, rgb.B);
                var d = max - min;
                V = max;
                S = is_equal(max, 0.0) ? 0.0 : d / max;

                if (is_equal(max, min))
                    H = 0.0; // achromatic
                else
                {
                    if (is_equal(max, rgb.R))
                        H = (rgb.G - rgb.B) / d + (rgb.G < rgb.B ? 6.0 : 0.0);
                    else if (is_equal(max, rgb.G))
                        H = (rgb.B - rgb.R) / d + 2.0;
                    else if (is_equal(max, rgb.B))
                        H = (rgb.R - rgb.G) / d + 4.0;
                    H /= 6.0;
                }
            }

            public RGB GetRGB()
            {
                var i = (int) (H * 6.0);
                var f = H * 6.0 - i;
                var p = V * (1.0 - S);
                var q = V * (1.0 - f * S);
                var t = V * (1.0 - (1.0 - f) * S);

                switch (i % 6.0)
                {
                    case 0: return new RGB(V, t, p);
                    case 1: return new RGB(q, V, p);
                    case 2: return new RGB(p, V, t);
                    case 3: return new RGB(p, q, V);
                    case 4: return new RGB(t, p, V);
                    case 5: return new RGB(V, p, q);
                }

                throw new Exception("Unexpected error!!! Check HSV.GetRGB method");
            }
            public override string ToString() => $"H: {H}, S: {S}, V: {V}";
        }

        #endregion

        #region  ===========  XYZ  ============
        public class XYZ
        {
            // The XYZ representation: x[0-95.5], y[0-100], and z[0-108.9]
            public double X, Y, Z;

            public XYZ(double x, double y, double z)
            {
                X = x; Y = y; Z = z;
            }

            public XYZ(RGB rgb)
            {
                var r = PivotRgb(rgb.R);
                var g = PivotRgb(rgb.G);
                var b = PivotRgb(rgb.B);

                // Observer. = 2°, Illuminant = D65
                X = r * 0.4124 + g * 0.3576 + b * 0.1805;
                Y = r * 0.2126 + g * 0.7152 + b * 0.0722;
                Z = r * 0.0193 + g * 0.1192 + b * 0.9505;
            }

            public RGB GetRGB()
            {
                // (Observer = 2°, Illuminant = D65)
                var x = X / 100.0;
                var y = Y / 100.0;
                var z = Z / 100.0;

                var r = x * 3.2406 + y * -1.5372 + z * -0.4986;
                var g = x * -0.9689 + y * 1.8758 + z * 0.0415;
                var b = x * 0.0557 + y * -0.2040 + z * 1.0570;

                r = check_double(r > 0.0031308 ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r);
                g = check_double(g > 0.0031308 ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g);
                b = check_double(b > 0.0031308 ? 1.055 * Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b);

                return new RGB(r, g, b);
            }
            public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}";

            private static double PivotRgb(double n) => (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100.0;
        }
        #endregion

        #region  ===========  LAB  ============
        public class LAB
        {
            // The CIELAB representation:
            // L*: lightness [0-100],
            // a*: from green (−) to red (+) component [-127.5, 127.5],
            // b*: from blue (−) to yellow (+) component [-127.5, 127.5]
            public double L, A, B;

            public LAB(double l, double a, double b)
            {
                L = l; A = a; B = b;
            }
            public LAB(RGB rgb) : this(new XYZ(rgb)) { }

            public LAB(XYZ xyz)
            {
                var white = XyzWhiteReference;

                var x = PivotXyz(xyz.X / white.X);
                var y = PivotXyz(xyz.Y / white.Y);
                var z = PivotXyz(xyz.Z / white.Z);

                L = Math.Max(0, 116 * y - 16);
                A = 500 * (x - y);
                B = 200 * (y - z);
            }

            public RGB GetRGB() => GetXYZ().GetRGB();

            public XYZ GetXYZ()
            {
                var y = (L + 16.0) / 116.0;
                var x = A / 500.0 + y;
                var z = y - B / 200.0;

                var white = XyzWhiteReference;
                var x3 = x * x * x;
                var z3 = z * z * z;

                var xyzX = white.X * (x3 > Epsilon ? x3 : (x - 16.0 / 116.0) / 7.787);
                var xyzY = white.Y * (L > (Kappa * Epsilon)
                               ? Math.Pow(((L + 16.0) / 116.0), 3)
                               : L / Kappa);
                var xyzZ = white.Z * (z3 > Epsilon ? z3 : (z - 16.0 / 116.0) / 7.787);
                return new XYZ(xyzX, xyzY, xyzZ);
            }

            public override string ToString() => $"L: {L}, A: {A}, B: {B}";

            private static XYZ XyzWhiteReference => new XYZ(95.047, 100.000, 108.883);
            private const double Epsilon = 216.0 / 24389.0; // Intent is 0.008856 = 216/24389
            private const double Kappa = 24389.0 / 27.0; // Intent is 903.3 = 24389/27
            private static double CubicRoot(double n) => Math.Pow(n, OneThird);
            private static double PivotXyz(double n) => n > Epsilon ? CubicRoot(n) : (Kappa * n + 16) / 116;
        }
        #endregion

        #region  ===========  YCbCr  ============
        public enum YCbCrStandard
        {
            BT601 = 0,
            BT709 = 1,
            BT2020 = 2,
            My = 3
        }
        public class YCbCr
        {
            // The YCbCr/YPbCb/YPrCr representation:
            // Y: luma component [0-1],
            // Cb: blue-difference chroma component [-0.5, 0.5],
            // Cr: red-difference chroma component [-0.5, 0.5]

            private const YCbCrStandard DefaultYCbCrStandard = YCbCrStandard.BT709;
            private static double[,] yCbCrMultipliers = { { 0.114, 0.299 }, { 0.0722, 0.2126 }, { 0.0593, 0.2627 }, { 0.0102, 0.1736 } };

            public double Y, Cb, Cr;
            public YCbCrStandard Standard = DefaultYCbCrStandard;
            public YCbCr(double y, double cB, double cR)
            {
                Y = y; Cb = cB; Cr = cR;
            }
            public YCbCr(RGB rgb, YCbCrStandard standard = DefaultYCbCrStandard)
            {
                var kB = yCbCrMultipliers[(int)standard, 0];
                var kR = yCbCrMultipliers[(int)standard, 1];
                Y = kR * rgb.R + (1 - kR - kB) * rgb.G + kB * rgb.B;
                Cb = 0.5 / (1.0 - kB) * (rgb.B - Y);
                Cr = 0.5 / (1.0 - kR) * (rgb.R - Y);
                Standard = standard;
            }

            public RGB GetRGB()
            {
                var kB = yCbCrMultipliers[(int)Standard, 0];
                var kR = yCbCrMultipliers[(int)Standard, 1];
                var r = Math.Min(1.0, Math.Max(0.0, Y + (1 - kR) / 0.5 * Cr));
                var g = Math.Min(1.0, Math.Max(0.0, Y - 2 * kB * (1 - kB) / (1 - kB - kR) * Cb - 2 * kR * (1 - kR) / (1 - kB - kR) * Cr));
                var b = Math.Min(1.0, Math.Max(0.0, Y + (1 - kB) / 0.5 * Cb));
                return new RGB(r, g, b);
            }

            /// <summary>
            /// Get color gray level based on the BT.709 standard of YCbCr color space
            /// </summary>
            /// <param name="rgb"></param>
            /// <returns>Gray level representation: double [0, 1]</returns>
            public static double GetGrayLevel(RGB rgb)
            {
                var kB = yCbCrMultipliers[(int)DefaultYCbCrStandard, 0];
                var kR = yCbCrMultipliers[(int)DefaultYCbCrStandard, 1];
                return kR * rgb.R + (1.0 - kB - kR) * rgb.G + kB * rgb.B;
            }

            public override string ToString() => $"Y: {Y}, Cb: {Cb}, Cr: {Cr}";
        }
        #endregion
    }
}
