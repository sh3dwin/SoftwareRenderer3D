using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System.Collections;
using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.Fragment
{
    public struct SimpleFragment: IEqualityComparer, IFragment
    {
        public SimpleFragment(Vector2 coordinates, double depth, Vector3 barycentric, IVertex v0, IVertex v1, IVertex v2)
        {
            Depth = depth;
            ScreenCoordinates = coordinates;
            BarycentricCoordinates = barycentric;
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }
        public double Depth { get; set; }
        public Vector2 ScreenCoordinates { get; set; }
        public Vector3 BarycentricCoordinates { get; set; }
        public IVertex V0 { get; }
        public IVertex V1 { get; }
        public IVertex V2 { get; }

        bool IEqualityComparer.Equals(object x, object y)
        {
            if (!y.GetType().IsAssignableFrom(x.GetType()))
                return false;

            var fragmentX = (SimpleFragment)x;
            var fragmentY = (SimpleFragment)y;

            return fragmentX.ScreenCoordinates.X == fragmentY.ScreenCoordinates.X
                && fragmentX.ScreenCoordinates.Y == fragmentY.ScreenCoordinates.Y
                && fragmentX.V0 == fragmentY.V0
                && fragmentX.V1 == fragmentY.V1
                && fragmentX.V2 == fragmentY.V2;
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return ScreenCoordinates.GetHashCode();
        }
    }
}
