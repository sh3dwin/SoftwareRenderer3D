using System.Drawing;

namespace SoftwareRenderer3D.DataStructures.Buffers
{
    public class FrameBuffer
    {
        private int[,] _colorBuffer;
        private float[,] _depthBuffer;

        private int _width;
        private int _height;

        public FrameBuffer(int width, int height)
        {
            _colorBuffer = GetEmptyIntBuffer(width, height);
            _depthBuffer = GetEmptyFloatBuffer(width, height);

            _width = width;
            _height = height;
        }

        private int[,] GetEmptyIntBuffer(int width, int height)
        {
            var result = new int[width, height];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    result[i, j] = int.MaxValue;
                }
            }
            return result;
        }

        private float[,] GetEmptyFloatBuffer(int width, int height)
        {
            var result = new float[width, height];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    result[i, j] = int.MaxValue;
                }
            }
            return result;
        }

        public void ColorPixel(int x, int y, float z, Color color)
        {
            if (z >= _depthBuffer[x, y])
                return;

            _depthBuffer[x, y] = z;
            _colorBuffer[x, y] = color.ToArgb();
        }

        public Bitmap GetFrame()
        {
            var result = new Bitmap(_width, _height);
            for (var i = 0; i < _height; i++)
            {
                for (var j = 0; j < _width; j++)
                {
                    result.SetPixel(i, j, Color.FromArgb(_colorBuffer[i, j]));
                }
            }

            return result;
        }
    }
}
