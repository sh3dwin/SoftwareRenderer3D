using g3;
using System.Numerics;

namespace SoftwareRenderer3D.Utils.GeneralUtils
{
    public static class VectorUtils
    {
        public static Vector2 XY(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        public static Vector2 XZ(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Z);
        }
        public static Vector2 YZ(this Vector3 vector)
        {
            return new Vector2(vector.Y, vector.Z);
        }

        public static Vector3f ToVector3f(this Vector3 vector) 
        {
            return new Vector3f(vector.X, vector.Y, vector.Z);    
        }

        public static Vector3d ToVector3d(this Vector3 vector)
        {
            return new Vector3d(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 Transform(this Matrix4x4 matrix, Vector3 vector)
        {
            return Vector3.Transform(vector, matrix);
        }
    }
}
