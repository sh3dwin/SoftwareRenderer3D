using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.Fragment
{
    public interface IFragment
    {
        double Depth { get; set; }
        Vector2 ScreenCoordinates { get; set; }
        Vector3 BarycentricCoordinates { get; set; }
        IVertex V0 { get; }
        IVertex V1 { get; }
        IVertex V2 { get; }
    }
}