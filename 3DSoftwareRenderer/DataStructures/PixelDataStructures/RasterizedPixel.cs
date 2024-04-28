using System.Windows.Media;

namespace SoftwareRenderer3D.DataStructures.PixelDataStructures
{
    public struct RasterizedPixel
    {
        public int X;
        public int Y;
        public float Depth;

        public Color Color;

        public RasterizedPixel(int x, int y, float depth, Color color)
        {
            X = x;
            Y = y;
            Depth = depth;
            Color = color;
        }
    }
}
