using System.Drawing;

namespace SoftwareRenderer3D.FrameBuffers
{
    public interface IFrameBuffer
    {
        (int Width, int Height) GetSize();
        void Update(int width, int height);
        void SetPixelColor(int x, int y, float depth, byte a, byte r, byte g, byte b);
        void SetPixelColor(int x, int y, float depth, int argb);
        Bitmap GetFrame();

    }
}
