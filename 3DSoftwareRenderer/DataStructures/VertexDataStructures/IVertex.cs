using System.Drawing;
using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.VertexDataStructures
{
    public interface IVertex
    {
        Vector3 Normal { get; }
        Vector3 WorldPoint { get; set; }
        Vector3 NDCPosition { get; set; }
        Vector3 ScreenPosition { get; }
        Color Color { get; set; }
        void SetScreenCoordinates(int width, int height);
        void SetNormal(Vector3 normal);
    }
}
