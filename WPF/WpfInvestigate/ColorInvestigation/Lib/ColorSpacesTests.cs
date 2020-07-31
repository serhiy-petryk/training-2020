using System;
using System.Windows.Media;

namespace ColorInvestigation.Lib
{
    public class ColorSpacesTests
    {
        public static double MinX = double.MaxValue;
        public static double MaxX = double.MinValue;
        public static double MinY = double.MaxValue;
        public static double MaxY = double.MinValue;
        public static double MinZ = double.MaxValue;
        public static double MaxZ = double.MinValue;

        public static void HueTest()
        {
            // Compare Hue of HSL and HSV color spaces
            for (var r = 0; r < 256; r++)
            for (var g = 0; g < 256; g++)
            for (var b = 0; b < 256; b++)
            {
                var color = Color.FromRgb((byte)r, (byte)g, (byte)b);
                var hsl = ColorUtilities.ColorToHsl(color);
                var hsv = ColorUtilities.ColorToHsv(color);

                if (Math.Abs(hsl.Item1 - hsv.Item1) > 0.001)
                    throw new Exception("Check!!!");
            }
        }

        public static void HslTest()
        {
            for (var r = 0; r < 256; r++)
            for (var g = 0; g < 256; g++)
            for (var b = 0; b < 256; b++)
            {
                var color = Color.FromRgb((byte)r, (byte)g, (byte)b);
                var hsl = ColorUtilities.ColorToHsl(color);

                if (hsl.Item1 < MinX) MinX = hsl.Item1;
                if (hsl.Item1 > MaxX) MaxX = hsl.Item1;
                if (hsl.Item2 < MinY) MinY = hsl.Item2;
                if (hsl.Item2 > MaxY) MaxY = hsl.Item2;
                if (hsl.Item3 < MinZ) MinZ = hsl.Item3;
                if (hsl.Item3 > MaxZ) MaxZ = hsl.Item3;

                var backColor = ColorUtilities.HslToColor(hsl.Item1, hsl.Item2, hsl.Item3);
                if (color != backColor)
                    throw new Exception("Check!!!");
            }
        }

        public static void HsvTest()
        {
            for (var r = 0; r < 256; r++)
            for (var g = 0; g < 256; g++)
            for (var b = 0; b < 256; b++)
            {
                var color = Color.FromRgb((byte)r, (byte)g, (byte)b);
                var hsv = ColorUtilities.ColorToHsv(color);

                if (hsv.Item1 < MinX) MinX = hsv.Item1;
                if (hsv.Item1 > MaxX) MaxX = hsv.Item1;
                if (hsv.Item2 < MinY) MinY = hsv.Item2;
                if (hsv.Item2 > MaxY) MaxY = hsv.Item2;
                if (hsv.Item3 < MinZ) MinZ = hsv.Item3;
                if (hsv.Item3 > MaxZ) MaxZ = hsv.Item3;

                var backColor = ColorUtilities.HsvToColor(hsv.Item1, hsv.Item2, hsv.Item3);
                if (color != backColor)
                    throw new Exception("Check!!!");
            }
        }

        public static void XyzTest()
        {
            for (var r = 0; r < 256; r++)
            for (var g = 0; g < 256; g++)
            for (var b = 0; b < 256; b++)
            {
                var color = Color.FromRgb((byte)r, (byte)g, (byte)b);
                var xyz = ColorUtilities.ColorToXyz(color);

                if (xyz.Item1 < MinX) MinX = xyz.Item1;
                if (xyz.Item1 > MaxX) MaxX = xyz.Item1;
                if (xyz.Item2 < MinY) MinY = xyz.Item2;
                if (xyz.Item2 > MaxY) MaxY = xyz.Item2;
                if (xyz.Item3 < MinZ) MinZ = xyz.Item3;
                if (xyz.Item3 > MaxZ) MaxZ = xyz.Item3;

                var backColor = ColorUtilities.XyzToColor(xyz.Item1, xyz.Item2, xyz.Item3);
                if (color != backColor)
                    throw new Exception("Check!!!");
            }
        }

        public static void LabTest()
        {
            for (var r = 0; r < 256; r++)
            for (var g = 0; g < 256; g++)
            for (var b = 0; b < 256; b++)
            {
                var color = Color.FromRgb((byte)r, (byte)g, (byte)b);
                var lab = ColorUtilities.ColorToLab(color);

                if (lab.Item1 < MinX) MinX = lab.Item1;
                if (lab.Item1 > MaxX) MaxX = lab.Item1;
                if (lab.Item2 < MinY) MinY = lab.Item2;
                if (lab.Item2 > MaxY) MaxY = lab.Item2;
                if (lab.Item3 < MinZ) MinZ = lab.Item3;
                if (lab.Item3 > MaxZ) MaxZ = lab.Item3;

                var backColor = ColorUtilities.LabToColor(lab.Item1, lab.Item2, lab.Item3);
                if (color != backColor)
                    throw new Exception("Check!!!");
            }
        }

        public static void YCbCrTest(ColorUtilities.YCbCrType yCbCrType)
        {
            for (var r = 0; r < 256; r++)
            for (var g = 0; g < 256; g++)
            for (var b = 0; b < 256; b++)
            {
                var color = Color.FromRgb((byte)r, (byte)g, (byte)b);
                var yCbCr = ColorUtilities.ColorToYCbCr(color, yCbCrType);

                if (yCbCr.Item1 < MinX) MinX = yCbCr.Item1;
                if (yCbCr.Item1 > MaxX) MaxX = yCbCr.Item1;
                if (yCbCr.Item2 < MinY) MinY = yCbCr.Item2;
                if (yCbCr.Item2 > MaxY) MaxY = yCbCr.Item2;
                if (yCbCr.Item3 < MinZ) MinZ = yCbCr.Item3;
                if (yCbCr.Item3 > MaxZ) MaxZ = yCbCr.Item3;

                var backColor = ColorUtilities.YCbCrToColor(yCbCr.Item1, yCbCr.Item2, yCbCr.Item3, yCbCrType);
                if (color != backColor)
                    throw new Exception("Check!!!");
            }
        }



    }
}
