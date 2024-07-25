using SoftwareRenderer3D.Camera;
using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.FragmentShaders;
using SoftwareRenderer3D.FrameBuffers;
using SoftwareRenderer3D.Rasterizers;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SoftwareRenderer3D.Renderers
{
    public static class OrderIndependentTransparencyPipeline
    {
        public static Bitmap Render(Mesh<IVertex> mesh, IFrameBuffer frameBuffer, ArcBallCamera camera, Texture texture = null)
        {
            if (mesh == null)
                return frameBuffer.GetFrame();

            var peelingBuffer = new DepthPeelingBuffer(frameBuffer.GetSize().Width, frameBuffer.GetSize().Height);

            TextureShader.BindTexture(texture);

            var depthPasses = Globals.DepthPeelingPasses;

            var width = frameBuffer.GetSize().Width;
            var height = frameBuffer.GetSize().Height;

            var viewMatrix = camera.ViewMatrix;
            var projectionMatrix = camera.ProjectionMatrix;
            var modelMatrix = mesh.ModelMatrix;

            mesh.TransformVertices(width, height, viewMatrix, projectionMatrix);

            var lightSources = Globals.LightSources;

            var facetIds = Globals.BackfaceCulling
                ? mesh.FacetIds.Where(faId => Vector3.Dot((mesh.GetFacetMidpoint(faId) - camera.EyePosition).Normalize(), mesh.GetFacetNormal(faId)) <= 0.1)
                : mesh.FacetIds;

            for (var i = 0; i < depthPasses; i++)
            {
                RenderPass(mesh, facetIds, lightSources, peelingBuffer);
                peelingBuffer.DepthPeel();
            }

            RenderPass(mesh, facetIds, lightSources, peelingBuffer);

            TextureShader.UnbindTexture();

            return peelingBuffer.GetFrame();
        }

        private static void RenderPass(Mesh<IVertex> mesh, IEnumerable<int> facetIds, List<Vector3> lightSources, DepthPeelingBuffer frameBuffer)
        {
            var width = frameBuffer.GetSize().Width;
            var height = frameBuffer.GetSize().Height;

            var fragments = ScanLineRasterizer.Rasterize(mesh, width, height, facetIds);

            if (mesh.Vertices.First().GetType().IsAssignableFrom(typeof(TexturedVertex)))
                TextureShader.ShadeFragments(lightSources, frameBuffer, fragments);
            else
                SimpleFragmentShader.ShadeFragments(frameBuffer, lightSources, fragments);
        }
    }
}
