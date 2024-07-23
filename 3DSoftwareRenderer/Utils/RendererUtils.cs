using System.Numerics;

namespace SoftwareRenderer3D.Utils
{
    public static class RendererUtils
    {
        public static bool InInclusiveLowerExclusiveUpper(float x, int lower, int upper)
        {
            return x >= lower && x < upper;
        }

        public static bool IsTriangleInFrustum(int width, int height, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return InInclusiveLowerExclusiveUpper(v0.X, 0, width) && InInclusiveLowerExclusiveUpper(v0.Y, 0, height) ||
                InInclusiveLowerExclusiveUpper(v1.X, 0, width) && InInclusiveLowerExclusiveUpper(v1.Y, 0, height) ||
                InInclusiveLowerExclusiveUpper(v2.X, 0, width) && InInclusiveLowerExclusiveUpper(v2.Y, 0, height);
        }
    }
}
