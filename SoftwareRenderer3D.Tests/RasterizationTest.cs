using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareRenderer3D.Camera;
using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.Factories;
using SoftwareRenderer3D.RenderContexts;
using SoftwareRenderer3D.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.Tests
{
    [TestClass]
    public class RasterizationTest
    {
        [TestMethod]
        public void TriangleRasterizationTest()
        {
            var vertexDict = new Dictionary<int, IVertex>()
            {
                { 0, new StandardVertex(-0.1f, 0.01f, -0.5f) },
                { 1, new StandardVertex(0.15f, 1, -0.5f) },
                { 2, new StandardVertex(1, -1, -0.5f) },
            };

            var normal = Vector3.Cross(Vector3.Normalize(vertexDict[1].GetVertexPoint() - vertexDict[0].GetVertexPoint()), Vector3.Normalize(vertexDict[2].GetVertexPoint() - vertexDict[1].GetVertexPoint()));

            var facetDict = new Dictionary<int, Facet>()
            {
                { 0, new Facet(0, 1, 2, normal) }
            };

            var mesh = new Mesh<IVertex>(vertexDict, facetDict);

            var camera = new ArcBallCamera(new Vector3(1, 0, 0));

            var bitmap = new SimpleRenderer(new RenderContext(800, 800)).Render(mesh, camera);

            bitmap.Save("testRasterization.png");
        }
    }
}
