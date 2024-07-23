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
    public static class TransparencyRenderer
    {
        public static Bitmap Render(Mesh<IVertex> mesh, IFrameBuffer frameBuffer, ArcBallCamera camera, Texture texture = null)
        {
            if (mesh == null)
                return frameBuffer.GetFrame();

            var peelingBuffer = new DepthPeelingBuffer(frameBuffer.GetSize().Width, frameBuffer.GetSize().Height);

            TexturedScanLineRasterizer.BindTexture(texture);

            var depthPasses = Globals.DepthPeelingPasses;

            var facets = Globals.BackfaceCulling
                ? mesh.GetFacets().Where((x, i) => Vector3.Dot((mesh.GetFacetMidpoint(i) - camera.EyePosition).Normalize(), x.Normal.Normalize()) <= 0.3)
                : mesh.GetFacets();

            for (var i = 0; i < depthPasses; i++)
            {
                RenderPass(mesh, facets, peelingBuffer, camera);
                peelingBuffer.DepthPeel();
            }

            RenderPass(mesh, facets, peelingBuffer, camera);

            TexturedScanLineRasterizer.UnbindTexture();

            return peelingBuffer.GetFrame();
        }

        private static void RenderPass(Mesh<IVertex> mesh, IEnumerable<Facet> facets, DepthPeelingBuffer frameBuffer, ArcBallCamera camera)
        {
            var startTime = DateTime.Now;
            var width = frameBuffer.GetSize().Width;
            var height = frameBuffer.GetSize().Height;

            var viewMatrix = camera.ViewMatrix;
            var projectionMatrix = camera.ProjectionMatrix;

            Matrix4x4.Invert(mesh.ModelMatrix, out var modelMatrix);

            var lightSources = new List<Vector3>()
            {
                new Vector3(0, 100, 100),
            };

            var facetIds = Globals.BackfaceCulling
                ? mesh.FacetIds.Where(faId => Vector3.Dot((mesh.GetFacetMidpoint(faId) - camera.EyePosition).Normalize(), mesh.GetFacetNormal(faId)) <= 0.1)
                : mesh.FacetIds;

            Parallel.ForEach(facetIds, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, facetId =>
            {
                var facet = mesh.GetFacet(facetId);

                var v0 = mesh.GetVertexPoint(facet.V0);
                var v1 = mesh.GetVertexPoint(facet.V1);
                var v2 = mesh.GetVertexPoint(facet.V2);

                var normal = facet.Normal;

                var lightContribution = 0.0f;
                foreach (var lightSource in lightSources)
                {
                    var lightDir = (lightSource - mesh.GetFacetNormal(facetId)).Normalize();
                    lightContribution += MathUtils.Clamp(Vector3.Dot(lightDir, normal.Normalize()).Clamp(), 0, 1);
                }

                lightContribution = lightContribution.Clamp(0, 1);

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

                if (RendererUtils.IsTriangleInFrustum(width, height, screenV0, screenV1, screenV2))
                {
                    if (mesh.GetVertex(facet.V0).GetType().IsAssignableFrom(typeof(TexturedVertex)))
                    {
                        TexturedScanLineRasterizer.ScanLineTriangle(frameBuffer, screenV0, screenV1, screenV2,
                            mesh.GetVertex(facet.V0) as TexturedVertex, mesh.GetVertex(facet.V1) as TexturedVertex, mesh.GetVertex(facet.V2) as TexturedVertex,
                            lightContribution);
                    }
                    else
                    {
                        ScanLineRasterizer.ScanLineTriangle(frameBuffer, screenV0, screenV1, screenV2, lightContribution);
                    }
                }
            });
        }
    }
}
