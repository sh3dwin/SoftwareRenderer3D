using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.FrameBuffers;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftwareRenderer3D.Rasterizers
{
    public static class TexturedScanLineRasterizer
    {
        private static Texture _texture;

        public static void BindTexture(Texture texture)
        {
            if(texture != null)
                _texture = texture;
        }
        public static void UnbindTexture() {
            _texture = null;
        }
        public static void ScanLineTriangle(IFrameBuffer frameBuffer, Vector3 v0, Vector3 v1, Vector3 v2, TexturedVertex ve0, TexturedVertex ve1, TexturedVertex ve2, float diffuse)
        {
            var (p0, p1, p2, vertex0, vertex1, vertex2) = SortIndices(v0, v1, v2, ve0, ve1, ve2);
            if (p0 == p1 || p1 == p2 || p2 == p0)
                return;

            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, frameBuffer.GetSize().Height - 1);

            // Out if clipped
            if (yStart > yEnd)
                return;

            var yMiddle = p1.Y.Clamp(yStart, yEnd);

            if (HaveClockwiseOrientation(p0, p1, p2))
            {
                // P0
                //   P1
                // P2
                ScanLineHalfTriangleBottomFlat(frameBuffer, yStart, (int)yMiddle - 1, p0, p1, p2, vertex0, vertex1, vertex2, diffuse);
                ScanLineHalfTriangleTopFlat(frameBuffer, (int)yMiddle, yEnd, p2, p1, p0, vertex2, vertex1, vertex0, diffuse);
            }
            else
            {
                //   P0
                // P1 
                //   P2

                ScanLineHalfTriangleBottomFlat(frameBuffer, yStart, (int)yMiddle - 1, p0, p2, p1, vertex0, vertex2, vertex1, diffuse);
                ScanLineHalfTriangleTopFlat(frameBuffer, (int)yMiddle, yEnd, p2, p0, p1, vertex2, vertex0, vertex1, diffuse);
            }
        }

        //            P0
        //          .....
        //       ..........
        //   .................P1
        // P2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ScanLineHalfTriangleBottomFlat(IFrameBuffer frameBuffer, int yStart, int yEnd,
            Vector3 anchor, Vector3 vRight, Vector3 vLeft,
            TexturedVertex ve0, TexturedVertex ve1, TexturedVertex ve2, 
            float diffuse)
        {
            var deltaY1 = Math.Abs(vLeft.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vLeft.Y - anchor.Y);
            var deltaY2 = Math.Abs(vRight.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vRight.Y - anchor.Y);

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

                ScanSingleLine(frameBuffer, start, end, anchor, vRight, vLeft, ve0, ve1, ve2, diffuse);
            }
        }

        // P2
        //   .................P1
        //       ..........
        //          .....
        //            P0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ScanLineHalfTriangleTopFlat(IFrameBuffer frameBuffer, int yStart, int yEnd,
            Vector3 anchor, Vector3 vRight, Vector3 vLeft,
            TexturedVertex ve0, TexturedVertex ve1, TexturedVertex ve2, float diffuse)
        {
            var deltaY1 = Math.Abs(vLeft.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vLeft.Y - anchor.Y);
            var deltaY2 = Math.Abs(vRight.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vRight.Y - anchor.Y);

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

                ScanSingleLine(frameBuffer, start, end, anchor, vRight, vLeft, ve0, ve1, ve2, diffuse);
            }
        }

        /// <summary>
        /// Scan line on the x direction
        /// </summary>
        /// <param name="start">Scan line start</param>
        /// <param name="end">Scan line end</param>
        /// <param name="faId">Facet id</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ScanSingleLine(IFrameBuffer frameBuffer, Vector3 start, Vector3 end,
            Vector3 screenCoords0, Vector3 screenCoords1, Vector3 screenCoords2,
            TexturedVertex ve0, TexturedVertex ve1, TexturedVertex ve2, float diffuse)
        {
            var minX = Math.Max(start.X, 0);
            var maxX = Math.Min(end.X, frameBuffer.GetSize().Width);

            var deltaX = 1 / (end.X - start.X);

            for (var x = minX; x < maxX; x++)
            {
                var gradient = (x - start.X) * deltaX;
                var point = Vector3.Lerp(start, end, gradient);
                var xInt = (int)x;
                var yInt = (int)point.Y;

                var barycentric = Barycentric.CalculateBarycentricCoordinates(xInt, yInt, screenCoords0.XY(), screenCoords1.XY(), screenCoords2.XY());

                var u = MathUtils.Clamp(ve0.TextureCoordinates.X * barycentric.X + ve1.TextureCoordinates.X * barycentric.Y + ve2.TextureCoordinates.X * barycentric.Z);
                var v = MathUtils.Clamp(ve0.TextureCoordinates.Y * barycentric.X + ve1.TextureCoordinates.Y * barycentric.Y + ve2.TextureCoordinates.Y * barycentric.Z); 

                if(u == float.NaN || v == float.NaN)
                    continue;

                var color = _texture.GetTextureColor(u, v, Globals.TextureInterpolation);

                var opacity = Globals.Opacity.Clamp(0, 255);
                color = Color.FromArgb((int)(opacity * 255), color.R, color.G, color.B).Mult(diffuse);

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

        public static (Vector3 i0, Vector3 i1, Vector3 i2, TexturedVertex v0, TexturedVertex v1, TexturedVertex v2) SortIndices(Vector3 p0, Vector3 p1, Vector3 p2, TexturedVertex ve0, TexturedVertex ve1, TexturedVertex ve2)
        {
            var c0 = p0.Y;
            var c1 = p1.Y;
            var c2 = p2.Y;

            if (c0 < c1)
            {
                if (c2 < c0)
                    return (p2, p0, p1, ve2, ve0, ve1);
                if (c1 < c2)
                    return (p0, p1, p2, ve0, ve1, ve2);
                return (p0, p2, p1, ve0, ve2, ve1);
            }

            if (c2 < c1)
                return (p2, p1, p0, ve2, ve1, ve0);
            if (c0 < c2)
                return (p1, p0, p2, ve1, ve0, ve2);
            return (p1, p2, p0, ve1, ve2, ve0);

        }
    }
}
