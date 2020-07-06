using System;
using System.Windows.Media;

// using System.Drawing;

namespace WpfInvestigate.Temp
{
    public class ColorUtilities
    {
        public static void Test()
        {
            var a0 = Colors.Red;
            var a01 = InvertColor(a0);
            var a02 = InvertColor(Colors.Aqua);
        }

        public static Color InvertColor(Color color)
        {
            double hue;
            double saturation;
            double value;
            ColorToHSV(color, out hue, out saturation, out value);
            double newHue = (hue + 180.0) % 360.0;
            var newColor = ColorFromHSV(newHue, saturation, value);
            return newColor;
        }

        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            byte max = Math.Max(color.R, Math.Max(color.G, color.B));
            byte min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = GetHue(color);
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            var hi = Convert.ToByte(Convert.ToInt32(Math.Floor(hue / 60)) % 6);
            var f = hue / 60.0 - Math.Floor(hue / 60.0);

            value = value * 255;
            var v = Convert.ToByte(value);
            var p = Convert.ToByte(value * (1 - saturation));
            var q = Convert.ToByte(value * (1 - f * saturation));
            var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            return Color.FromArgb(255, v, p, q);
        }

        private static double GetHue(Color color)
        {
            if (color.R == color.G && color.G == color.B)
                return 0.0;

            var num = color.R / 255.0;
            var num2 = color.G / 255.0;
            var num3 = color.B / 255.0;
            var num4 = 0.0;
            var num5 = num;
            var num6 = num;
            if (num2 > num5) num5 = num2;
            if (num3 > num5) num5 = num3;
            if (num2 < num6) num6 = num2;
            if (num3 < num6) num6 = num3;
            var num7 = num5 - num6;
            if (num == num5)
                num4 = (num2 - num3) / num7;
            else
            {
                if (num2 == num5)
                    num4 = 2.0 + (num3 - num) / num7;
                else if 
                    (num3 == num5) num4 = 4.0 + (num - num2) / num7;
            }
            num4 *= 60.0;
            if (num4 < 0.0) num4 += 360.0;

            return num4;
        }

	}
}
