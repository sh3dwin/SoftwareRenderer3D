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
using System.Windows.Media.Media3D;

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

            var width = frameBuffer.GetSize().Width;
            var height = frameBuffer.GetSize().Height;

            var viewMatrix = camera.ViewMatrix;
            var projectionMatrix = camera.ProjectionMatrix;

            Matrix4x4.Invert(mesh.ModelMatrix, out var modelMatrix);

            var vertices = new Dictionary<int, IVertex>(mesh.VertexCount);

            var vertexIds = mesh.VertexIds;

            Parallel.ForEach(vertexIds, vertexId =>
            {
                var vertex = mesh.GetVertex(vertexId);
                var modelV0 = vertex.WorldPoint.TransformHomogeneus(modelMatrix);
                modelV0 /= modelV0.W;

                var viewV0 = modelV0.Transform(viewMatrix);
                viewV0 /= viewV0.W;

                var clipV0 = viewV0.Transform(projectionMatrix);
                var ndcV0 = clipV0 / clipV0.W;

                vertex.NDCPosition = ndcV0.ToVector3();
                vertex.SetScreenCoordinates(width, height);
                vertices[vertexId] = vertex;
            });

            var lightSources = new List<Vector3>()
            {
                new Vector3(0, 100, 100),
                new Vector3(0, -123, -242),
            };

            var facetIds = Globals.BackfaceCulling
                ? mesh.FacetIds.Where(faId => Vector3.Dot((mesh.GetFacetMidpoint(faId) - camera.EyePosition).Normalize(), mesh.GetFacetNormal(faId)) <= 0.1)
                : mesh.FacetIds;

            for (var i = 0; i < depthPasses; i++)
            {
                RenderPass(mesh, facetIds, vertices, lightSources, peelingBuffer, camera);
                peelingBuffer.DepthPeel();
            }

            RenderPass(mesh, facetIds, vertices, lightSources, peelingBuffer, camera);

            TexturedScanLineRasterizer.UnbindTexture();

            return peelingBuffer.GetFrame();
        }

        private static void RenderPass(Mesh<IVertex> mesh, IEnumerable<int> facets, Dictionary<int, IVertex> vertices, List<Vector3> lightSources, DepthPeelingBuffer frameBuffer, ArcBallCamera camera)
        {
            Parallel.ForEach(facets, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, facetId =>
            {
                var facet = mesh.GetFacet(facetId);

                var v0 = mesh.GetVertex(facet.V0);
                var v1 = mesh.GetVertex(facet.V1);
                var v2 = mesh.GetVertex(facet.V2);

                var normal = facet.Normal;

                var lightContribution = 0.0f;
                foreach (var lightSource in lightSources)
                {
                    var lightDir = (lightSource - mesh.GetFacetNormal(facetId)).Normalize();
                    lightContribution += MathUtils.Clamp(Vector3.Dot(lightDir, normal.Normalize()).Clamp(), 0, 1);
                }

                lightContribution = lightContribution.Clamp(0, 1);

                if (RenderUtils.IsTriangleInFrustum(frameBuffer.GetSize().Width, frameBuffer.GetSize().Height, v0.ScreenPosition, v1.ScreenPosition, v2.ScreenPosition))
                {
                    if (mesh.GetVertex(facet.V0).GetType().IsAssignableFrom(typeof(TexturedVertex)))
                        TexturedScanLineRasterizer.ScanLineTriangle(frameBuffer, v0 as TexturedVertex, v1 as TexturedVertex, v2 as TexturedVertex, lightSources);
                    else
                        ScanLineRasterizer.ScanLineTriangle(frameBuffer, v0, v1, v2, lightSources);
                }
            });
        }
    }
}
