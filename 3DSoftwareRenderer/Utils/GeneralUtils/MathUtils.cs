using System;
using System.Numerics;

namespace SoftwareRenderer3D.Utils.GeneralUtils
{
    public static class MathUtils
    {
        public static double Clamp(this double x, double min = 0, double max = 1)
        {
            return Math.Min(max, Math.Max(x, min));
        }

        public static float Clamp(this float x, float min = 0, float max = 1)
        {
            return Math.Min(max, Math.Max(x, min));
        }

        public static byte Clamp(this byte x, byte min = 0, byte max = 255)
        {
            return Math.Min(max, Math.Max(x, min));
        }

        public static int Clamp(this int x, int min = 0, int max = int.MaxValue)
        {
            return Math.Min(max, Math.Max(x, min));
        }

        public static Vector3 Normalize(this Vector3 vector)
        {
            return Vector3.Normalize(vector);
        }

        public static Matrix4x4 RotateAroundAxis(float angle, Vector3 axis)
        {
            return new Matrix4x4(
                // First row
                (float)(Math.Pow(axis.X, 2) + (1 - Math.Pow(axis.X, 2)) * Math.Cos(angle)),
                (float)(axis.X * axis.Y * (1 - Math.Cos(angle)) - axis.Z * Math.Sin(angle)),
                (float)(axis.Z * axis.X * (1 - Math.Cos(angle)) + axis.Y * Math.Sin(angle)),
                0,

                // Second Row
                (float)(axis.X * axis.Y * (1 - Math.Cos(angle)) + axis.Z * Math.Sin(angle)),
                (float)(Math.Pow(axis.Y, 2) + (1 - Math.Pow(axis.Y, 2)) * Math.Cos(angle)),
                (float)(axis.Z * axis.Y * (1 - Math.Cos(angle)) - axis.X * Math.Sin(angle)),
                0,

                // Third Row
                (float)(axis.Z * axis.X * (1 - Math.Cos(angle)) - axis.Y * Math.Sin(angle)),
                (float)(axis.Z * axis.Y * (1 - Math.Cos(angle)) + axis.X * Math.Sin(angle)),
                (float)(Math.Pow(axis.Z, 2) + (1.0 - Math.Pow(axis.Z, 2)) * Math.Cos(angle)),
                0,

                // Fourth row
                0,
                0,
                0,
                1
                );
        }

        public static Vector4 TransformHomogeneus(this Vector3 vector, Matrix4x4 matrix)
        {
            var homogeneusVector = new Vector4(vector.X, vector.Y, vector.Z, 1);

            var x = homogeneusVector.X * matrix.M11 + homogeneusVector.Y * matrix.M12 + homogeneusVector.Z * matrix.M13 + homogeneusVector.W * matrix.M14;
            var y = homogeneusVector.X * matrix.M21 + homogeneusVector.Y * matrix.M22 + homogeneusVector.Z * matrix.M23 + homogeneusVector.W * matrix.M24;
            var z = homogeneusVector.X * matrix.M31 + homogeneusVector.Y * matrix.M32 + homogeneusVector.Z * matrix.M33 + homogeneusVector.W * matrix.M34;
            var w = homogeneusVector.X * matrix.M41 + homogeneusVector.Y * matrix.M42 + homogeneusVector.Z * matrix.M43 + homogeneusVector.W * matrix.M44;

            return new Vector4(x, y, z, w);

        }

        public static Vector4 Transform(this Vector4 vector, Matrix4x4 matrix)
        {
            var x = vector.X * matrix.M11 + vector.Y * matrix.M12 + vector.Z * matrix.M13 + vector.W * matrix.M14;
            var y = vector.X * matrix.M21 + vector.Y * matrix.M22 + vector.Z * matrix.M23 + vector.W * matrix.M24;
            var z = vector.X * matrix.M31 + vector.Y * matrix.M32 + vector.Z * matrix.M33 + vector.W * matrix.M34;
            var w = vector.X * matrix.M41 + vector.Y * matrix.M42 + vector.Z * matrix.M43 + vector.W * matrix.M44;

            return new Vector4(x, y, z, w);

        }

        public static Vector3 ToVector3(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}
