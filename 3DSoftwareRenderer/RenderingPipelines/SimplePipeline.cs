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

namespace SoftwareRenderer3D.RenderingPipelines
{
    public class SimplePipeline : IRenderPipeline
    {
        public Bitmap Render(Mesh<IVertex> mesh, IFrameBuffer frameBuffer, ArcBallCamera camera, Texture texture = null)
        {
            if (mesh == null)
                return frameBuffer.GetFrame();

            var width = frameBuffer.GetSize().Width;
            var height = frameBuffer.GetSize().Height;

            var viewMatrix = camera.ViewMatrix;
            var projectionMatrix = camera.ProjectionMatrix;
            var modelMatrix = mesh.ModelMatrix;

            mesh.TransformVertices(width, height, viewMatrix, projectionMatrix);

            var facetIds = mesh.FacetIds.ToList();

            if(Globals.BackfaceCulling)
            {
                var facetsToKeep = new List<int>(mesh.FacetCount);
                var cameraEyePos = camera.EyePosition;
                for (var i = 0; i < facetIds.Count; i++)
                {
                    var faId = facetIds[i];
                    var normal = mesh.GetFacetNormal(faId);
                    var viewingDirection = (mesh.GetFacetMidpoint(faId) - cameraEyePos).Normalize();
                    var angle = Vector3.Dot(viewingDirection, normal);
                    if (angle <= Constants.BackfaceCullingAngleThreshold)
                        facetsToKeep.Add(faId);
                }

                facetIds = facetsToKeep;
            }

            var fragments = ScanLineRasterizer.Rasterize(mesh, width, height, facetIds);

            var lightSources = Globals.LightSources;

            SimpleFragmentShader.BindTexture(texture);
            SimpleFragmentShader.ShadeFragments(frameBuffer, lightSources, fragments);
            SimpleFragmentShader.UnbindTexture();

            return frameBuffer.GetFrame();
        }
    }
}
