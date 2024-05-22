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
            return Color.FromArgb(color.A, R, G, B);
        }

        public static Color Mult(this Color color, double value)
        {
            var R = (byte)(color.R * value);
            var G = (byte)(color.G * value);
            var B = (byte)(color.B * value);
            return Color.FromArgb(color.A, R, G, B);
        }

        public static Color Add(this Color color,  Color otherColor) {
            var R = (byte)MathUtils.Clamp(color.R + otherColor.R, 0, 255);
            var G = (byte)MathUtils.Clamp(color.G + otherColor.G, 0, 255);
            var B = (byte)MathUtils.Clamp(color.B + otherColor.B, 0, 255);
            return Color.FromArgb(color.A, R, G, B);
        }

        public static Color Blend(this Color color, Color otherColor) {

            var alphaFirst = color.A / 255.0;
            var alphaSecond = otherColor.A / 255.0;

            var a0 = alphaFirst + alphaSecond * (1 - alphaFirst);
            var blendedColor = color.Mult(alphaFirst / a0).Add(otherColor.Mult(alphaSecond * (1 - alphaFirst) / a0));

            blendedColor = Color.FromArgb((int)(a0 * 255), blendedColor.R, blendedColor.G, blendedColor.B);

            return blendedColor;
        }
    }
}
