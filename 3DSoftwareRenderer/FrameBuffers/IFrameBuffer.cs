using System.Drawing;

namespace SoftwareRenderer3D.FrameBuffers
{
    public interface IFrameBuffer
    {
        (int Width, int Height) GetSize();
        void Update(int width, int height);
        void SetPixelColor(int x, int y, float depth, Color color);
        Bitmap GetFrame();

    }
}
