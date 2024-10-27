using System.Numerics;

namespace SoftwareRenderer3D.Utils.GeneralUtils
{
    public static class MathUtils
    {
        public static double Clamp(this double x, double min = 0, double max = 1)
        {
            return System.Math.Min(max, System.Math.Max(x, min));
        }

        public static float Clamp(this float x, float min = 0, float max = 1)
        {
            return System.Math.Min(max, System.Math.Max(x, min));
        }

        public static byte Clamp(this byte x, byte min = 0, byte max = 255)
        {
            return System.Math.Min(max, System.Math.Max(x, min));
        }

        public static int Clamp(this int x, int min = 0, int max = int.MaxValue)
        {
            return System.Math.Min(max, System.Math.Max(x, min));
        }

        public static Vector3 Normalize(this Vector3 vector)
        {
            return Vector3.Normalize(vector);
        }

        public static Matrix4x4 RotateAroundAxis(float angle, Vector3 axis)
        {
            return new Matrix4x4(
                // First row
                (float)(System.Math.Pow(axis.X, 2) + (1 - System.Math.Pow(axis.X, 2)) * System.Math.Cos(angle)),
                (float)(axis.X * axis.Y * (1 - System.Math.Cos(angle)) - axis.Z * System.Math.Sin(angle)),
                (float)(axis.Z * axis.X * (1 - System.Math.Cos(angle)) + axis.Y * System.Math.Sin(angle)),
                0,

                // Second Row
                (float)(axis.X * axis.Y * (1 - System.Math.Cos(angle)) + axis.Z * System.Math.Sin(angle)),
                (float)(System.Math.Pow(axis.Y, 2) + (1 - System.Math.Pow(axis.Y, 2)) * System.Math.Cos(angle)),
                (float)(axis.Z * axis.Y * (1 - System.Math.Cos(angle)) - axis.X * System.Math.Sin(angle)),
                0,

                // Third Row
                (float)(axis.Z * axis.X * (1 - System.Math.Cos(angle)) - axis.Y * System.Math.Sin(angle)),
                (float)(axis.Z * axis.Y * (1 - System.Math.Cos(angle)) + axis.X * System.Math.Sin(angle)),
                (float)(System.Math.Pow(axis.Z, 2) + (1.0 - System.Math.Pow(axis.Z, 2)) * System.Math.Cos(angle)),
                0,

                // Fourth row
                0,
                0,
                0,
                1
                );
        }

        public static Vector4 TransformHomogeneous(this Vector3 vector, Matrix4x4 matrix)
        {
            var homogeneousVector = new Vector4(vector.X, vector.Y, vector.Z, 1);

            var x = homogeneousVector.X * matrix.M11 + homogeneousVector.Y * matrix.M12 + homogeneousVector.Z * matrix.M13 + homogeneousVector.W * matrix.M14;
            var y = homogeneousVector.X * matrix.M21 + homogeneousVector.Y * matrix.M22 + homogeneousVector.Z * matrix.M23 + homogeneousVector.W * matrix.M24;
            var z = homogeneousVector.X * matrix.M31 + homogeneousVector.Y * matrix.M32 + homogeneousVector.Z * matrix.M33 + homogeneousVector.W * matrix.M34;
            var w = homogeneousVector.X * matrix.M41 + homogeneousVector.Y * matrix.M42 + homogeneousVector.Z * matrix.M43 + homogeneousVector.W * matrix.M44;

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

        public static Vector3 ToNDC(this Vector3 vector, double width, double height)
        {
            var ndcX = (vector.X - width / 2) / width / 2;
            var ndcY = (- vector.Y - height / 2) / height / 2;
            var ndcZ = vector.Z;

            return new Vector3((float)ndcX, (float)ndcY, (float)ndcZ);
        }
    }
}
