using SoftwareRenderer3D.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SoftwareRenderer3D.FrameBuffers
{
    public class FrameBuffer : IFrameBuffer
    {
        private int[] _colorBuffer;
        private float[] _depthBuffer;

        private int _width;
        private int _height;

        public FrameBuffer(int width, int height)
        {
            _colorBuffer = ArrayUtils.GetEmptyIntBuffer(width, height, Constants.BackgroundColor);
            _depthBuffer = ArrayUtils.GetEmptyFloatBuffer(width, height);

            _width = width;
            _height = height;

        }

        public FrameBuffer(FrameBuffer otherFrameBuffer)
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

        public void SetPixelColor(int x, int y, float z, Color color)
        {
            int index = x + y * _width;
            if (z >= _depthBuffer[index])
                return;

            _depthBuffer[index] = z;
            _colorBuffer[index] = color.ToArgb();
        }

        public Bitmap GetFrame()
        {
            var colorBuffer = _colorBuffer;
            var bitsHandle = GCHandle.Alloc(colorBuffer, GCHandleType.Pinned);
            var bitmap = new Bitmap(_width, _height, _width * 4, PixelFormat.Format32bppPArgb, bitsHandle.AddrOfPinnedObject());

            bitsHandle.Free();

            return bitmap;
        }

        public void Update(int width, int height)
        {
            _width = width;
            _height = height;

            _colorBuffer = ArrayUtils.GetEmptyIntBuffer(_width, _height, Constants.BackgroundColor);
            _depthBuffer = ArrayUtils.GetEmptyFloatBuffer(_width, _height);
        }
        


        public static int[] BlendColorBuffers(FrameBuffer first, FrameBuffer second)
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
