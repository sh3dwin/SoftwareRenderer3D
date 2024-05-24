using SoftwareRenderer3D.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SoftwareRenderer3D.FrameBuffers
{
    public class DepthPeelingBuffer : IFrameBuffer
    {
        private int[] _colorBuffer;

        private float[] _depthBuffer;
        private float[] _minDepthBuffer;

        private int _width;
        private int _height;

        public DepthPeelingBuffer(int width, int height)
        {
            _colorBuffer = GetEmptyIntBuffer(width, height, int.MaxValue);
            _depthBuffer = GetEmptyFloatBuffer(width, height, float.MaxValue);
            _minDepthBuffer = GetEmptyFloatBuffer(width, height, float.MinValue);

            _width = width;
            _height = height;

        }

        public DepthPeelingBuffer(DepthPeelingBuffer otherFrameBuffer)
        {
            _width = otherFrameBuffer._width;
            _height = otherFrameBuffer._height;

            _colorBuffer = otherFrameBuffer._colorBuffer;
            _depthBuffer = otherFrameBuffer._depthBuffer;
        }

        public (int Width, int Height) GetSize()
        {
            return (_width, _height);
        }

        public void ColorPixel(int x, int y, float z, Color color)
        {
            int index = x + y * _width;
            if (z >= _depthBuffer[index] || z <= _minDepthBuffer[index])
                return;

            Color blendedColor;

            // Something has been drawn in the previous pass
            if (_minDepthBuffer[index] != float.MinValue)
                blendedColor = Color.FromArgb(_colorBuffer[index]).Blend(color);
            else
                blendedColor = color.Blend(Color.Transparent);

            _depthBuffer[index] = z;
            _colorBuffer[index] = blendedColor.ToArgb();
        }

        public Bitmap GetFrame()
        {
            var bitsHandle = GCHandle.Alloc(_colorBuffer, GCHandleType.Pinned);
            var bitmap = new Bitmap(_width, _height, _width * 4, PixelFormat.Format32bppPArgb, bitsHandle.AddrOfPinnedObject());

            bitsHandle.Free();

            return bitmap;
        }

        public void Update(int width, int height)
        {
            _width = width;
            _height = height;

            _colorBuffer = GetEmptyIntBuffer(width, height, int.MaxValue);
            _depthBuffer = GetEmptyFloatBuffer(width, height, float.MaxValue);
            _minDepthBuffer = GetEmptyFloatBuffer(width, height, float.MinValue);
        }

        public void DepthPeel()
        {

            var count = 0;

            for (var row = 0; row < _height; row++)
            {
                for (var col = 0; col < _width; col++)
                {
                    var index = row * _width + col;

                    if (_depthBuffer[index] != float.MaxValue)
                    {
                        _minDepthBuffer[index] = _depthBuffer[index];
                        count++;
                    }
                }
            }

            _depthBuffer = GetEmptyFloatBuffer(_width, _height, float.MaxValue);

        }

        private float[] GetEmptyFloatBuffer(int width, int height, float value)
        {
            var result = new float[height * width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    int index = j + i * width;
                    result[index] = value;
                }
            }
            return result;
        }

        private int[] GetEmptyIntBuffer(int width, int height, int value)
        {
            var result = new int[height * width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    int index = j + i * width;
                    result[index] = value;
                }
            }
            return result;
        }


        public static int[] BlendColorBuffers(DepthPeelingBuffer first, DepthPeelingBuffer second)
        {
            if (first == null || second == null)
                return null;

            if (first._width != second._width || first._height != second._height)
                throw new ArgumentException($"Cannot blend buffers with size ({first._width}, {first._height}) and ({second._width}, {second._width})!");

            var width = first._width;
            var height = first._height;

            var colorBufferFirst = first._colorBuffer;
            var colorBufferSecond = second._colorBuffer;

            var depthBufferFirst = first._depthBuffer;
            var depthBufferSecond = second._depthBuffer;

            var blendedColorBuffer = new int[width * height];

            for (var row = 0; row < height; row++)
            {
                for (var col = 0; col < width; col++)
                {
                    var index = row * width + col;

                    if (depthBufferFirst[index] == float.MaxValue && depthBufferSecond[index] == float.MaxValue)
                        continue;

                    var colorFirst = Color.FromArgb(colorBufferFirst[index]);
                    var colorSecond = Color.FromArgb(colorBufferSecond[index]);

                    var blendedColor = colorFirst.Blend(colorSecond);

                    blendedColorBuffer[index] = blendedColor.ToArgb();
                }
            }

            return blendedColorBuffer;
        }
    }
}
