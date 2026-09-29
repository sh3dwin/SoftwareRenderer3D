using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.Fragment;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.Rasterizers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace SoftwareRenderer3D.Tests
{
    [TestClass]
    public class RasterizationTest
    {
        [TestMethod]
        public void TriangleRasterizationTest()
        {
            const int width = 100;
            const int height = 190;

            //  Expected frame
            //     ________
            //     |     o| v2
            //     |    /||
            //     |   / ||
            //     |  /  ||
            //     | /   ||
            //  v0 |o - -o| v1
            //     | \   ||
            //     |  \  ||
            //     |   \ ||
            //     |    \||
            //     |     o| v3
            //     --------

            // Arange
            // Middle left
            var v0 = new StandardVertex(-1.0f, 0.0f, 0.0f, Vector3.UnitZ)
            {
                NDCPosition = new Vector3(-1.0f, 0.0f, -0.5f)
            };
            v0.SetScreenCoordinates(width, height);

            // Middle right
            var v1 = new StandardVertex(0.99f, 0.0f, 0.0f, Vector3.UnitZ)
            {
                NDCPosition = new Vector3(0.99f, 0.0f, -0.5f)
            };
            v1.SetScreenCoordinates(width, height);

            // Top right
            var v2 = new StandardVertex(0.99f, 0.99f, 0.0f, Vector3.UnitZ)
            {
                NDCPosition = new Vector3(0.99f, 1.0f, -0.5f)
            };
            v2.SetScreenCoordinates(width, height);

            // Bottom right
            var v3 = new StandardVertex(0.99f, -0.99f, 0.0f, Vector3.UnitZ)
            {
                NDCPosition = new Vector3(0.99f, -1.0f, -0.5f)
            };
            v3.SetScreenCoordinates(width, height);

            var f1 = new Facet(0, 1, 2, Vector3.UnitZ);
            var f2 = new Facet(0, 1, 3, Vector3.UnitZ);

            var vertices = new Dictionary<int, IVertex>()
            {
                {0, v0}, {1, v1}, {2, v2}, {3, v3},
            };

            Dictionary<int, Facet> facets = new() { { 0, f1 }, { 1, f2 } };
            var mesh = new Mesh<IVertex>(vertices, facets);

            var rasterizer = new ScanLineRasterizer();

            // Act
            var fragments = rasterizer.Rasterize(mesh, width, height, facets.Keys.ToList());

            // Print in debug
#if DEBUG
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    if (fragments.Any(f => f is SimpleFragment simpleFragment && simpleFragment.ScreenCoordinates.Equals(new Vector2(j, i))))
                        Debug.Write("o ");
                    else
                        Debug.Write("- ");
                }
                Debug.Write($"{i} \n");
            }
#endif

            // Assert
            Assert.IsTrue(fragments.Any());

            const int minShadedPixelsCount = (width * height) / 2 + width;
            Assert.IsTrue(fragments.Count > minShadedPixelsCount);
        }
    }
}
