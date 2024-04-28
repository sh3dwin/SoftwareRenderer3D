using SoftwareRenderer3D.Camera;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.RenderContexts;
using SoftwareRenderer3D.Utils.LinearAlgebraUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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

        public Bitmap Render(Mesh<StandardVertex> mesh, ArcBallCamera camera)
        {
            var width = _renderContext.Width;
            var height = _renderContext.Height;

            var viewMatrix = camera.LookAt();

            foreach ( var facet in mesh.GetFacets()) {
                var v0 = mesh.GetVertexPoint(facet.V0);
                var v1 = mesh.GetVertexPoint(facet.V1);
                var v2 = mesh.GetVertexPoint(facet.V2);
                var normal = facet.Normal;

                var viewV0 = v0.Transform(viewMatrix);
                var viewV1 = v1.Transform(viewMatrix);
                var viewV2 = v2.Transform(viewMatrix);
                var transformedNormal = normal.Transform(viewMatrix);
            }
        }
    }
}
