// Some algorithms taken from https://github.com/waesteve/Color-RGB_HSL_HSV_XY/blob/master/RGBConverter.cs (no license)
// and https://github.com/muak/ColorMinePortable (MIT license)

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

namespace ColorInvestigation.Common
{
    public class ColorSpaces
    {
        public enum YCbCrStandard
        {
            BT601 = 0,
            BT709 = 1,
            BT2020 = 2,
            My = 3
        }

        private const double DarkSplit = 0.582 * 255.0; // ~148.4
        private const double DefaultPrecision = 0.0001;
        private const double OneThird = 1.0 / 3.0;
        private const double TwoThirds = 2.0 / 3.0;
        private const double OneSixth = 1.0 / 6.0;
        private const YCbCrStandard DefaultYCbCr = YCbCrStandard.BT709;

        private static double threeway_max(double a, double b, double c) => Math.Max(a, Math.Max(b, c));
        private static double threeway_min(double a, double b, double c) => Math.Min(a, Math.Min(b, c));
        private static bool is_equal(double d1, double d2) => Math.Abs(d1 - d2) < DefaultPrecision;

        private static double check_double(double value, double min = 0.0, double max = 1.0) =>
            Math.Min(max, Math.Max(min, value));

        #region  ===========  RGB  ============

        public class RGB
        {
            public double R, G, B;
            public Color Color => Color.FromRgb(Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));

            public RGB(double r, double g, double b)
            {
                R = r;
                G = g;
                B = b;
            }

            public RGB(Color color)
            {
                R = color.R / 255.0;
                G = color.G / 255.0;
                B = color.B / 255.0;
            }

            public Color GetColor(double alpha = 1.0) =>
                Color.FromArgb(Convert.ToByte(alpha * 255), Convert.ToByte(R * 255), Convert.ToByte(G * 255),
                    Convert.ToByte(B * 255));

            /*public RGB(LAB lab)
            {
                var y = (lab.L + 16.0) / 116.0;
                var x = lab.A / 500.0 + y;
                var z = y - lab.B / 200.0;
    
                var white = XYZ.XyzWhiteReference;
                var x3 = x * x * x;
                var z3 = z * z * z;
    
                var xyzX = white.X * (x3 > LAB.Epsilon ? x3 : (x - 16.0 / 116.0) / 7.787);
                var xyzY = white.Y * (l > (LAB.Kappa * LAB.Epsilon)
                               ? Math.Pow(((l + 16.0) / 116.0), 3)
                               : l / LAB.Kappa);
                var xyzZ = white.Z * (z3 > LAB.Epsilon ? z3 : (z - 16.0 / 116.0) / 7.787);
            }*/
            public override string ToString() => $"R: {R}, G: {G}, B: {B}";
        }

        #endregion

        #region  ===========  HSL  ============

        public class HSL
        {
            public double H, S, L;
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
            public static XYZ XyzWhiteReference => new XYZ(95.047, 100.000, 108.883);

            // The XYZ representation: x[0-95.5], y[0-100], and z[0-108.9]
            public double X, Y, Z;

            public XYZ(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
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

            private static double PivotRgb(double n) =>
                (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100.0;
        }

        #endregion

        #region  ===========  LAB  ============

        public class LAB
        {
            // The CIELAB representation: L*: lightness [0-100], a*: from green (−) to red (+) component [-127.5, 127.5], b*: from blue (−) to yellow (+) component [-127.5, 127.5]
            public double L, A, B;

            public LAB(RGB rgb)
            {
                var xyz = new XYZ(rgb);
                var white = XYZ.XyzWhiteReference;

                var x = PivotXyz(xyz.X / white.Z);
                var y = PivotXyz(xyz.Y / white.Z);
                var z = PivotXyz(xyz.Z / white.Z);

                L = Math.Max(0, 116 * y - 16);
                A = 500 * (x - y);
                B = 200 * (y - z);
            }

            private const double Epsilon = 216.0 / 24389.0; // Intent is 0.008856 = 216/24389
            private const double Kappa = 24389.0 / 27.0; // Intent is 903.3 = 24389/27
            private static double CubicRoot(double n) => Math.Pow(n, OneThird);
            private static double PivotXyz(double n) => n > Epsilon ? CubicRoot(n) : (Kappa * n + 16) / 116;
        }

        #endregion
    }
}
