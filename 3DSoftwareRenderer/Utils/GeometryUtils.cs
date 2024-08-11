using System.Collections.Generic;
using System.Numerics;

namespace SoftwareRenderer3D.Utils
{
    public static class GeometryUtils
    {
        public static (Vector3 bottomLeftFront, Vector3 topRightBack) BoundingBox(this IEnumerable<Vector3> points)
        {
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            var minZ = float.MaxValue;
            var maxZ = float.MinValue;

            foreach(var point in points)
            {
                if(point.X < minX) minX = point.X;
                if(point.Y < minY) minY = point.Y;
                if(point.Z < minZ) minZ = point.Z;
                if(point.X > maxX) maxX = point.X;
                if(point.Y > maxY) maxY = point.Y;
                if(point.Z > maxZ) maxZ = point.Z;
            }

            var bottomLeftFront = new Vector3(minX, minY, minZ);
            var topRightBack = new Vector3(maxX, maxY, minZ);

            return (bottomLeftFront, topRightBack);
        }
    }
}
