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
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SoftwareRenderer3D.Renderers
{
    public static class SimplePipeline
    {
        public static Bitmap Render(Mesh<IVertex> mesh, IFrameBuffer frameBuffer, ArcBallCamera camera, Texture texture = null)
        {
            if (mesh == null)
                return frameBuffer.GetFrame();

            TextureShader.BindTexture(texture);

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

            var fragments = ScanLineRasterizer.Rasterize(mesh, width, height, facetIds);
            if (mesh.Vertices.First().GetType().IsAssignableFrom(typeof(TexturedVertex)))
                TextureShader.ShadeFragments(lightSources, frameBuffer, fragments);
            else
                SimpleFragmentShader.ShadeFragments(frameBuffer, lightSources, fragments);

            TextureShader.UnbindTexture();

            return frameBuffer.GetFrame();
        }
    }
}
