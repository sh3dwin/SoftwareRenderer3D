using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.Utils.LinearAlgebraUtils
{
    public static class Vector3Extensions
    {
        public static Vector3 Transform(this Vector3 vector, Matrix4x4 transformation) {
            var vector4 = new Vector4(vector, 1);
            var result = new Vector4(
                vector4.X * transformation.M11 + vector4.Y * transformation.M12 + vector4.Z * transformation.M13 + vector4.W * transformation.M14,
                vector4.X * transformation.M21 + vector4.Y * transformation.M22 + vector4.Z * transformation.M23 + vector4.W * transformation.M24,
                vector4.X * transformation.M31 + vector4.Y * transformation.M32 + vector4.Z * transformation.M33 + vector4.W * transformation.M34,
                vector4.X * transformation.M41 + vector4.Y * transformation.M42 + vector4.Z * transformation.M43 + vector4.W * transformation.M44);

            return new Vector3(result.X, result.Y, result.Z);
        }
    }
}
