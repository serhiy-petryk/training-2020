using System;

namespace ColorInvestigation.Common
{
    public static class Tips
    {
        private const double DefaultPrecision = 0.0001;

        public static bool AreEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < DefaultPrecision;
        }

        public static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;

            if (temp3 < 0.5)
                return temp2;

            if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);

            return temp1;
        }
        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0) return temp3 + 1.0;
            if (temp3 > 1.0) return temp3 - 1.0;
            return temp3;
        }


    }
}
