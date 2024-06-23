using SoftwareRenderer3D.Camera;
using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.FrameBuffers;
using SoftwareRenderer3D.Rasterizers;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.Renderers
{
    public static class SimpleRenderer
    {
        public static Bitmap Render(Mesh<IVertex> mesh, IFrameBuffer frameBuffer, ArcBallCamera camera, Texture texture = null)
        {
            var startTime = DateTime.Now;
            TexturedScanLineRasterizer.BindTexture(texture);

            var width = frameBuffer.GetSize().Width;
            var height = frameBuffer.GetSize().Height;

            var viewMatrix = camera.ViewMatrix;
            var projectionMatrix = camera.ProjectionMatrix;

            Matrix4x4.Invert(mesh.ModelMatrix, out var modelMatrix);

            var lightSourceAt = new Vector3(0, 100, 100);

            startTime = DateTime.Now;
            var facets = Globals.BackfaceCulling
                ? mesh.GetFacets().Where((x, i) => Vector3.Dot((mesh.GetFacetMidpoint(i) - camera.Position).Normalize(), x.Normal.Normalize()) <= 0.1)
                : mesh.GetFacets();

            System.Diagnostics.Debug.WriteLine($"Back-face culling time: {(DateTime.Now - startTime).TotalMilliseconds / 1000.0}");

            startTime = DateTime.Now;
            Parallel.ForEach(facets, new ParallelOptions() { MaxDegreeOfParallelism = 12 }, facet =>
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

                if (RendererUtils.IsTriangleWithinScreen(width, height, screenV0, screenV1, screenV2))
                {
                    if (mesh.GetVertex(facet.V0).GetType().IsAssignableFrom(typeof(TexturedVertex)))
                    {
                        TexturedScanLineRasterizer.ScanLineTriangle(frameBuffer, screenV0, screenV1, screenV2,
                            mesh.GetVertex(facet.V0) as TexturedVertex, mesh.GetVertex(facet.V1) as TexturedVertex, mesh.GetVertex(facet.V2) as TexturedVertex, lightContribution);
                    }
                    else
                    {
                        ScanLineRasterizer.ScanLineTriangle(frameBuffer, screenV0, screenV1, screenV2, lightContribution);
                    }
                }
            });
            System.Diagnostics.Debug.WriteLine($"Rasterization time: {(DateTime.Now - startTime).TotalMilliseconds / 1000.0}");

            TexturedScanLineRasterizer.UnbindTexture();

            return frameBuffer.GetFrame();
        }
    }
}
