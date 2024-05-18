using SoftwareRenderer3D.Camera;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.FrameBuffers;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.Renderers
{
    public static class SimpleRenderer
    {
        public static Bitmap Render(Mesh<IVertex> mesh, FrameBuffer frameBuffer, ArcBallCamera camera)
        {
            var width = frameBuffer.Width;
            var height = frameBuffer.Height;

            var viewMatrix = camera.ViewMatrix;
            var projectionMatrix = camera.ProjectionMatrix;

            Matrix4x4.Invert(mesh.ModelMatrix, out var modelMatrix);

            var lightSourceAt = new Vector3(0, 100, 1);

            var facets = mesh.GetFacets().Where((x, i) => 
            Vector3.Dot(
                (mesh.GetFacetMidpoint(i) - camera.Position).Normalize(), 
                x.Normal.Normalize()) <= 10.3);

            Parallel.ForEach(facets, new ParallelOptions() { MaxDegreeOfParallelism = 1} ,facet =>
            {
                var v0 = mesh.GetVertexPoint(facet.V0);
                var v1 = mesh.GetVertexPoint(facet.V1);
                var v2 = mesh.GetVertexPoint(facet.V2);

                var normal = facet.Normal;

                var lightContribution = MathUtils.Clamp(-Vector3.Dot(lightSourceAt.Normalize(), normal.Normalize()), 0, 1);

                var modelV0 = v0.TransformHomogeneus(modelMatrix);
                modelV0 /= modelV0.W;
                var modelV1 = v1.TransformHomogeneus(modelMatrix);
                modelV1 /= modelV1.W;
                var modelV2 = v2.TransformHomogeneus(modelMatrix);
                modelV2 /= modelV2.W;

                var viewV0 = modelV0.Transform(viewMatrix);
                viewV0 /= viewV0.W;
                var viewV1 = modelV1.Transform(viewMatrix);
                viewV1 /= viewV1.W;
                var viewV2 = modelV2.Transform(viewMatrix);
                viewV2 /= viewV2.W;

                var clipV0 = viewV0.Transform(projectionMatrix);
                var ndcV0 = clipV0 / clipV0.W;
                var clipV1 = viewV1.Transform(projectionMatrix);
                var ndcV1 = clipV1 / clipV1.W;
                var clipV2 = viewV2.Transform(projectionMatrix);
                var ndcV2 = clipV2 / clipV2.W;

                var screenV0 = new Vector3((ndcV0.X + 1) * width / 2.0f, (-ndcV0.Y + 1) * height / 2.0f, ndcV0.Z);
                var screenV1 = new Vector3((ndcV1.X + 1) * width / 2.0f, (-ndcV1.Y + 1) * height / 2.0f, ndcV1.Z);
                var screenV2 = new Vector3((ndcV2.X + 1) * width / 2.0f, (-ndcV2.Y + 1) * height / 2.0f, ndcV2.Z);

                if(IsTriangleVisible(width, height, screenV0, screenV1, screenV2))
                    ScanLineTriangle(frameBuffer, screenV0, screenV1, screenV2, lightContribution);
            });

            return frameBuffer.GetFrame();
        }

        public static void ScanLineTriangle(FrameBuffer frameBuffer, Vector3 v0, Vector3 v1, Vector3 v2, float diffuse)
        {
            var (p0, p1, p2) = SortIndices(v0, v1, v2);
            if (p0 == p1 || p1 == p2 || p2 == p0)
                return;

            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, frameBuffer.Height - 1);

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
        private static void ScanLineHalfTriangleBottomFlat(FrameBuffer frameBuffer, int yStart, int yEnd,
            Vector3 anchor, Vector3 vRight, Vector3 vLeft, float diffuse)
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

                ScanSingleLine(frameBuffer, start, end, diffuse);
            }
        }

        // P2
        //   .................P1
        //       ..........
        //          .....
        //            P0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ScanLineHalfTriangleTopFlat(FrameBuffer frameBuffer, int yStart, int yEnd,
            Vector3 anchor, Vector3 vRight, Vector3 vLeft, float diffuse)
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
        private static void ScanSingleLine(FrameBuffer frameBuffer, Vector3 start, Vector3 end, float diffuse)
        {
            var minX = Math.Max(start.X, 0);
            var maxX = Math.Min(end.X, frameBuffer.Width);

            var deltaX = 1 / (end.X - start.X);

            for (var x = minX; x < maxX; x++)
            {
                var gradient = (x - start.X) * deltaX;
                var point = Vector3.Lerp(start, end, gradient);
                var xInt = (int)x;
                var yInt = (int)point.Y;

                frameBuffer.ColorPixel(xInt, yInt, point.Z, Color.FromArgb((int)(255 * diffuse), (int)(255 * diffuse), (int)(255 * diffuse)));
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

        private static bool InInclusiveLowerExclusiveUpper(float x, int lower, int upper)
        {
            return x >= lower && x < upper;
        }

        private static bool IsTriangleVisible(int width, int height, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return InInclusiveLowerExclusiveUpper(v0.X, 0, width) && InInclusiveLowerExclusiveUpper(v0.Y, 0, height) ||
                InInclusiveLowerExclusiveUpper(v1.X, 0, width) && InInclusiveLowerExclusiveUpper(v1.Y, 0, height) ||
                InInclusiveLowerExclusiveUpper(v2.X, 0, width) && InInclusiveLowerExclusiveUpper(v2.Y, 0, height);
        }
    }
}
