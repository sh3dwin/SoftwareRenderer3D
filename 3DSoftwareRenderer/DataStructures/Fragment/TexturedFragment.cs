using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SoftwareRenderer3D.DataStructures.Fragment
{
    internal struct TexturedFragment : IFragment, IEqualityComparer
    {
        public TexturedFragment(Vector2 coordinates, double depth, Vector3 barycentric, IVertex v0, IVertex v1, IVertex v2)
        {
            Depth = depth;
            ScreenCoordinates = coordinates;
            BarycentricCoordinates = barycentric;
            V0 = v0;
            V1 = v1;
            V2 = v2;

            TextureCoordinates = ((TexturedVertex)V0).TextureCoordinates * BarycentricCoordinates.X
                + ((TexturedVertex)V1).TextureCoordinates * BarycentricCoordinates.Y
                + ((TexturedVertex)V2).TextureCoordinates * BarycentricCoordinates.Z;
        }
        public double Depth { get; }
        public Vector2 ScreenCoordinates { get; }
        public Vector3 BarycentricCoordinates { get; }
        public IVertex V0 { get; }
        public IVertex V1 { get; }
        public IVertex V2 { get; }

        public Vector2 TextureCoordinates { get; }

        bool IEqualityComparer.Equals(object x, object y)
        {
            if (!y.GetType().IsAssignableFrom(x.GetType()))
                return false;

            var fragmentX = (TexturedFragment)x;
            var fragmentY = (TexturedFragment)y;

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
