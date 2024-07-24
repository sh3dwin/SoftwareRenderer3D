using System.Numerics;

namespace SoftwareRenderer3D.Utils
{
    public static class Barycentric
    {
        public static Vector3 CalculateBarycentricCoordinates(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var edge1 = v1 - v0;
            var edge2 = v2 - v0;
            var n = Vector3.Cross(edge1, edge2);

            // Ensure the area is not zero (or too small), otherwise the triangle is degenerate
            var area = n.Length();
            if (area < float.Epsilon)
            {
                return new Vector3(float.NaN, float.NaN, float.NaN);
            }

            var invArea = 1 / area;

            var n1 = Vector3.Cross(v2 - v1, p - v1) * invArea;
            var n2 = Vector3.Cross(-edge2, p - v2) * invArea;
            var n3 = Vector3.Cross(edge1, p - v0) * invArea;

            var alpha = Vector3.Dot(n, n1);
            var beta = Vector3.Dot(n, n2);
            var gamma = Vector3.Dot(n, n3);

            return new Vector3(alpha, beta, gamma);
        }

        public static Vector3 CalculateBarycentricCoordinates(float x, float y, Vector2 v0, Vector2 v1, Vector2 v2)
        {
            var alpha = (-(x - v1.X) * (v2.Y - v1.Y) + (y - v1.Y) * (v2.X - v1.X)) /
                    (-(v0.X - v1.X) * (v2.Y - v1.Y) + (v0.Y - v1.Y) * (v2.X - v1.X));
            var beta = (-(x - v2.X) * (v0.Y - v2.Y) + (y - v2.Y) * (v0.X - v2.X)) /
                (-(v1.X - v2.X) * (v0.Y - v2.Y) + (v1.Y - v2.Y) * (v0.X - v2.X));
            var gamma = 1 - alpha - beta;

            return new Vector3(alpha, beta, gamma);
        }

        public static Vector3 CalculateBarycentricCoordinates(Vector2 p, Vector2 v0, Vector2 v1, Vector2 v2)
        {
            var x = p.X;
            var y = p.Y;

            var alpha = (-(x - v1.X) * (v2.Y - v1.Y) + (y - v1.Y) * (v2.X - v1.X)) /
                    (-(v0.X - v1.X) * (v2.Y - v1.Y) + (v0.Y - v1.Y) * (v2.X - v1.X));
            var beta = (-(x - v2.X) * (v0.Y - v2.Y) + (y - v2.Y) * (v0.X - v2.X)) /
                (-(v1.X - v2.X) * (v0.Y - v2.Y) + (v1.Y - v2.Y) * (v0.X - v2.X));
            var gamma = 1 - alpha - beta;

            return new Vector3(alpha, beta, gamma);
        }
    }
}
