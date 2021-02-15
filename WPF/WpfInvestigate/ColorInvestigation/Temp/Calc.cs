using System;
using System.Collections.Generic;
using ColorInvestigation.Views;

namespace ColorInvestigation.Temp
{
    public class Calc
    {
        public static void Calculate()
        {
            // Неправильний підхід:
            // Найкращий результат дає для split1=0.37-0.76, split2=0.8, r=0.1-0.2, g=0.3-0.7, b=0, що візуально неправильно
            var result = new List<Tuple<double, double, double, double, double>>();
            var colors = GrayScaleDiff.GetColors();
            var step1 = 0.001;
            var step2 = 0.001;
            var min = int.MaxValue;
            for (var split1 = 0.55; split1 < 0.63; split1 += step1)
            for (var split2 = 0.38; split2 < 0.39; split2 += step1)
            for (var r = 0.15; r < 0.175; r += step2)
            for (var g = 0.7; g < 0.81; g += step2)
            for (var b = 0.002; b < 0.015; b += step2)
            {
                var cnt = 0;
                foreach (var color in colors)
                {
                    var gray1 = (r * color.R + g * color.G + b * color.B) / 255.0;
                    var gray2 = GrayScales.ContrastingForegroundColor(color);
                    var a1 = gray1 < split1 ? 0 : 1;
                    var a2 = gray2 < split2 ? 0 : 1;
                    if (a1 == a2) continue;
                    cnt++;
                }
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
