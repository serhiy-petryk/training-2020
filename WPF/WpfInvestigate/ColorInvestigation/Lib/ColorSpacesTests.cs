using System;
using System.Windows.Media;
using ColorInvestigation.Common;
using ColorInvestigation.Controls;

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
                var hsl = new ColorSpaces.HSL(new ColorSpaces.RGB(color));
                var hsv = new ColorSpaces.HSV(new ColorSpaces.RGB(color));

                if (Math.Abs(hsl.H - hsv.H) > 0.001)
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
                var hsl = new ColorSpaces.HSL(new ColorSpaces.RGB(color));

                if (hsl.H < MinX) MinX = hsl.H;
                if (hsl.H > MaxX) MaxX = hsl.H;
                if (hsl.S < MinY) MinY = hsl.S;
                if (hsl.S > MaxY) MaxY = hsl.S;
                if (hsl.L < MinZ) MinZ = hsl.L;
                if (hsl.L > MaxZ) MaxZ = hsl.L;

                var backColor = hsl.GetRGB().Color;
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
                var hsv = new ColorSpaces.HSV(new ColorSpaces.RGB(color));

                if (hsv.H < MinX) MinX = hsv.H;
                if (hsv.H > MaxX) MaxX = hsv.H;
                if (hsv.S < MinY) MinY = hsv.S;
                if (hsv.S > MaxY) MaxY = hsv.S;
                if (hsv.V < MinZ) MinZ = hsv.V;
                if (hsv.V > MaxZ) MaxZ = hsv.V;

                var backColor = hsv.GetRGB().Color;
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
                var xyz = new ColorSpaces.XYZ(new ColorSpaces.RGB(color));

                if (xyz.X < MinX) MinX = xyz.X;
                if (xyz.X > MaxX) MaxX = xyz.X;
                if (xyz.Y < MinY) MinY = xyz.Y;
                if (xyz.Y > MaxY) MaxY = xyz.Y;
                if (xyz.Z < MinZ) MinZ = xyz.Z;
                if (xyz.Z > MaxZ) MaxZ = xyz.Z;

                var backColor = xyz.GetRGB().Color;
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
                var lab = new ColorSpaces.LAB(new ColorSpaces.RGB(color));

                if (lab.L < MinX) MinX = lab.L;
                if (lab.L > MaxX) MaxX = lab.L;
                if (lab.A < MinY) MinY = lab.A;
                if (lab.A > MaxY) MaxY = lab.A;
                if (lab.B < MinZ) MinZ = lab.B;
                if (lab.B > MaxZ) MaxZ = lab.B;

                var backColor = lab.GetRGB().Color;
                if (color != backColor)
                    throw new Exception("Check!!!");
            }
        }

        public static void YCbCrTest(ColorSpaces.YCbCrStandard yCbCrStandard)
        {
            for (var r = 0; r < 256; r++)
            for (var g = 0; g < 256; g++)
            for (var b = 0; b < 256; b++)
            {
                var color = Color.FromRgb((byte)r, (byte)g, (byte)b);
                var yCbCr = new ColorSpaces.YCbCr(new ColorSpaces.RGB(color), yCbCrStandard);

                if (yCbCr.Y < MinX) MinX = yCbCr.Y;
                if (yCbCr.Y > MaxX) MaxX = yCbCr.Y;
                if (yCbCr.Cb < MinY) MinY = yCbCr.Cb;
                if (yCbCr.Cb > MaxY) MaxY = yCbCr.Cb;
                if (yCbCr.Cr < MinZ) MinZ = yCbCr.Cr;
                if (yCbCr.Cr > MaxZ) MaxZ = yCbCr.Cr;

                var backColor = yCbCr.GetRGB().Color;
                if (color != backColor)
                    throw new Exception("Check!!!");
            }
        }
    }
}
