using System;
using System.Windows.Media;

namespace ColorInvestigation.Temp
{
    class GrayScales
    {
        // Gray scale: https://en.wikipedia.org/wiki/Grayscale
        // https://github.com/A1chem1st/GrayscaleConverter/blob/master/Grayscale%20Converter/Grayscale%20Converter/Program.cs
        // foreground = White if gray>=0.55
        public static double ColorToGrayScale(Color color) => (0.3 * color.R + 0.59 * color.G + 0.11 * color.B) / 256.0;
        // public static double ColorToGrayScale1(Color color) => (1.0 / 3 * color.R + 1.0 / 3 * color.G + 1.0 / 3 * color.B) / 256.0;
        // public static double ColorToGrayScale1(Color color) => (0.16 * color.R + 0.73 * color.G + 0.01 * color.B) / 256.0; // 20 different items
        public static double ColorToGrayScale1(Color color) => (0.16 * color.R + 0.73 * color.G + 0.01 * color.B) / 256.0;
        /*public static double ColorToGrayScale2(Color color)
        {
            double max = Math.Max(color.R, Math.Max(color.G, color.B));
            double min = Math.Min(color.R, Math.Min(color.G, color.B));
            return (max+min) / 2.0 / 256.0;
        }*/
        public static double ColorToGrayScale2(Color color) => (0.17 * color.R + 0.78 * color.G + 0.01 * color.B) / 256.0;
        public static double ColorToGrayScale3(Color color) => (0.21 * color.R + 0.71 * color.G + 0.07 * color.B) / 256.0;
        public static double ColorToGrayScale4(Color color) => (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 256.0;
        public static double ColorToGrayScale5(Color color) => (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 256.0;
        public static double ColorToGrayScale6(Color color) => (0.2627 * color.R + 0.6780 * color.G + 0.0593 * color.B) / 256.0;
        public static double ContrastingForegroundColor(Color color)
        {
            double rgb_srgb(double d)
            {
                d = d / 255.0;
                return d > 0.03928 ? Math.Pow((d + 0.055) / 1.055, 2.4) : d / 12.92;
            }

            var r = rgb_srgb(color.R);
            var g = rgb_srgb(color.G);
            var b = rgb_srgb(color.B);

            var luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;
            return luminance;
            // return luminance > 0.179 ? Colors.Black : Colors.White;
        }


    }
}
