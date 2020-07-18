using System;
using System.Collections.Generic;
using System.Linq;
using ColorInvestigation.Views;

namespace ColorInvestigation.Temp
{
    public class Class1
    {
        public static void Calc()
        {
            // Підібоати kR, kG, kB, split1/2, щоб максимально співпасти з ContrastingForegroundColor
            var result = new List<Tuple<double, double, double, double, double>>();
            var colors = GrayScaleDiff.GetColors().ToArray();
            var step1 = 0.001;
            var step2 = 0.01;
            var min = int.MaxValue;
            for (var split1 = 0.50; split1 < 0.65; split1 += step1)
            for (var split2 = 0.37; split2 < 0.39; split2 += step2)
            for (var r = 0.15; r < 0.3; r += step2)
            for (var g = 0.7; g < 0.85; g += step2)
            for (var b = 0.0; b < 0.2; b += step2)
                foreach (var color in colors)
                {
                    var gray1 = (r * color.R + g * color.G + b * color.B) / 256.0;
                    var gray2 = GrayScales.ContrastingForegroundColor(color);
                    var cnt = 0;
                    var a1 = gray1 < split1 ? 0 : 1;
                    var a2 = gray2 < split2 ? 0 : 1;
                    if (a1 == a2) continue;
                    cnt++;

                    if (cnt < min)
                    {
                        result.Clear();
                        min = cnt;
                    }
                    if (cnt == min)
                        result.Add(new Tuple<double, double, double, double, double>(split1, split2, r, g, b));
                }
        }

    }
}

