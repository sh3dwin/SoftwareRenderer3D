using System.Drawing;

namespace SoftwareRenderer3D.DataStructures.Buffers
{
    public class FrameBuffer
    {
        private int[,] _colorBuffer;
        private int[,] _depthBuffer;

        private int _width;
        private int _height;

        public FrameBuffer(int width, int height)
        {
            _colorBuffer = GetEmptyBuffer(width, height);
            _depthBuffer = GetEmptyBuffer(width, height);

            _width = width;
            _height = height;
        }

        private int[,] GetEmptyBuffer(int width, int height)
        {
            var result = new int[width, height];
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    result[i, j] = int.MaxValue;
                }
            }
            return result;
        }

        public void ColorPixel(int y, int x, int z, Color color)
        {
            if (z >= _depthBuffer[x, y])
                return;

            _colorBuffer[x, y] = color.ToArgb();
        }

        public Bitmap GetFrame()
        {
            var result = new Bitmap(_width, _height);
            for (var i = 0; i < _width; i++)
            {
                for (var j = 0; j < _height; j++)
                {
                    result.SetPixel(i, j, Color.FromArgb(_colorBuffer[i, j]));
                }
            }

            return result;
        }
    }
}
