using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.RenderContexts;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.Renderers
{
    public class SimpleRenderer
    {
        private RenderContext _renderContext;

        public SimpleRenderer(RenderContext renderContext)
        {
            _renderContext = renderContext;
        }

        public Bitmap Render(Mesh<IVertex> mesh)
        {
            var width = _renderContext.Width;
            var height = _renderContext.Height;

            var viewMatrix = _renderContext.Camera.ViewMatrix;
            var projectionMatrix = _renderContext.Camera.ProjectionMatrix;

            var modelMatrix = Matrix4x4.Transpose(Matrix4x4.CreateTranslation(-mesh.GetCenterOfMass()));

            var lightSourceAt = new Vector3(0, 10, 1);

            var facets = mesh.GetFacets().Where(x => Vector3.Dot(Vector3.Normalize(
                ((mesh.GetVertexPoint(x.V0) + mesh.GetVertexPoint(x.V1) + mesh.GetVertexPoint(x.V2)) / 3.0f) - _renderContext.Camera.Position)
                , Vector3.Normalize(x.Normal)) < 0);

            Parallel.ForEach(facets, new ParallelOptions() { MaxDegreeOfParallelism = 1} ,facet =>
            {
                var v0 = mesh.GetVertexPoint(facet.V0);
                var v1 = mesh.GetVertexPoint(facet.V1);
                var v2 = mesh.GetVertexPoint(facet.V2);

                var normal = facet.Normal;

                var lightContribution = -Vector3.Dot(Vector3.Normalize(lightSourceAt), Vector3.Normalize(normal));

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

                var screenV0 = new Vector3((ndcV0.X + 1) * _renderContext.Width / 2.0f, (-ndcV0.Y + 1) * _renderContext.Height / 2.0f, ndcV0.Z);
                var screenV1 = new Vector3((ndcV1.X + 1) * _renderContext.Width / 2.0f, (-ndcV1.Y + 1) * _renderContext.Height / 2.0f, ndcV1.Z);
                var screenV2 = new Vector3((ndcV2.X + 1) * _renderContext.Width / 2.0f, (-ndcV2.Y + 1) * _renderContext.Height / 2.0f, ndcV2.Z);

                ScanLineTriangle(screenV0, screenV1, screenV2, Math.Abs(lightContribution));
            });

            return _renderContext.GetFrame();
        }

        public void Update(float width, float height, Vector3 previousMouseCoords, Vector3 newMouseCoords)
        {
            _renderContext.Update(width, height, previousMouseCoords, newMouseCoords);
        }

        public void Update(float width, float height)
        {
            _renderContext.Update(width, height);
        }

        internal void Update(Vector3 previousMouseCoords, Vector3 mouseCoords)
        {
            _renderContext.Update(previousMouseCoords, mouseCoords);
        }


        public void ScanLineTriangle(Vector3 v0, Vector3 v1, Vector3 v2, float diffuse)
        {
            var (p0, p1, p2) = SortIndices(v0, v1, v2);
            if (p0 == p1 || p1 == p2 || p2 == p0)
                return;

            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, _renderContext.Height - 1);

            // Out if clipped
            if (yStart > yEnd)
                return;

            var yMiddle = p1.Y.Clamp(yStart, yEnd);

            if (HaveClockwiseOrientation(p0, p1, p2))
            {
                // P0
                //   P1
                // P2
                ScanLineHalfTriangleBottomFlat(yStart, (int)yMiddle - 1, p0, p1, p2, diffuse);
                ScanLineHalfTriangleTopFlat((int)yMiddle, yEnd, p2, p1, p0, diffuse);
            }
            else
            {
                //   P0
                // P1 
                //   P2

                ScanLineHalfTriangleBottomFlat(yStart, (int)yMiddle - 1, p0, p2, p1, diffuse);
                ScanLineHalfTriangleTopFlat((int)yMiddle, yEnd, p2, p0, p1, diffuse);
            }
        }

        //            P0
        //          .....
        //       ..........
        //   .................P1
        // P2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScanLineHalfTriangleBottomFlat(int yStart, int yEnd,
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

                ScanSingleLine(start, end, diffuse);
            }
        }

        // P2
        //   .................P1
        //       ..........
        //          .....
        //            P0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScanLineHalfTriangleTopFlat(int yStart, int yEnd,
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

                ScanSingleLine(start, end, diffuse);
            }
        }

        /// <summary>
        /// Scan line on the x direction
        /// </summary>
        /// <param name="start">Scan line start</param>
        /// <param name="end">Scan line end</param>
        /// <param name="faId">Facet id</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScanSingleLine(Vector3 start, Vector3 end, float diffuse)
        {
            var minX = Math.Max(start.X, 0);
            var maxX = Math.Min(end.X, _renderContext.Width);

            var deltaX = 1 / (end.X - start.X);

            for (var x = minX; x < maxX; x++)
            {
                var gradient = (x - start.X) * deltaX;
                var point = Vector3.Lerp(start, end, gradient);
                var xInt = (int)x;
                var yInt = (int)point.Y;

                _renderContext.ColorPixel(xInt, yInt, point.Z, Color.FromArgb((int)(255 * diffuse), (int)(255 * diffuse), (int)(255 * diffuse)));
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
