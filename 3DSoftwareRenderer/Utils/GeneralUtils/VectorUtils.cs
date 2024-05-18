using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
    }
}
