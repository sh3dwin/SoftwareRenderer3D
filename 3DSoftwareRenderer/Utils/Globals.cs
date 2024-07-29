using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System.Collections.Generic;
using System.Numerics;

namespace SoftwareRenderer3D.Utils
{
    public static class Globals
    {
        public static Enums.TextureInterpolation TextureInterpolation = Enums.TextureInterpolation.NEAREST;


        private static readonly Dictionary<int, IVertex> testVertices = new Dictionary<int, IVertex>
            {
                {0,  new StandardVertex(new Vector3(-1.0f,  1.0f, 1.0f)) },
                {1,  new StandardVertex(new Vector3(-1.0f, -1.0f, 1.0f)) },
                {2,  new StandardVertex(new Vector3( 1.0f, -1.0f, 1.0f)) },
                {3,  new StandardVertex(new Vector3(-2.0f, -0.0f, 10.0f)) },
                {4,  new StandardVertex(new Vector3(-2.0f, -2.0f, 10.0f)) },
                {5,  new StandardVertex(new Vector3(-0.0f, -2.0f, 10.0f)) },
                {6,  new StandardVertex(new Vector3(-0.0f, -0.0f, 10.0f)) },
                {7,  new StandardVertex(new Vector3(-0.0f, -2.0f, 8.0f)) },
                {8,  new StandardVertex(new Vector3(-0.0f, -2.0f, 10.0f)) },
            };

        public static uint DepthPeelingPasses = 2;
        public static double NormalizedOpacity = 0.5;

        private static readonly Dictionary<int, Facet> testFacets = new Dictionary<int, Facet>
            {
                {0, new Facet(0, 1, 2, Vector3.Cross(Vector3.Normalize(testVertices[2].WorldPoint - testVertices[0].WorldPoint), Vector3.Normalize(testVertices[1].WorldPoint - testVertices[0].WorldPoint))) },
                {1, new Facet(3, 4, 5, Vector3.Cross(Vector3.Normalize(testVertices[5].WorldPoint - testVertices[3].WorldPoint), Vector3.Normalize(testVertices[4].WorldPoint - testVertices[3].WorldPoint))) },
                {2, new Facet(6, 7, 8, Vector3.Cross(Vector3.Normalize(testVertices[8].WorldPoint - testVertices[6].WorldPoint), Vector3.Normalize(testVertices[7].WorldPoint - testVertices[6].WorldPoint))) },
            };

        public static Mesh<IVertex> TestMesh = new Mesh<IVertex>(testVertices, testFacets);

        public static bool BackfaceCulling = false;

        public static List<Vector3> LightSources = new List<Vector3>()
            {
                //new Vector3(0, 100, 100),
                new Vector3(0, -123, 242),
            };

    }
}
