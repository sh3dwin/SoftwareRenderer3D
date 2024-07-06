using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Numerics;

namespace SoftwareRenderer3D.Maths
{
    public static class LinearAlgebra
    {

        public static Vector3 RotateAroundAxis(this Vector3 vector, Vector3 axis, double theta)
        {
            var normalizedAxis = axis.Normalize();

            var vectorQuaternion = new Quaternion(0, vector);

            var q = new Quaternion(Math.Cos(theta / 2), normalizedAxis);

            return (q * vectorQuaternion * q.Conjugate()).Imaginary;
        }
    }
}
