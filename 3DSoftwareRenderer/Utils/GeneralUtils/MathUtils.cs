using System;

namespace SoftwareRenderer3D.Utils.GeneralUtils
{
    public static class MathUtils
    {
        public static double Clamp(this double x, double min = 0, double max = 1)
        {
            return Math.Min(max, Math.Max(x, min));
        }

        public static float Clamp(this float x, float min = 0, float max = 1)
        {
            return Math.Min(max, Math.Max(x, min));
        }
    }
}
