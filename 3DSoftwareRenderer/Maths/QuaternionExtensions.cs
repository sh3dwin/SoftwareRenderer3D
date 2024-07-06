namespace SoftwareRenderer3D.Maths
{
    public static class QuaternionExtensions
    {
        public static Quaternion Conjugate(this Quaternion quaternion)
        {
            return new Quaternion(quaternion.Real, -quaternion.Imaginary);
        }
    }
}
