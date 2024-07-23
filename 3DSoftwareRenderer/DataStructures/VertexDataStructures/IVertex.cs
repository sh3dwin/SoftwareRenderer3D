using System.Drawing;
using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.VertexDataStructures
{
    public interface IVertex
    {
        Vector3 GetVertexPoint();

        Color Color { get; set; }
    }
}
