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
    public enum YCbCrStandard
    {
        BT601 = 0,
        BT709 = 1,
        BT2020 = 2,
        My = 3
    }

    internal static class ColorCommon
    {
        internal const double DarkSplit = 0.582 * 255.0; // ~148.4

        internal const double DefaultPrecision = 0.0001;
        internal const double OneThird = 1.0 / 3.0;
        internal const double TwoThirds = 2.0 / 3.0;
        internal const double OneSixth = 1.0 / 6.0;
        internal const YCbCrStandard DefaultYCbCr = YCbCrStandard.BT709;

        internal static double threeway_max(double a, double b, double c) => Math.Max(a, Math.Max(b, c));
        internal static double threeway_min(double a, double b, double c) => Math.Min(a, Math.Min(b, c));
        internal static bool is_equal(double d1, double d2) => Math.Abs(d1 - d2) < DefaultPrecision;

        internal static double check_double(double value, double min = 0.0, double max = 1.0) => Math.Min(max, Math.Max(min, value));
    }

    #region  ===========  RGB  ============
    public class RGB
    {
        public double R; public double G; public double B;
        public Color Color => Color.FromRgb(Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));

        public RGB(double r, double g, double b )
        {
            R = r; G = g; B = b;
        }
        public RGB(Color color)
        {
            R = color.R / 255.0;
            G = color.G / 255.0;
            B = color.B / 255.0;
        }

        public Color GetColor(double alpha = 1.0) =>
            Color.FromArgb(Convert.ToByte(alpha * 255), Convert.ToByte(R * 255), Convert.ToByte(G * 255), Convert.ToByte(B * 255));

        public RGB(HSL hsl)
        {
            if (ColorCommon.is_equal(hsl.S, 0.0))
                R = G = B = hsl.L; // achromatic
            else
            {
                var q = hsl.L < 0.5 ? hsl.L * (1.0 + hsl.S) : hsl.L + hsl.S - hsl.L * hsl.S;
                var p = 2.0 * hsl.L - q;
                R = HSL.hue2rgb(p, q, hsl.H + ColorCommon.OneThird);
                G = HSL.hue2rgb(p, q, hsl.H);
                B = HSL.hue2rgb(p, q, hsl.H - ColorCommon.OneThird);
            }
        }

        public RGB(HSV hsv)
        {
            var i = (int)(hsv.H * 6.0);
            var f = hsv.H * 6.0 - i;
            var p = hsv.V * (1.0 - hsv.S);
            var q = hsv.V * (1.0 - f * hsv.S);
            var t = hsv.V * (1.0 - (1.0 - f) * hsv.S);

            switch (i % 6.0)
            {
                case 0: R = hsv.V; G = t; B = p; break;
                case 1: R = q; G = hsv.V; B = p; break;
                case 2: R = p; G = hsv.V; B = t; break;
                case 3: R = p; G = q; B = hsv.V; break;
                case 4: R = t; G = p; B = hsv.V; break;
                case 5: R = hsv.V; G = p; B = q; break;
            }
        }

        public RGB(XYZ xyz)
        {
            // (Observer = 2°, Illuminant = D65)
            var x = xyz.X / 100.0;
            var y = xyz.Y / 100.0;
            var z = xyz.Z / 100.0;

            var r = x * 3.2406 + y * -1.5372 + z * -0.4986;
            var g = x * -0.9689 + y * 1.8758 + z * 0.0415;
            var b = x * 0.0557 + y * -0.2040 + z * 1.0570;

            R = ColorCommon.check_double(r > 0.0031308 ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r);
            G = ColorCommon.check_double(g > 0.0031308 ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g);
            B = ColorCommon.check_double(b > 0.0031308 ? 1.055 * Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b);
        }

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
    }
    #endregion

    #region  ===========  HSL  ============
    public class HSL
    {
        public double H; public double S; public double L;

        public HSL(RGB rgb)
        {
            var max = ColorCommon.threeway_max(rgb.R, rgb.G, rgb.B);
            var min = ColorCommon.threeway_min(rgb.R, rgb.G, rgb.B);
            L = (max + min) / 2;

            if (ColorCommon.is_equal(max, min))
                H = S = 0.0; // achromatic
            else
            {
                var d = max - min;
                S = L > 0.5 ? d / (2.0 - max - min) : d / (max + min);
                if (ColorCommon.is_equal(max, rgb.R))
                    H = (rgb.G - rgb.B) / d + (rgb.G < rgb.B ? 6.0 : 0.0);
                else if (ColorCommon.is_equal(max, rgb.G))
                    H = (rgb.B - rgb.R) / d + 2.0;
                else if (ColorCommon.is_equal(max, rgb.B))
                    H = (rgb.R - rgb.G) / d + 4.0;
                H /= 6.0;
            }
        }

        public RGB GetRGB()
        {
            if (ColorCommon.is_equal(S, 0.0))
                return  new RGB(L, L, L); // achromatic
            
            var q = L < 0.5 ? L * (1.0 + S) : L + S - L * S;
            var p = 2.0 * L - q;
            var r = hue2rgb(p, q, H + ColorCommon.OneThird);
            var g = hue2rgb(p, q, H);
            var b = hue2rgb(p, q, H - ColorCommon.OneThird);
            return new RGB(r, g, b);
        }

        internal static double hue2rgb(double p, double q, double t)
        {
            if (t < 0) t += 1.0;
            if (t > 1) t -= 1.0;
            if (t < ColorCommon.OneSixth) return p + (q - p) * 6.0 * t;
            if (t < 0.5) return q;
            if (t < ColorCommon.TwoThirds) return p + (q - p) * (ColorCommon.TwoThirds - t) * 6.0;
            return p;
        }
    }
    #endregion

    #region  ===========  HSV  ============
    public class HSV
    {
        public double H;
        public double S;
        public double V;

        public HSV(RGB rgb)
        {
            var max = ColorCommon.threeway_max(rgb.R, rgb.G, rgb.B);
            var min = ColorCommon.threeway_min(rgb.R, rgb.G, rgb.B);
            var d = max - min;
            V = max;
            S = ColorCommon.is_equal(max, 0.0) ? 0.0 : d / max;

            if (ColorCommon.is_equal(max, min))
                H = 0.0; // achromatic
            else
            {
                if (ColorCommon.is_equal(max, rgb.R))
                    H = (rgb.G - rgb.B) / d + (rgb.G < rgb.B ? 6.0 : 0.0);
                else if (ColorCommon.is_equal(max, rgb.G))
                    H = (rgb.B - rgb.R) / d + 2.0;
                else if (ColorCommon.is_equal(max, rgb.B))
                    H = (rgb.R - rgb.G) / d + 4.0;
                H /= 6.0;
            }
        }
    }
    #endregion

    #region  ===========  XYZ  ============
    public class XYZ
    {
        public static XYZ XyzWhiteReference => new XYZ(95.047, 100.000, 108.883);

        // The XYZ representation: x[0-95.5], y[0-100], and z[0-108.9]
        public double X;
        public double Y;
        public double Z;

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

        private static double PivotRgb(double n) => (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100.0;
    }
    #endregion

    #region  ===========  LAB  ============
    public class LAB
    {
        // The CIELAB representation: L*: lightness [0-100], a*: from green (−) to red (+) component [-127.5, 127.5], b*: from blue (−) to yellow (+) component [-127.5, 127.5]
        public double L;
        public double A;
        public double B;

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

        internal const double Epsilon = 216.0 / 24389.0; // Intent is 0.008856 = 216/24389
        internal const double Kappa = 24389.0 / 27.0; // Intent is 903.3 = 24389/27
        private static double CubicRoot(double n) => Math.Pow(n, ColorCommon.OneThird);
        private static double PivotXyz(double n) => n > Epsilon ? CubicRoot(n) : (Kappa * n + 16) / 116;
    }
    #endregion

}
