using System.Numerics;
using System.Runtime.CompilerServices;
using SoftwareRenderer3D.Utils.GeneralUtils;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System.Collections.Generic;
using SoftwareRenderer3D.DataStructures.Fragment;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

namespace SoftwareRenderer3D.Rasterizers
{
    public static class ScanLineRasterizer
    {
        public static List<SimpleFragment> Rasterize(Mesh<IVertex> mesh, int width, int height, IEnumerable<int> facetIds)
        {
            var fragments = new ConcurrentBag<SimpleFragment>();
            Parallel.ForEach(facetIds, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, facetId =>
            {
                var facet = mesh.GetFacet(facetId);

                var v0 = mesh.GetVertex(facet.V0);
                var v1 = mesh.GetVertex(facet.V1);
                var v2 = mesh.GetVertex(facet.V2);

                var normal = facet.Normal;

                if (RenderUtils.IsTriangleInFrustum(width, height, v0.ScreenPosition, v1.ScreenPosition, v2.ScreenPosition))
                    foreach(var fragment in RasterizeTriangle(width, height, v0, v1, v2))
                    fragments.Add(fragment);
            });
            return fragments.ToList();
        }
        private static List<SimpleFragment> RasterizeTriangle(int width, int height, IVertex v0, IVertex v1, IVertex v2)
        {
            var (sortedV0, sortedV1, sortedV2) = RenderUtils.SortIndices(v0, v1, v2);
            if (sortedV0 == sortedV1 || sortedV1 == sortedV2 || sortedV2 == sortedV0)
                return null;

            var yStart = (int)System.Math.Max(sortedV0.ScreenPosition.Y, 0);
            var yEnd = (int)System.Math.Min(sortedV2.ScreenPosition.Y, height - 1);

            // Out if clipped
            if (yStart > yEnd)
                return null;

            var yMiddle = sortedV1.ScreenPosition.Y.Clamp(yStart, yEnd);

            var result = new List<SimpleFragment>();
            if (RenderUtils.HaveClockwiseOrientation(sortedV0.ScreenPosition, sortedV1.ScreenPosition, sortedV2.ScreenPosition))
            {
                // P0
                //   P1
                // P2
                result.AddRange(ScanLineHalfTriangleBottomFlat(width, height, yStart, (int)yMiddle - 1, sortedV0, sortedV1, sortedV2));
                result.AddRange(ScanLineHalfTriangleTopFlat(width, height, (int)yMiddle, yEnd, sortedV2, sortedV1, sortedV0));
            }
            else
            {
                //   P0
                // P1 
                //   P2

                result.AddRange(ScanLineHalfTriangleBottomFlat(width, height, yStart, (int)yMiddle - 1, sortedV0, sortedV2, sortedV1));
                result.AddRange(ScanLineHalfTriangleTopFlat(width, height, (int)yMiddle, yEnd, sortedV2, sortedV0, sortedV1));
            }

            return result;
        }

        //            P0
        //          .....
        //       ..........
        //   .................P1
        // P2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<SimpleFragment> ScanLineHalfTriangleBottomFlat(int width, int height, int yStart, int yEnd,
            IVertex anchor, IVertex vRight, IVertex vLeft)
        {
            var deltaY1 = System.Math.Abs(vLeft.ScreenPosition.Y - anchor.ScreenPosition.Y) < float.Epsilon
                ? 1f
                : 1 / (vLeft.ScreenPosition.Y - anchor.ScreenPosition.Y);
            var deltaY2 = System.Math.Abs(vRight.ScreenPosition.Y - anchor.ScreenPosition.Y) < float.Epsilon
                ? 1f
                : 1 / (vRight.ScreenPosition.Y - anchor.ScreenPosition.Y);

            var result = new List<SimpleFragment>();
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

                result.AddRange(ScanSingleLine(width, height, start, end, anchor, vLeft, vRight));
            }

            return result;
        }

        // P2
        //   .................P1
        //       ..........
        //          .....
        //            P0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<SimpleFragment> ScanLineHalfTriangleTopFlat(int width, int height, int yStart, int yEnd,
            IVertex anchor, IVertex vRight, IVertex vLeft)
        {
            var deltaY1 = System.Math.Abs(vLeft.ScreenPosition.Y - anchor.ScreenPosition.Y) < float.Epsilon
                ? 1f
                : 1 / (vLeft.ScreenPosition.Y - anchor.ScreenPosition.Y);
            var deltaY2 = System.Math.Abs(vRight.ScreenPosition.Y - anchor.ScreenPosition.Y) < float.Epsilon
                ? 1f
                : 1 / (vRight.ScreenPosition.Y - anchor.ScreenPosition.Y);

            var result = new List<SimpleFragment>();
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

                result.AddRange(ScanSingleLine(width, height, start, end, anchor, vRight, vLeft));
            }

            return result;
        }

        /// <summary>
        /// Scan line on the x direction
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<SimpleFragment> ScanSingleLine(int width, int height, Vector3 start, Vector3 end,
            IVertex v0, IVertex v1, IVertex v2)
        {
            var minX = System.Math.Max(start.X, 0);
            var maxX = System.Math.Min(end.X, width);

            var deltaX = 1 / (end.X - start.X);

            var result = new List<SimpleFragment>();
            for (var x = minX; x < maxX; x++)
            {
                var gradient = (x - start.X) * deltaX;
                var point = Vector3.Lerp(start, end, gradient);
                var xInt = (int)x;
                var yInt = (int)point.Y;

                var screenPoint = new Vector3(xInt, yInt, point.Z);
                var barycentric = Barycentric.CalculateBarycentricCoordinates(screenPoint.XY(), v0.ScreenPosition.XY(), v1.ScreenPosition.XY(), v2.ScreenPosition.XY());

                var fragment = new SimpleFragment(screenPoint.XY(), point.Z, barycentric, v0, v1, v2);

                result.Add(fragment);
            }

            return result;
        }
    }
}
