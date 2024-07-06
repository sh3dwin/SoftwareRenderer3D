using SoftwareRenderer3D.FrameBuffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System.Drawing;
using SoftwareRenderer3D.Utils;

namespace SoftwareRenderer3D.Rasterizers
{
    public static class ScanLineRasterizer
    {
        public static void ScanLineTriangle(IFrameBuffer frameBuffer, Vector3 v0, Vector3 v1, Vector3 v2, float diffuse)
        {
            var (p0, p1, p2) = SortIndices(v0, v1, v2);
            if (p0 == p1 || p1 == p2 || p2 == p0)
                return;

            var yStart = (int)System.Math.Max(p0.Y, 0);
            var yEnd = (int)System.Math.Min(p2.Y, frameBuffer.GetSize().Height - 1);

            // Out if clipped
            if (yStart > yEnd)
                return;

            var yMiddle = p1.Y.Clamp(yStart, yEnd);

            if (HaveClockwiseOrientation(p0, p1, p2))
            {
                // P0
                //   P1
                // P2
                ScanLineHalfTriangleBottomFlat(frameBuffer, yStart, (int)yMiddle - 1, p0, p1, p2, diffuse);
                ScanLineHalfTriangleTopFlat(frameBuffer, (int)yMiddle, yEnd, p2, p1, p0, diffuse);
            }
            else
            {
                //   P0
                // P1 
                //   P2

                ScanLineHalfTriangleBottomFlat(frameBuffer, yStart, (int)yMiddle - 1, p0, p2, p1, diffuse);
                ScanLineHalfTriangleTopFlat(frameBuffer, (int)yMiddle, yEnd, p2, p0, p1, diffuse);
            }
        }

        //            P0
        //          .....
        //       ..........
        //   .................P1
        // P2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ScanLineHalfTriangleBottomFlat(IFrameBuffer frameBuffer, int yStart, int yEnd,
            Vector3 anchor, Vector3 vRight, Vector3 vLeft, float diffuse)
        {
            var deltaY1 = System.Math.Abs(vLeft.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vLeft.Y - anchor.Y);
            var deltaY2 = System.Math.Abs(vRight.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vRight.Y - anchor.Y);

            for (var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - anchor.Y) * deltaY1).Clamp();
                var gradient2 = ((vRight.Y - y) * deltaY2).Clamp();

                var start = Vector3.Lerp(anchor, vLeft, gradient1);
                var end = Vector3.Lerp(vRight, anchor, gradient2);

                if (start.X >= end.X)
                    continue;

                start.Y = y;
                end.Y = y;

                ScanSingleLine(frameBuffer, start, end, diffuse);
            }
        }

        // P2
        //   .................P1
        //       ..........
        //          .....
        //            P0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ScanLineHalfTriangleTopFlat(IFrameBuffer frameBuffer, int yStart, int yEnd,
            Vector3 anchor, Vector3 vRight, Vector3 vLeft, float diffuse)
        {
            var deltaY1 = System.Math.Abs(vLeft.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vLeft.Y - anchor.Y);
            var deltaY2 = System.Math.Abs(vRight.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vRight.Y - anchor.Y);

            for (var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((vLeft.Y - y) * deltaY1).Clamp();
                var gradient2 = ((vRight.Y - y) * deltaY2).Clamp();

                var start = Vector3.Lerp(vLeft, anchor, gradient1);
                var end = Vector3.Lerp(vRight, anchor, gradient2);

                if (start.X >= end.X)
                    continue;

                start.Y = y;
                end.Y = y;

                ScanSingleLine(frameBuffer, start, end, diffuse);
            }
        }

        /// <summary>
        /// Scan line on the x direction
        /// </summary>
        /// <param name="start">Scan line start</param>
        /// <param name="end">Scan line end</param>
        /// <param name="faId">Facet id</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ScanSingleLine(IFrameBuffer frameBuffer, Vector3 start, Vector3 end, float diffuse)
        {
            var minX = System.Math.Max(start.X, 0);
            var maxX = System.Math.Min(end.X, frameBuffer.GetSize().Width);

            var deltaX = 1 / (end.X - start.X);

            for (var x = minX; x < maxX; x++)
            {
                var gradient = (x - start.X) * deltaX;
                var point = Vector3.Lerp(start, end, gradient);
                var xInt = (int)x;
                var yInt = (int)point.Y;

                var opacity = Globals.Opacity.Clamp(0, 255);
                var color = Color.FromArgb((int)(opacity * 255), (int)(255 * diffuse), (int)(255 * diffuse), (int)(255 * diffuse));

                frameBuffer.ColorPixel(xInt, yInt, point.Z, color);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HaveClockwiseOrientation(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return Cross2D(p0, p1, p2) > 0;
        }

        // https://www.geeksforgeeks.org/orientation-3-ordered-points/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross2D(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return (p1.X - p0.X) * (p2.Y - p1.Y) - (p1.Y - p0.Y) * (p2.X - p1.X);
        }

        public static (Vector3 i0, Vector3 i1, Vector3 i2) SortIndices(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var c0 = p0.Y;
            var c1 = p1.Y;
            var c2 = p2.Y;

            if (c0 < c1)
            {
                if (c2 < c0)
                    return (p2, p0, p1);
                if (c1 < c2)
                    return (p0, p1, p2);
                return (p0, p2, p1);
            }

            if (c2 < c1)
                return (p2, p1, p0);
            if (c0 < c2)
                return (p1, p0, p2);
            return (p1, p2, p0);

        }
    }
}
