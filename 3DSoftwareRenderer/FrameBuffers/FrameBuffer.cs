using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SoftwareRenderer3D.FrameBuffers
{
    public class FrameBuffer
    {
        private int[] _colorBuffer;
        private float[] _depthBuffer;

        private int _width;
        private int _height;

        public FrameBuffer(int width, int height)
        {
            _colorBuffer = GetEmptyIntBuffer(width, height);
            _depthBuffer = GetEmptyFloatBuffer(width, height);

            _width = width;
            _height = height;
        }

        private int[] GetEmptyIntBuffer(int width, int height)
        {
            var result = new int[height * width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    int index = j + i * width;
                    result[index] = int.MaxValue;
                }
            }
            return result;
        }

        private float[] GetEmptyFloatBuffer(int width, int height)
        {
            var result = new float[height * width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    int index = j + i * width;
                    result[index] = int.MaxValue;
                }
            }
            return result;
        }

        public void ColorPixel(int x, int y, float z, Color color)
        {
            int index = x + y * _width;
            if (z >= _depthBuffer[index])
                return;

            _depthBuffer[index] = z;
            _colorBuffer[index] = color.ToArgb();
        }

        public Bitmap GetFrame()
        {
            var bitsHandle = GCHandle.Alloc(_colorBuffer, GCHandleType.Pinned);
            var bitmap = new Bitmap(_width, _height, _width * 4, PixelFormat.Format32bppPArgb, bitsHandle.AddrOfPinnedObject());

            bitsHandle.Free();

            return bitmap;
        }

        public void Update(float width, float height)
        {
            _width = (int)width;
            _height = (int)height;

            _colorBuffer = GetEmptyIntBuffer(_width, _height);
            _depthBuffer = GetEmptyFloatBuffer(_width, _height);
        }
    }
}
