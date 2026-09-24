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
using System.Buffers;
using System.Xml.XPath;

namespace SoftwareRenderer3D.Rasterizers
{
    public static class ScanLineRasterizer
    {
        public static List<IFragment> Rasterize(Mesh<IVertex> mesh, int width, int height, IReadOnlyList<int> facetIds)
        {
            var fragments = new ConcurrentBag<IFragment>();

            var blockSize = facetIds.Count / Constants.NumberOfThreads;
            Parallel.For(0, Constants.NumberOfThreads, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, threadId =>
            {
                var startIndex = threadId * blockSize;
                var endIndex = System.Math.Min(startIndex + blockSize, mesh.FacetCount);

                var blockFragments = new List<IFragment>();
                for (var i = startIndex; i < endIndex; i++)
                {
                    var facet = mesh.GetFacet(facetIds[i]);

                    var v0 = mesh.GetVertex(facet.V0);
                    var v1 = mesh.GetVertex(facet.V1);
                    var v2 = mesh.GetVertex(facet.V2);

                    var normal = facet.Normal;

                    if (RenderUtils.IsTriangleInFrustum(width, height, v0.ScreenPosition, v1.ScreenPosition, v2.ScreenPosition))
                        foreach (var fragment in RasterizeTriangle(width, height, v0, v1, v2))
                            blockFragments.Add(fragment);
                }

                for (var i = 0; i < blockFragments.Count; i++)
                    fragments.Add(blockFragments[i]);
            });

            var result = new List<IFragment>(fragments.Count);
            foreach (var fragment in fragments.ToList())
            {
                if (fragment != null)
                    result.Add(fragment);
            }

            return result;
        }
        private static IReadOnlyList<IFragment> RasterizeTriangle(int width, int height, IVertex v0, IVertex v1, IVertex v2)
        {
            var result = new List<IFragment>();

            var (sortedV0, sortedV1, sortedV2) = RenderUtils.SortIndices(v0, v1, v2);

            if (sortedV0 == sortedV1 || sortedV1 == sortedV2 || sortedV2 == sortedV0)
                return null;

            var yStart = (int)System.Math.Max(sortedV0.ScreenPosition.Y, 0);
            var yEnd = (int)System.Math.Min(sortedV2.ScreenPosition.Y, height - 1);

            // Out if clipped
            if (yStart > yEnd)
                return null;

            var yMiddle = sortedV1.ScreenPosition.Y.Clamp(yStart, yEnd);

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
        private static IReadOnlyList<IFragment> ScanLineHalfTriangleBottomFlat(int width, int height, int yStart, int yEnd,
            in IVertex anchor, in IVertex vRight, in IVertex vLeft)
        {
            Vector3 anchorScreenPos = anchor.ScreenPosition;
            Vector3 vLeftScreenPos = vLeft.ScreenPosition;
            Vector3 vRightScreenPos = vRight.ScreenPosition;

            var deltaY1 = System.Math.Abs(vLeftScreenPos.Y - anchorScreenPos.Y) < float.Epsilon
                ? 1f
                : 1 / (vLeftScreenPos.Y - anchorScreenPos.Y);
            var deltaY2 = System.Math.Abs(vRightScreenPos.Y - anchorScreenPos.Y) < float.Epsilon
                ? 1f
                : 1 / (vRightScreenPos.Y - anchorScreenPos.Y);

            var result = new List<IFragment>();
            for (var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - anchorScreenPos.Y) * deltaY1).Clamp();
                var gradient2 = ((vRightScreenPos.Y - y) * deltaY2).Clamp();

                var start = Vector3.Lerp(anchorScreenPos, vLeftScreenPos, gradient1);
                var end = Vector3.Lerp(vRightScreenPos, anchorScreenPos, gradient2);

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
        private static IReadOnlyList<IFragment> ScanLineHalfTriangleTopFlat(int width, int height, int yStart, int yEnd,
            in IVertex anchor, in IVertex vRight, in IVertex vLeft)
        {
            Vector3 anchorScreenPos = anchor.ScreenPosition;
            Vector3 vLeftScreenPos = vLeft.ScreenPosition;
            Vector3 vRightScreenPos = vRight.ScreenPosition;

            var deltaY1 = System.Math.Abs(vLeftScreenPos.Y - anchorScreenPos.Y) < float.Epsilon
                ? 1f
                : 1 / (vLeftScreenPos.Y - anchorScreenPos.Y);
            var deltaY2 = System.Math.Abs(vRightScreenPos.Y - anchorScreenPos.Y) < float.Epsilon
                ? 1f
                : 1 / (vRightScreenPos.Y - anchorScreenPos.Y);

            var result = new List<IFragment>();
            for (var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((vLeftScreenPos.Y - y) * deltaY1).Clamp();
                var gradient2 = ((vRightScreenPos.Y - y) * deltaY2).Clamp();

                var start = Vector3.Lerp(vLeftScreenPos, anchorScreenPos, gradient1);
                var end = Vector3.Lerp(vRightScreenPos, anchorScreenPos, gradient2);

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
        private static IReadOnlyList<IFragment> ScanSingleLine(int width, int height, in Vector3 start, in Vector3 end,
            IVertex v0, IVertex v1, IVertex v2)
        {
            var minX = System.Math.Max(start.X, 0);
            var maxX = System.Math.Min(end.X, width);

            var deltaX = 1 / (end.X - start.X);

            var result = new List<IFragment>();
            for (var x = minX; x < maxX; x++)
            {
                var gradient = (x - start.X) * deltaX;
                var point = Vector3.Lerp(start, end, gradient);
                var xInt = (int)x;
                var yInt = (int)point.Y;

                var screenPoint = new Vector3(xInt, yInt, point.Z);
                var barycentric = Barycentric.CalculateBarycentricCoordinatesVector3(screenPoint, v0.ScreenPosition, v1.ScreenPosition, v2.ScreenPosition);

                var fragment = new SimpleFragment(screenPoint.XY(), point.Z, barycentric, v0, v1, v2);

                result.Add(fragment);
            }

            return result;
        }
    }
}
