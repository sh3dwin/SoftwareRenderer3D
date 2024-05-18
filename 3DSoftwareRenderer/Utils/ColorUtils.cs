using SoftwareRenderer3D.Utils.GeneralUtils;
using System.Drawing;

namespace SoftwareRenderer3D.Utils
{
    public static class ColorUtils
    {
        public static Color Mult(this Color color, float value){
            var R = (byte)(color.R * value);
            var G = (byte)(color.G * value);
            var B = (byte)(color.B * value);
            return Color.FromArgb(R, G, B);
        }

        public static Color Add(this Color color,  Color otherColor) {
            var R = (byte)MathUtils.Clamp(color.R + otherColor.R, 0, 255);
            var G = (byte)MathUtils.Clamp(color.G + otherColor.G, 0, 255);
            var B = (byte)MathUtils.Clamp(color.B + otherColor.B, 0, 255);
            return Color.FromArgb(R, G, B);
        }
    }
}
