using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Numerics;
using System.Windows;

namespace SoftwareRenderer3D.Maths
{
    public class Quaternion
    {
        private const double Epsilon = 1e-5;

        private double a;
        private double b;
        private double c;
        private double d;

        private Vector3 _imaginary;

        public double Real => a;
        public Vector3 Imaginary => _imaginary;

        public Quaternion(double real, double x, double y, double z)
        {
            a = real;
            b = x;
            c = y;
            d = z;

            if (Math.Abs(b) < Epsilon)
                b = 0.0;
            if (Math.Abs(c) < Epsilon)
                c = 0.0;
            if (Math.Abs(d) < Epsilon)
                d = 0.0;

            _imaginary = new Vector3((float)b, (float)c, (float)d);
        }

        public Quaternion(double real, Vector3 imaginary)
        {
            a = real;
            b = imaginary.X;
            c = imaginary.Y;
            d = imaginary.Z;

            if (Math.Abs(b) < Epsilon)
                b = 0.0;
            if (Math.Abs(c) < Epsilon)
                c = 0.0;
            if (Math.Abs(d) < Epsilon)
                d = 0.0;

            _imaginary = new Vector3((float)b, (float)c, (float)d);
        }

        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            var real = a.a * b.a - a.b * b.b - a.c * b.c - a.d * b.d;
            var i = a.a * b.b + a.b * b.a + a.c * b.d - a.d * b.c;
            var j = a.a * b.c - a.b * b.d + a.c * b.a + a.d * b.b;
            var k = a.a * b.d + a.b * b.c - a.c * b.b + a.d * b.a;

            return new Quaternion(real, i, j, k);
        }

        public static Quaternion operator +(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        public static Quaternion Identity = new Quaternion(1, 0, 0, 0);

        public static Quaternion FromBetweenVectors(Vector3 a, Vector3 b)
        {
            var angle = Math.Acos(Vector3.Dot(a.Normalize(), b.Normalize())).Clamp(0, Math.PI);
            var axis = Vector3.Cross(a.Normalize(), b.Normalize());

            return new Quaternion(Math.Cos(angle / 2.0), (float)Math.Sin(angle / 2.0) * axis.Normalize());
        }

        public Matrix4x4 RotationMatrix()
        {
            return new Matrix4x4(
                (float)(2 * (a * a + b * b) - 1), (float)(2 * (b * c - a * d)), (float)(2 * (b * d + a * c)), 0,
                (float)(2 * (b * c + a * d)), (float)(2 * (a * a + c * c) - 1), (float)(2 * (c * d - a * b)), 0,
                (float)(2 * (b * d - a * c)), (float)(2 * (c * d + a * b)), (float)(2 * (a * a + d * d) - 1), 0,
                0, 0, 0, 1);
        }

    }
}
