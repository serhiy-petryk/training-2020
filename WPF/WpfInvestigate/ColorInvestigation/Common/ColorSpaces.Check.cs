using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace ColorInvestigation.Common
{
    public static class ColorSpacesCheck
    {
        public static void RunTests()
        {
            // ColorSpacesCheck.CheckRGB();
            // CheckHSL();
            CheckHSV();
        }

        private const double DefaultPrecision = 0.0001;
        private static bool IsEqual(double d1, double d2) => Math.Abs(d1 - d2) < DefaultPrecision;
        private static bool AerRGBEqual(ColorSpaces.RGB rgb1, ColorSpaces.RGB rgb2)
        {
            return IsEqual(rgb1.R, rgb2.R) && IsEqual(rgb1.G, rgb2.G) && IsEqual(rgb1.B, rgb2.B);
        }

        private static List<Color> _cachedTestData;
        private static List<Color> GetTestData()
        {
            if (_cachedTestData == null)
            {
                const int step = 2;
                _cachedTestData = new List<Color>();
                for (var r = 0; r < 256; r += step)
                for (var g = 0; g < 256; g += step)
                for (var b = 0; b < 256; b += step)
                    _cachedTestData.Add(Color.FromRgb((byte)r, (byte)g, (byte)b));
                _cachedTestData.Add(Color.FromRgb(255, 255,255));
            }
            return _cachedTestData;
        }


        private static void CheckRGB()
        {
            var testData = GetTestData();
            var a1 = testData[testData.Count - 1];

            foreach (var color in testData)
            {
                var rgb = new ColorSpaces.RGB(color);
                var color2 = rgb.GetColor();
                if (color != color2)
                    throw new Exception($"CheckRGB error. Color: {color}. Color2: {color2}. RGB: {rgb}");
            }
        }

        private static void CheckHSL()
        {
            var testData = GetTestData();
            var a1 = testData[testData.Count - 1];

            foreach (var color in testData)
            {
                var rgb = new ColorSpaces.RGB(color);
                var hsl = new ColorSpaces.HSL(rgb);
                var rgb2 = hsl.GetRGB();
                if (!AerRGBEqual(rgb, rgb2))
                    throw new Exception($"CheckHSL error. Color: {color}. RGB: {rgb}. RGB2: {rgb2}");
            }
        }
        private static void CheckHSV()
        {
            var testData = GetTestData();
            var a1 = testData[testData.Count - 1];

            foreach (var color in testData)
            {
                var rgb = new ColorSpaces.RGB(color);
                var hsv = new ColorSpaces.HSV(rgb);
                var rgb2 = hsv.GetRGB();
                if (!AerRGBEqual(rgb, rgb2))
                    throw new Exception($"CheckHSV error. Color: {color}. RGB: {rgb}. RGB2: {rgb2}");
            }
        }
    }
}
