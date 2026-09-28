using BenchmarkDotNet.Attributes;
using SoftwareRenderer3D.DataStructures.Fragment;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.Rasterizers
{
    public class ScanLineRasterizer
    {
        public List<IFragment> Rasterize(Mesh<IVertex> mesh, int width, int height, IReadOnlyList<int> facetIds)
        {
            var fragments = new List<IFragment>();

            var blockSize = facetIds.Count / Constants.NumberOfThreads;
            Parallel.For(0, Constants.NumberOfThreads, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads },
                () => new List<IFragment>(512),
                (threadId, loop, localFragments) =>
            {
                var startIndex = threadId * blockSize;
                var endIndex =
                Math.Min(startIndex + blockSize, mesh.FacetCount);

                for (var i = startIndex; i < endIndex; i++)
                {
                    var facet = mesh.GetFacet(facetIds[i]);

                    var v0 = mesh.GetVertex(facet.V0);
                    var v1 = mesh.GetVertex(facet.V1);
                    var v2 = mesh.GetVertex(facet.V2);

                    var normal = facet.Normal;

                    if (RenderUtils.IsTriangleInFrustum(width, height, v0.ScreenPosition, v1.ScreenPosition, v2.ScreenPosition))
                        foreach (var fragment in RasterizeTriangle(width, height, v0, v1, v2))
                            if(fragment is not null)
                                localFragments.Add(fragment);
                }

                return localFragments;
            }, localFragments =>
            {
                lock (fragments)
                {
                    fragments.AddRange(localFragments.Where(f => f is not null));
                }
            });

            return fragments;
        }
        private IReadOnlyList<IFragment> RasterizeTriangle(int width, int height, IVertex v0, IVertex v1, IVertex v2)
        {
            var result = new List<IFragment>();

            var (sortedV0, sortedV1, sortedV2) = RenderUtils.SortIndices(v0, v1, v2);

            if (sortedV0 == sortedV1 || sortedV1 == sortedV2 || sortedV2 == sortedV0)
                return null;

            var yStart = (int)Math.Max(sortedV0.ScreenPosition.Y, 0);
            var yEnd = (int)Math.Min(sortedV2.ScreenPosition.Y, height - 1);

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
        private IReadOnlyList<IFragment> ScanLineHalfTriangleBottomFlat(int width, int height, int yStart, int yEnd,
            in IVertex anchor, in IVertex vRight, in IVertex vLeft)
        {
            Vector3 anchorScreenPos = anchor.ScreenPosition;
            Vector3 vLeftScreenPos = vLeft.ScreenPosition;
            Vector3 vRightScreenPos = vRight.ScreenPosition;

            var deltaY1 = Math.Abs(vLeftScreenPos.Y - anchorScreenPos.Y) < float.Epsilon
                ? 1f
                : 1 / (vLeftScreenPos.Y - anchorScreenPos.Y);
            var deltaY2 = Math.Abs(vRightScreenPos.Y - anchorScreenPos.Y) < float.Epsilon
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
        private IReadOnlyList<IFragment> ScanLineHalfTriangleTopFlat(int width, int height, int yStart, int yEnd,
            in IVertex anchor, in IVertex vRight, in IVertex vLeft)
        {
            Vector3 anchorScreenPos = anchor.ScreenPosition;
            Vector3 vLeftScreenPos = vLeft.ScreenPosition;
            Vector3 vRightScreenPos = vRight.ScreenPosition;

            var deltaY1 = Math.Abs(vLeftScreenPos.Y - anchorScreenPos.Y) < float.Epsilon
                ? 1f
                : 1 / (vLeftScreenPos.Y - anchorScreenPos.Y);
            var deltaY2 = Math.Abs(vRightScreenPos.Y - anchorScreenPos.Y) < float.Epsilon
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
        private IReadOnlyList<IFragment> ScanSingleLine(int width, int height, in Vector3 start, in Vector3 end,
            IVertex v0, IVertex v1, IVertex v2)
        {
            var minX = Math.Clamp(start.X, 0, width);
            var maxX = Math.Clamp(end.X, 0, width);

            var dx = maxX - minX;
            var invDX = 1 / dx;

            var result = new IFragment[((int)dx + 1)];
            for (var x = minX; x < maxX; x++)
            {
                var point = Vector3.Lerp(start, end, (x - start.X) * invDX);

                var screenPoint = new Vector3((int)x, (int)point.Y, point.Z);
                var barycentric = Barycentric.CalculateBarycentricCoordinatesVector3(screenPoint, v0.ScreenPosition, v1.ScreenPosition, v2.ScreenPosition);

                var fragment = new SimpleFragment(screenPoint.XY(), point.Z, barycentric, v0, v1, v2);

                result[(int)(x - minX)] = fragment;
            }
            return result;
        }



        #region Benchmark

        public IEnumerable<(IVertex, IVertex, IVertex)> GetBenchmarkVertices(Random random)
        {
            for (var i = 0; i < N; i++)
            {
                // Random v0, v1, v2
                var vertices = new List<StandardVertex>(3);
                for (var j = 0; j < 3; j++)
                    vertices.Add(new StandardVertex(new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble())));

                yield return (vertices[0], vertices[1], vertices[2]);
            }
        }

        public List<(IVertex, IVertex, IVertex)> _benchmarkVertices;

        public List<Vector3> _startValues;

        public List<Vector3> _endValues;

        [Params(1_000, 10_000)]
        public int N;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random();
            _benchmarkVertices = GetBenchmarkVertices(random).ToList();

            _startValues = new List<Vector3>(N);
            for (var i = 0; i < N; i++)
                _startValues.Add(new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()));

            _endValues = new List<Vector3>(N);
            for (var i = 0; i < N; i++)
                _endValues.Add(new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()));
        }

        [Benchmark]
        public IReadOnlyList<IFragment> BenchmarkScanSingleLine()
        {
            IReadOnlyList<IFragment> result = default;

            for (int i = 0; i < _benchmarkVertices.Count; i++)
                result = ScanSingleLine(600, 800, _startValues[i], _endValues[i],
                    _benchmarkVertices[i].Item1, _benchmarkVertices[i].Item2, _benchmarkVertices[i].Item3);

            return result;
        }
        #endregion
    }
}
