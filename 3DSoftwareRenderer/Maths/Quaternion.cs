using System.Numerics;

namespace SoftwareRenderer3D.Maths
{
    public class Quaternion
    {
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

            _imaginary = new Vector3((float)x, (float)y, (float)z);
        }

        public Quaternion(double real, Vector3 imaginary)
        {
            a = real;
            b = imaginary.X;
            c = imaginary.Y;
            d = imaginary.Z;

            _imaginary = imaginary;
        }

        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            var real = a.a * b.a - a.b * b.b - a.c * b.c - a.d * b.d;
            var i =    a.a * b.b + a.b * b.a + a.c * b.d - a.d * b.c;
            var j =    a.a * b.c - a.b * b.d + a.c * b.a + a.d * b.b;
            var k =    a.a * b.d + a.b * b.c - a.c * b.b + a.d * b.a;

            return new Quaternion(real, i, j, k);
        }

        public static Quaternion operator +(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

    }
}
