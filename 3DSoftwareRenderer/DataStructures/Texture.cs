using SoftwareRenderer3D.Enums;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System.Drawing;
using System.Drawing.Imaging;

namespace SoftwareRenderer3D.DataStructures
{
    public class Texture
    {
        private readonly Bitmap _bitmap;
        private readonly int[,] _rawImageData;
        private readonly bool _flipY = false;

        private readonly int _width;
        private readonly int _height;
        public Texture(Bitmap texture, bool flipY = false) {
            _bitmap = texture;
            _rawImageData = ExtractColorInformation(texture);
            _flipY = flipY;

            _width = texture.Width;
            _height = texture.Height;
        }

        public Color GetTextureColor(float u, float v, TextureInterpolation textureInterpolation)
        {
            v = _flipY ? (1 - v) : v;
            return textureInterpolation == TextureInterpolation.LINEAR ? GetLinearlyInterpolatedColor(u, v) : GetNearestNeighborColor(u, v);
        }

        private Color GetLinearlyInterpolatedColor(float u, float v)
        {
            if (u == 1 || v == 1)
                return Color.FromArgb(GetPixelColor(_width - 1, _height - 1));

            var xWhole = (int)(u * (_width - 1));
            var xFraction = u * (_height - 1) - xWhole;

            var yWhole = (int)(v * (_height - 1));
            var yFraction = v * (_height - 1) - yWhole;

            var topLeft = Color.FromArgb(_rawImageData[xWhole, yWhole]);
            var topRight = Color.FromArgb(_rawImageData[xWhole + 1, yWhole]);
            var bottomLeft = Color.FromArgb(_rawImageData[xWhole, yWhole + 1]);
            var bottomRight = Color.FromArgb(_rawImageData[xWhole + 1, yWhole + 1]);

            var top = topLeft.Mult(xFraction).Add(topRight.Mult(1 - xFraction)).Mult(yFraction);
            var bottom = bottomLeft.Mult(xFraction).Add(bottomRight.Mult(1 - xFraction)).Mult(1 - yFraction);

            return top.Add(bottom);
        }

        public Bitmap AsImage => _bitmap;

        private Color GetNearestNeighborColor(float u, float v)
        {
            lock (this)
            {
                var x = (int)MathUtils.Clamp(u * _width - 1, 0, _width - 1);
                var y = (int)MathUtils.Clamp(v * _height - 1, 0, _height - 1);

                return Color.FromArgb(_rawImageData[x, y]);
            }
        }
        private static int[,] ExtractColorInformation(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int[,] colorArray = new int[width, height];

            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            // Check the pixel format
            bool is32bpp = (bitmap.PixelFormat == PixelFormat.Format32bppArgb ||
                            bitmap.PixelFormat == PixelFormat.Format32bppRgb);

            int pixelSize = is32bpp ? 4 : 3; // 32bpp has 4 bytes per pixel, 24bpp has 3 bytes per pixel

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * bmpData.Stride + x * pixelSize;
                        int color = 0;

                        if (is32bpp)
                        {
                            color = *(int*)(ptr + index);
                        }
                        else
                        {
                            byte b = ptr[index];
                            byte g = ptr[index + 1];
                            byte r = ptr[index + 2];
                            color = (r << 16) | (g << 8) | b;
                        }

                        colorArray[x, y] = color;
                    }
                }
            }
            // Unlock the bits.
            bitmap.UnlockBits(bmpData);

            return colorArray;
        }

        private int GetPixelColor(int x, int y)
        {
            return _rawImageData[x, y];
        }
    }
}
