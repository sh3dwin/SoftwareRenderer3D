using SoftwareRenderer3D.FrameBuffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System.Drawing;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System.Collections.Generic;

namespace SoftwareRenderer3D.Rasterizers
{
    public static class SubsurfaceScatteringScanLineRasterizer
    {
        public static void ScanLineTriangle(IFrameBuffer frameBuffer, IVertex v0, IVertex v1, IVertex v2, List<Vector3> lightSources)
        {
            var (sortedV0, sortedV1, sortedV2) = RenderUtils.SortIndices(v0, v1, v2);
            if (sortedV0 == sortedV1 || sortedV1 == sortedV2 || sortedV2 == sortedV0)
                return;

            var yStart = (int)System.Math.Max(sortedV0.ScreenPosition.Y, 0);
            var yEnd = (int)System.Math.Min(sortedV2.ScreenPosition.Y, frameBuffer.GetSize().Height - 1);

            // Out if clipped
            if (yStart > yEnd)
                return;

            var yMiddle = sortedV1.ScreenPosition.Y.Clamp(yStart, yEnd);

            if (RenderUtils.HaveClockwiseOrientation(sortedV0.ScreenPosition, sortedV1.ScreenPosition, sortedV2.ScreenPosition))
            {
                // P0
                //   P1
                // P2
                ScanLineHalfTriangleBottomFlat(frameBuffer, yStart, (int)yMiddle - 1, sortedV0, sortedV1, sortedV2, lightSources);
                ScanLineHalfTriangleTopFlat(frameBuffer, (int)yMiddle, yEnd, sortedV2, sortedV1, sortedV0, lightSources);
            }
            else
            {
                //   P0
                // P1 
                //   P2

                ScanLineHalfTriangleBottomFlat(frameBuffer, yStart, (int)yMiddle - 1, sortedV0, sortedV2, sortedV1, lightSources);
                ScanLineHalfTriangleTopFlat(frameBuffer, (int)yMiddle, yEnd, sortedV2, sortedV0, sortedV1, lightSources);
            }
        }

        //            P0
        //          .....
        //       ..........
        //   .................P1
        // P2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ScanLineHalfTriangleBottomFlat(IFrameBuffer frameBuffer, int yStart, int yEnd,
            IVertex anchor, IVertex vRight, IVertex vLeft, List<Vector3> lightSources)
        {
            var deltaY1 = System.Math.Abs(vLeft.ScreenPosition.Y - anchor.ScreenPosition.Y) < float.Epsilon
                ? 1f
                : 1 / (vLeft.ScreenPosition.Y - anchor.ScreenPosition.Y);
            var deltaY2 = System.Math.Abs(vRight.ScreenPosition.Y - anchor.ScreenPosition.Y) < float.Epsilon
                ? 1f
                : 1 / (vRight.ScreenPosition.Y - anchor.ScreenPosition.Y);

            for (var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - anchor.ScreenPosition.Y) * deltaY1).Clamp();
                var gradient2 = ((vRight.ScreenPosition.Y - y) * deltaY2).Clamp();

                var start = Vector3.Lerp(anchor.ScreenPosition, vLeft.ScreenPosition, gradient1);
                var end = Vector3.Lerp(vRight.ScreenPosition, anchor.ScreenPosition, gradient2);

                if (start.X >= end.X)
                    continue;

                start.Y = y;
                end.Y = y;

                ScanSingleLine(frameBuffer, start, end, anchor, vLeft, vRight, lightSources);
            }
        }

        // P2
        //   .................P1
        //       ..........
        //          .....
        //            P0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ScanLineHalfTriangleTopFlat(IFrameBuffer frameBuffer, int yStart, int yEnd,
            IVertex anchor, IVertex vRight, IVertex vLeft, List<Vector3> lightSources)
        {
            var deltaY1 = System.Math.Abs(vLeft.ScreenPosition.Y - anchor.ScreenPosition.Y) < float.Epsilon
                ? 1f
                : 1 / (vLeft.ScreenPosition.Y - anchor.ScreenPosition.Y);
            var deltaY2 = System.Math.Abs(vRight.ScreenPosition.Y - anchor.ScreenPosition.Y) < float.Epsilon
                ? 1f
                : 1 / (vRight.ScreenPosition.Y - anchor.ScreenPosition.Y);

            for (var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((vLeft.ScreenPosition.Y - y) * deltaY1).Clamp();
                var gradient2 = ((vRight.ScreenPosition.Y - y) * deltaY2).Clamp();

                var start = Vector3.Lerp(vLeft.ScreenPosition, anchor.ScreenPosition, gradient1);
                var end = Vector3.Lerp(vRight.ScreenPosition, anchor.ScreenPosition, gradient2);

                if (start.X >= end.X)
                    continue;

                start.Y = y;
                end.Y = y;

                ScanSingleLine(frameBuffer, start, end, anchor, vRight, vLeft, lightSources);
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
            IVertex v0, IVertex v1, IVertex v2,
            List<Vector3> lightSources)
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

                var screenPoint = new Vector3(xInt, yInt, point.Z);
                var barycentric = Barycentric.CalculateBarycentricCoordinates(screenPoint.XY(), v0.ScreenPosition.XY(), v1.ScreenPosition.XY(), v2.ScreenPosition.XY());

                var diffuse = 0.0;

                foreach (var lightSource in lightSources)
                {
                    var interpolatedNormal = (v0.Normal * barycentric.X + v1.Normal * barycentric.Y + v2.Normal * barycentric.Z).Normalize();
                    var worldPosition = v0.WorldPoint * barycentric.X + v1.WorldPoint * barycentric.Y + v2.WorldPoint * barycentric.Z;
                    var lightDirection = (worldPosition - lightSource).Normalize();

                    diffuse += (-Vector3.Dot(interpolatedNormal, lightDirection)).Clamp();
                }

                diffuse = diffuse.Clamp(0, 1);

                var opacity = Globals.NormalizedOpacity.Clamp(0, 255);
                var color = Color.FromArgb((int)(opacity * 255), (int)(255 * diffuse), (int)(255 * diffuse), (int)(255 * diffuse));

                frameBuffer.SetPixelColor(xInt, yInt, point.Z, color);
            }
        }
    }
}
