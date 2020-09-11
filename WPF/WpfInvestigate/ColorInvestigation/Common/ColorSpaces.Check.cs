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
            // CheckHSV();
            // CheckXYZ();
            // CheckLAB();
            // CheckYCbCr();
        }

        private static void CheckColorRange()
        {
            var step = 0.1;
            var lab = new ColorSpaces.LAB(new ColorSpaces.RGB(0, 0, 0));
            for (var k1 = 0.0; k1 <= 100.00001; k1 += step)
            for (var k2 = -127.5; k2 <= 127.5000001; k2 += step)
            for (var k3 = -127.5; k3 <= 127.5000001; k3 += step)
            {
                lab.L = k1;
                lab.A = k2;
                lab.B = k3;
            }
        }


        private const double DefaultPrecision = 0.0001;
        private static bool IsEqual(double d1, double d2) => Math.Abs(d1 - d2) < DefaultPrecision;
        private static bool AreRGBEqual(ColorSpaces.RGB rgb1, ColorSpaces.RGB rgb2)
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
                if (!AreRGBEqual(rgb, rgb2))
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
                if (!AreRGBEqual(rgb, rgb2))
                    throw new Exception($"CheckHSV error. Color: {color}. RGB: {rgb}. RGB2: {rgb2}");
            }
        }
        private static void CheckXYZ()
        {
            var testData = GetTestData();
            var a1 = testData[testData.Count - 1];

            foreach (var color in testData)
            {
                var rgb = new ColorSpaces.RGB(color);
                var xyz = new ColorSpaces.XYZ(rgb);
                var color2 = xyz.GetRGB().GetColor();
                if (color != color2)
                    throw new Exception($"CheckXYZ error. Color: {color}. Color2: {color2}");
            }
        }

        private static void CheckLAB()
        {
            var testData = GetTestData();
            var a1 = testData[testData.Count - 1];

            var minL = double.MaxValue; // [0, 100]
            var minA = double.MaxValue; // [-86, 98]
            var minB = double.MaxValue; // [-107, 94]
            var maxL = double.MinValue;
            var maxA = double.MinValue;
            var maxB = double.MinValue;

            foreach (var color in testData)
            {
                var rgb = new ColorSpaces.RGB(color);
                var lab = new ColorSpaces.LAB(rgb);
                if (lab.L < minL) minL = lab.L;
                if (lab.A < minA) minA = lab.A;
                if (lab.B < minB) minB = lab.B;

                if (lab.L > maxL) maxL = lab.L;
                if (lab.A > maxA) maxA = lab.A;
                if (lab.B > maxB) maxB = lab.B;

                var color2 = lab.GetRGB().GetColor();
                if (color != color2)
                    throw new Exception($"CheckLAB error. Color: {color}. Color2: {color2}");
            }
        }

        private static void CheckYCbCr()
        {
            CheckYCbCr(ColorSpaces.YCbCrStandard.BT601);
            CheckYCbCr(ColorSpaces.YCbCrStandard.BT709);
            CheckYCbCr(ColorSpaces.YCbCrStandard.BT2020);
            CheckYCbCr(ColorSpaces.YCbCrStandard.My);
        }
        private static void CheckYCbCr(ColorSpaces.YCbCrStandard yCbCrStandard)
        {
            var testData = GetTestData();
            var a1 = testData[testData.Count - 1];

            foreach (var color in testData)
            {
                var rgb = new ColorSpaces.RGB(color);
                var yCbCr = new ColorSpaces.YCbCr(rgb, yCbCrStandard);
                var rgb2 = yCbCr.GetRGB();
                if (!AreRGBEqual(rgb, rgb2))
                    throw new Exception($"CheckYCbCr error. Color: {color}. RGB: {rgb}. RGB2: {rgb2}. yCbCrStandard: {yCbCrStandard}");
            }
        }
    }
}
