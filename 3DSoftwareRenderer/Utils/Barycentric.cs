using System;
using System.Numerics;
using System.Security.Policy;

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

            if (alpha > 1)
                return new Vector3(1, 0, 0);
            if (alpha < 0)
                return new Vector3(0, beta / (Math.Abs(beta) + Math.Abs(gamma)), gamma / (Math.Abs(beta) + Math.Abs(gamma)));
            if (beta > 1)
                return new Vector3(0, 1, 0);
            if (beta < 0)
                return new Vector3(alpha / (Math.Abs(alpha) + Math.Abs(gamma)), 0, gamma / (Math.Abs(alpha) + Math.Abs(gamma)));
            if (gamma > 1)
                return new Vector3(0, 0, 1);
            if (gamma < 0)
                return new Vector3(alpha / (Math.Abs(alpha) + Math.Abs(beta)), beta / (Math.Abs(alpha) + Math.Abs(beta)), 0);

            return new Vector3(alpha, beta, gamma);
        }

        public static Vector3 CalculateBarycentricCoordinates(float x, float y, Vector2 v0, Vector2 v1, Vector2 v2)
        {
            var alpha = (-(x - v1.X) * (v2.Y - v1.Y) + (y - v1.Y) * (v2.X - v1.X)) /
                    (-(v0.X - v1.X) * (v2.Y - v1.Y) + (v0.Y - v1.Y) * (v2.X - v1.X));
            var beta = (-(x - v2.X) * (v0.Y - v2.Y) + (y - v2.Y) * (v0.X - v2.X)) /
                (-(v1.X - v2.X) * (v0.Y - v2.Y) + (v1.Y - v2.Y) * (v0.X - v2.X));
            var gamma = 1 - alpha - beta;

            if (alpha > 1)
                return new Vector3(1, 0, 0);
            if (alpha < 0)
                return new Vector3(0, beta / (Math.Abs(beta) + Math.Abs(gamma)), gamma / (Math.Abs(beta) + Math.Abs(gamma)));
            if (beta > 1)
                return new Vector3(0, 1, 0);
            if (beta < 0)
                return new Vector3(alpha / (Math.Abs(alpha) + Math.Abs(gamma)), 0, gamma / (Math.Abs(alpha) + Math.Abs(gamma)));
            if (gamma > 1)
                return new Vector3(0, 0, 1);
            if (gamma < 0)
                return new Vector3(alpha / (Math.Abs(alpha) + Math.Abs(beta)), beta / (Math.Abs(alpha) + Math.Abs(beta)), 0);

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

            if (alpha > 1)
                return new Vector3(1, 0, 0);
            if (alpha < 0)
                return new Vector3(0, beta / (Math.Abs(beta) + Math.Abs(gamma)), gamma / (Math.Abs(beta) + Math.Abs(gamma)));
            if (beta > 1)
                return new Vector3(0, 1, 0);
            if (beta < 0)
                return new Vector3(alpha / (Math.Abs(alpha) + Math.Abs(gamma)), 0, gamma / (Math.Abs(alpha) + Math.Abs(gamma)));
            if (gamma > 1)
                return new Vector3(0, 0, 1);
            if (gamma < 0)
                return new Vector3(alpha / (Math.Abs(alpha) + Math.Abs(beta)), beta / (Math.Abs(alpha) + Math.Abs(beta)), 0);

            return new Vector3(alpha, beta, gamma);
        }
    }
}
