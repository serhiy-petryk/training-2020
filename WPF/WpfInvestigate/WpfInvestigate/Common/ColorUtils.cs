using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using WpfInvestigate.Common.ColorSpaces;

namespace WpfInvestigate.Common
{
    public static class ColorUtils
    {
        static ColorUtils()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(t => t.Namespace == "System.Windows.Media" && t.Name == "KnownColor");
            if (type != null)
            {
                var nameList = Enum.GetValues(type).OfType<object>().Skip(1).Select(a => a.ToString()).OrderBy(a => a).ToList();
                nameList.AddRange(new[] { "Cyan", "Aqua", "Fuchsia", "Magenta" });// repeating color values: name may be skipped
                foreach (var colorName in nameList)
                    if (!KnownColors.ContainsKey(colorName))
                        KnownColors.Add(colorName, StringToColor(colorName));
            }
        }

        /// <summary>
        /// 141 known colors from System.Windows.Media.KnownColor enumeration
        /// I don't want to use System.Drawing.KnownColor because I need to add additional assembly to my project
        /// </summary>
        public static Dictionary<string, Color> KnownColors { get; } = new Dictionary<string, Color>();

        public static Color StringToColor(string hexStringOfColor) => (Color)ColorConverter.ConvertFromString(hexStringOfColor);

        /// <summary>
        /// Get color gray level based on the BT.709 standard of YCbCr color space
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns>Gray level representation: double [0, 1]
        public static double GetGrayLevel(RGB rgb) => YCbCr.GetGrayLevel(rgb);

        public static HSL GetHSLByGrayLevel(double hue, double saturation, double newGrayLevel)
        {
            var grayLevel = GetGrayLevel(new HSL(hue, saturation, 0.5).RGB);
            double newL;
            if (grayLevel < newGrayLevel)
                newL = 1.0 - (1.0 - newGrayLevel) / (1.0 - grayLevel) * 0.5;
            else
                newL = newGrayLevel / grayLevel * 0.5;
            return new HSL(hue, saturation, newL);
        }

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
            return hsv.RGB.Color;
        }

        private const double DarkSplit = 0.582;
        private const double DefaultPrecision = 0.0001;
        internal const double OneThird = 1.0 / 3.0;
        internal const double TwoThirds = 2.0 / 3.0;
        internal const double OneSixth = 1.0 / 6.0;

        internal static double threeway_max(double a, double b, double c) => Math.Max(a, Math.Max(b, c));
        internal static double threeway_min(double a, double b, double c) => Math.Min(a, Math.Min(b, c));
        internal static bool is_equal(double d1, double d2) => Math.Abs(d1 - d2) < DefaultPrecision;
        internal static double check_double(double value, double min = 0.0, double max = 1.0) => Math.Min(max, Math.Max(min, value));

    }
}
